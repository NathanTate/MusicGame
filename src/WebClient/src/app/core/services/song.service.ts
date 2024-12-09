import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { environment } from "../../../environments/environment.development";
import { SongResponse } from "../models/songResponse";

@Injectable({
  providedIn: 'root'
})
export class SongService {
  private _baseUrl = environment.apiUrl + 'songs/'

  private http = inject(HttpClient)

  getSongs() {
    return this.http.get<SongResponse[]>(this._baseUrl);
  }

  getSong(songId: number) {
    return this.http.get<SongResponse>(this._baseUrl + songId);
  }
}