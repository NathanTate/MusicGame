import { inject, Injectable } from "@angular/core";
import { environment } from "../../../environments/environment.development";
import { HttpClient, HttpHeaders, HttpParams } from "@angular/common/http";
import { PlaylistResponse } from "../models/playlist/playlistResponse";
import { PlaylistUpdateRequest } from "../models/playlist/updatePlaylistRequest";
import { PlaylistsQuery } from "../models/queries/playlistsQuery";
import { generateHttpParams } from "../../shared/helpers/httpParamsHelper";
import { PlaylistListResponse } from "../models/playlist/playlistListResponse";
import { Subject, tap } from "rxjs";
import { UpsertSongPlaylistRequest } from "../models/playlist/upsertSongPlaylistRequest";

@Injectable({
  providedIn: 'root'
})
export class PlaylistService {
  private readonly _baseUrl = environment.apiUrl + 'playlists/';
  public readonly playlistsQuery = new PlaylistsQuery();
  public readonly playlistUpdated$ = new Subject<number>();

  private http = inject(HttpClient);

  createPlaylist() {
    return this.http.post<PlaylistResponse>(this._baseUrl, null);
  }

  getPlaylists(query: PlaylistsQuery) {
    const httpParams = generateHttpParams(query);
    return this.http.get<PlaylistListResponse>(this._baseUrl, { params: httpParams });
  }

  getPlaylist(playlistId: number) {
    return this.http.get<PlaylistResponse>(this._baseUrl + playlistId);
  }

  updatePlaylist(playlist: PlaylistUpdateRequest) {
    return this.http.put<PlaylistResponse>(this._baseUrl, playlist).pipe(tap(() => {
      this.playlistUpdated$.next(playlist.playlistId);
    }));
  }

  deletePlaylist(playlistId: number) {
    return this.http.delete<void>(this._baseUrl + playlistId);
  }

  upsertPhoto(playingId: number, photo: File) {
    const formData = new FormData();
    formData.append('photo', photo)
    let httpHeaders = new HttpHeaders().set('Content-Type', 'multipart/form-data');

    return this.http.patch<void>(`${this._baseUrl}${playingId}/photo`, formData);
  }

  deletePhoto(playingId: number) {
    return this.http.delete<void>(`${this._baseUrl}${playingId}/photo`);
  }

  addSongToPlaylist(model: UpsertSongPlaylistRequest) {
    let httpParams = new HttpParams().set('position', model.position);

    return this.http.post<void>(this._baseUrl + `${model.playlistId}/songs/${model.songId}`, null, { params: httpParams })
  }

  updateSongPosition(model: UpsertSongPlaylistRequest) {
    let httpParams = new HttpParams().set('position', model.position);

    return this.http.put<void>(this._baseUrl + `${model.playlistId}/songs/${model.songId}`, null, { params: httpParams })
  }

  removeSongFromPlaylist(playlistId: number, songId: number) {
    return this.http.delete<void>(this._baseUrl + `${playlistId}/songs/${songId}`);
  }

  isNameAvailable(name: string) {
    console.log('name: ' + name)
    return this.http.post<boolean>(this._baseUrl + 'nameAvailable', { name });
  }
}