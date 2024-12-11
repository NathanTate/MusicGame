import { ResolveFn } from "@angular/router";
import { PlaylistResponse } from "../models/playlist/playlistResponse";
import { inject } from "@angular/core";
import { PlaylistService } from "../services/playlist.service";

export const playlistResolver: ResolveFn<PlaylistResponse> = (route, state) => {
  const id = route.params['id'];
  return inject(PlaylistService).getPlaylist(id);
}