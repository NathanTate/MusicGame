export interface PlaylistUpdateRequest {
  playlistId: number;
  name: string;
  description: string;
  isPrivate: boolean;
}