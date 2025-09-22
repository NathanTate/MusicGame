import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { PlaylistService } from '../services/playlist.service';
import { PlaylistListResponse } from '../models/playlist/playlistListResponse';


export const playlistsResolver: ResolveFn<PlaylistListResponse> = (route, state) => {
  const playlistService = inject(PlaylistService)

  return playlistService.getPlaylists(playlistService.playlistsQuery);
};
