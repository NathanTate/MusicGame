import { inject, Injectable } from "@angular/core";
import { environment } from "../../../environments/environment.development";
import { HttpClient } from "@angular/common/http";
import { PlaylistResponse } from "../models/playlist/playlistResponse";
import { PlaylistUpdateRequest } from "../models/playlist/PlaylistUpdateRequest";

@Injectable({
  providedIn: 'root'
})
export class PlaylistService {
  private _baseUrl = environment.apiUrl + 'playlists/';

  private http = inject(HttpClient);

  createPlaylist() {
    return this.http.post<PlaylistResponse>(this._baseUrl, null);
  }

  getPlaylists() {
    return this.http.get<PlaylistResponse[]>(this._baseUrl);
  }

  getPlaylist(playlistId: number) {
    return this.http.get<PlaylistResponse>(this._baseUrl + playlistId);
  }

  updatePlaylist(playlist: PlaylistUpdateRequest) {
    return this.http.put(this._baseUrl, playlist);
  }

  upsertPhoto(playingId: number, photo: File) {
    return this.http.patch<void>(`${this._baseUrl}${playingId}/photo`, photo);
  }

  deletePhoto(playingId: number) {
    return this.http.delete<void>(`${this._baseUrl}${playingId}/photo`);
  }

  deletePlaylist(playlistId: number) {
    return this.http.delete<void>(this._baseUrl + playlistId);
  }
}