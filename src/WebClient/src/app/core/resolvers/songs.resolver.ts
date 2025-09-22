import { ResolveFn } from "@angular/router";
import { inject } from "@angular/core";
import { SongService } from "../services/song.service";
import { SongListResponse } from "../models/song/songListResponse";

export const songsResolver: ResolveFn<SongListResponse> = (route, state) => {
  const songService = inject(SongService)

  return songService.getSongs(songService.songsQuery);
}