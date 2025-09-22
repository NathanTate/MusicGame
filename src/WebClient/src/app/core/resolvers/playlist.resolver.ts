import { ResolveFn } from "@angular/router";
import { PlaylistResponse } from "../models/playlist/playlistResponse";
import { inject } from "@angular/core";
import { PlaylistService } from "../services/playlist.service";
import { EMPTY } from "rxjs";
import { UserService } from "../services/user.service";

export const playlistResolver: ResolveFn<PlaylistResponse> = (route, state) => {
  const id = route.params['id'];
  if (id === '-1') {
    const playlist = inject(UserService).likedSongsPlaylist
    return playlist() || EMPTY;
  }
  return inject(PlaylistService).getPlaylist(id);
}