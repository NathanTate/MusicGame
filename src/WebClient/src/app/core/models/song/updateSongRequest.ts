export interface UpdateSongRequest {
  songId: number;
  name: string;
  isPrivate: boolean;
  releaseDate: Date;
  genreIds: number[];
}