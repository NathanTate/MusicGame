import { ArtistResponse } from "./artistResponse";
import { GenreResponse } from "./genreResponse";

export interface SongResponse {
  songId: number;
  name: string;
  url: string;
  likesCount: number;
  duration: number;
  isPrivate: boolean;
  releaseDate: string;
  createdAt: string; 
  genres: GenreResponse[];
  photoUrl?: string | null;
  artist: ArtistResponse;
}