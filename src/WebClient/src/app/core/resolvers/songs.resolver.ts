import { ResolveFn } from "@angular/router";
import { SongResponse } from "../models/songResponse";
import { inject } from "@angular/core";
import { SongService } from "../services/song.service";

export const songsResolver: ResolveFn<SongResponse[]> = (route, state) => {
  return inject(SongService).getSongs();
}