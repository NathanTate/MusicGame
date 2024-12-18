import { inject, Injectable } from "@angular/core";
import { BehaviorSubject, switchMap, take, tap } from "rxjs";
import { PlaylistResponse } from "../models/playlist/playlistResponse";
import { SongResponse } from "../models/song/songResponse";
import { SongService } from "./song.service";
import { PlaylistService } from "./playlist.service";
import { AudioService } from "./audio.service";
import { AudioState } from "../models/audioState";

@Injectable({
  providedIn: 'root'
})
export class PlaybackStateService {
  private readonly playbackState = 'playbackState';
  private readonly songService = inject(SongService);
  private readonly playlistService = inject(PlaylistService);
  private readonly audioService = inject(AudioService);

  private state: PlaybackState = {
    song: undefined,
    playlist: undefined,
    playlistId: undefined,
    currentTime: 0,
    volume: 30,
    repeat: false
  }

  private readonly stateChange = new BehaviorSubject<PlaybackState>(this.state);
  public readonly state$ = this.stateChange.asObservable();

  saveState() {
    this.audioService.state$.pipe(take(1)).subscribe((state: AudioState) => {
      this.state.currentTime = state.currentTime;
      this.state.volume = state.volume;
      this.state.playlist = undefined;
      this.setPlaybackState(this.state);
    })
  }

  repeat(mode: boolean) {
    this.state.repeat = mode;
    this.setPlaybackState(this.state);
  }

  setCurrentPlaylist(playlist?: PlaylistResponse) {
    this.state.playlist = playlist;
    this.state.playlistId = playlist?.playlistId;
    this.setPlaybackState(this.state);
  }

  setCurrentSong(song: SongResponse) {
    this.state.song = song;
    this.setPlaybackState(this.state);
  }

  private setPlaylistFromLocalStorage() {
    const playlistId = this.state.playlistId;
    if (!playlistId) return;
    this.playlistService.getPlaylist(playlistId).subscribe({
      next: (playlist: PlaylistResponse) => this.setCurrentPlaylist(playlist),
      error: () => {
        this.state.playlistId = undefined;
        this.setPlaybackState(this.state);
      }
    })
  }

  private setTrackFromLocalStorage() {
    const songId = this.state.song?.songId;
    if (!songId) return;
    this.songService.getSong(songId).pipe(
      switchMap((song: SongResponse) => {
        this.setCurrentSong(song);
        this.audioService.seekTo(this.state.currentTime);
        return this.audioService.playStream(song, false)
      })
    ).subscribe({
      error: () => {
        this.state.song = undefined;
        this.setPlaybackState(this.state);
      }
    });
  }


  initialize(): void {
    const stateLC = localStorage.getItem(this.playbackState);
    if (stateLC) {
      this.state = JSON.parse(stateLC);
      this.setTrackFromLocalStorage();
      this.setPlaylistFromLocalStorage();
    }
  }

  resetState() {
    this.state = {
      song: undefined,
      playlist: undefined,
      playlistId: undefined,
      currentTime: 0,
      volume: 30,
      repeat: false
    }

    this.setPlaybackState(this.state);
  }

  setPlaybackState(state: PlaybackState) {
    this.stateChange.next(state);
    localStorage.setItem(this.playbackState, JSON.stringify(this.state));
  }

}

export interface PlaybackState {
  song?: SongResponse;
  playlist?: PlaylistResponse;
  playlistId?: number;
  currentTime: number;
  volume: number;
  repeat: boolean;
}