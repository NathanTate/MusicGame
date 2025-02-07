import { PlaylistResponse } from "./playlist/playlistResponse";
import { SongResponse } from "./song/songResponse";

export interface AudioState {
  song?: SongResponse;
  playlist?: PlaylistResponse;
  repeat: boolean;
  shuffle: boolean;
  playing: boolean;
  ended: boolean;
  readableCurrentTime: string;
  readableDuration: string;
  duration: number;
  currentTime: number;
  volume: number;
  muted: boolean;
  canPlay: boolean;
  error: boolean;
}