export interface AudioState {
  playing: boolean;
  ended: boolean;
  redableCurrentTime: string;
  readableDuration: string;
  duration: number;
  currentTime: number;
  volume: number;
  muted: boolean;
  canPlay: boolean;
  error: boolean;
}