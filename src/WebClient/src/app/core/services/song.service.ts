import { HttpClient, HttpHeaders } from "@angular/common/http";
import { inject, Injectable, OnInit } from "@angular/core";
import { environment } from "../../../environments/environment.development";
import { SongResponse } from "../models/song/songResponse";
import { SongsQuery } from "../models/queries/songsQuery";
import { generateHttpParams } from "../../shared/helpers/httpParamsHelper";
import { SongListResponse } from "../models/song/songListResponse";
import { Subject, tap } from "rxjs";
import { CreateSongRequest } from "../models/song/createSongRequest";
import { UpdateSongRequest } from "../models/song/updateSongRequest";

@Injectable({
  providedIn: 'root'
})
export class SongService {
  private readonly _baseUrl = environment.apiUrl + 'songs/'
  public readonly songsQuery = new SongsQuery();
  public readonly songUpdated$ = new Subject<number>();

  private readonly http = inject(HttpClient);

  getSongs(query: SongsQuery) {
    const httpParams = generateHttpParams(query);
    return this.http.get<SongListResponse>(this._baseUrl, { params: httpParams });
  }

  getSong(songId: number) {
    return this.http.get<SongResponse>(this._baseUrl + songId);
  }

  createSong(model: CreateSongRequest) {
    const httpHeaders = new HttpHeaders().set('Content-Type', 'multipart/form-data');

    return this.http.post<SongResponse>(this._baseUrl, model, { headers: httpHeaders });
  }

  updateSong(model: UpdateSongRequest) {
    return this.http.post<SongResponse>(this._baseUrl, model).pipe(tap(() => {
      this.songUpdated$.next(model.songId);
    }));
  }

  deleteSong(songId: number) {
    return this.http.delete<void>(this._baseUrl + songId);
  }

  uplaodSongPhoto(songId: number, photo: File) {
    return this.http.patch<SongResponse>(this._baseUrl + songId + '/photo', photo);
  }

  deleteSongPhoto(songId: number) {
    return this.http.delete<void>(this._baseUrl + songId + '/photo');
  }

  isNameAvailable(name: string) {
    return this.http.post<boolean>(this._baseUrl + 'nameAvailable', { name });
  }

}