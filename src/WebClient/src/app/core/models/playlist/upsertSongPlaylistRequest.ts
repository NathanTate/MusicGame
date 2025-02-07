export interface UpsertSongPlaylistRequest {
  songId: number;
  playlistId: number;
  position?: number;
}