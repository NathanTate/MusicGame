import { GenreListResponse } from "./genre/genreListResponse";
import { PlaylistListResponse } from "./playlist/playlistListResponse";
import { SongListResponse } from "./song/songListResponse";
import { UserListResponse } from "./user/userListResponse";

export interface SearchData {
  bestFitType: string;
  bestFitItem: unknown;
  songs: SongListResponse;
  playlists: PlaylistListResponse;
  genres: GenreListResponse;
  users: UserListResponse;
  hasItems: boolean;
}