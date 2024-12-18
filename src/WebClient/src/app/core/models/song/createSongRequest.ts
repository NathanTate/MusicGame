export interface CreateSongRequest {
  name: string;
  duration: number;
  isPrivate: boolean;
  releaseDate: Date;
  songFile: File;
  genreIds: number[];
}