import { ArtistResponse } from "../user/artistResponse";
import { SongResponse } from "../song/songResponse";


export interface PlaylistResponse {
  playlistId: number;
  name: string;
  description?: string;
  isPrivate: boolean;
  totalDuration: number;
  songsCount: number;
  likesCount: number;
  createdAt: string;
  songs: PlaylistSongResponse[];
  photoUrl?: string;
  user: ArtistResponse;
}

export interface PlaylistSongResponse {
  position: number;
  song: SongResponse;
}