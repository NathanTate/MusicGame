import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { PlaylistService } from '../services/playlist.service';
import { PlaylistResponse } from '../models/playlist/playlistResponse';


export const playlistsResolver: ResolveFn<PlaylistResponse[]> = (route, state) => {
  return inject(PlaylistService).getPlaylists();
};
