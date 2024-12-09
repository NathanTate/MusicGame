import { SongResponse } from "./songResponse";

export interface AudioState {
  song: SongResponse | undefined;
  playing: boolean;
  ended: boolean;
  redableCurrentTime: string;
  readableDuration: string;
  duration: number;
  currentTime: number;
  canPlay: boolean;
  error: boolean;
}