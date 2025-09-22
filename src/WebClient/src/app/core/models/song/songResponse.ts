import { ArtistResponse } from "../user/artistResponse";
import { GenreResponse } from "../genre/genreResponse";

export interface SongResponse {
  songId: number;
  name: string;
  artistName: string;
  url: string;
  likesCount: number;
  duration: number;
  isPrivate: boolean;
  releaseDate: string;
  createdAt: string;
  genres: GenreResponse[];
  photoUrl?: string;
  artist: ArtistResponse;
  lyrics?: string;
}