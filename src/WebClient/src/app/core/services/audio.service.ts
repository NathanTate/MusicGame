import { inject, Injectable } from "@angular/core";
import { BehaviorSubject, Observable, Subject, takeUntil, tap, throttleTime } from "rxjs";
import { AudioState } from "../models/audioState";
import { SongResponse } from "../models/song/songResponse";
import { PlaylistResponse } from "../models/playlist/playlistResponse";
import { SongService } from "./song.service";
import { PlaylistService } from "./playlist.service";
import { formatDuration } from "../../shared/helpers/format-duration";


@Injectable({
  providedIn: 'root'
})
export class AudioService {
  private readonly songService = inject(SongService);
  private readonly playlistService = inject(PlaylistService);

  private audioEvents = [
    "ended",
    "error",
    "play",
    "playing",
    "pause",
    "timeupdate",
    "canplay",
    "volumechange",
    "loadedmetadata",
    "loadstart"
  ]

  private state: AudioState = {
    song: undefined,
    playlist: undefined,
    repeat: false,
    shuffle: false,
    playing: false,
    ended: false,
    readableDuration: '',
    readableCurrentTime: '',
    duration: 0,
    currentTime: 0,
    volume: 30,
    muted: false,
    canPlay: false,
    error: false
  }

  private stateChange = new BehaviorSubject(this.state);
  public state$ = this.stateChange.asObservable().pipe();
  private stop$ = new Subject<void>();
  private _audio = new Audio();

  playStream(song: SongResponse, playlist?: PlaylistResponse, play: boolean = true) {
    this.stop();
    return this.streamObservable(song, playlist, play).pipe(takeUntil(this.stop$));
  }

  play() {
    this._audio.play();
  }

  pause() {
    this._audio.pause();
  }

  stop() {
    this.stop$.next();
  }

  seekTo(seconds: number) {
    this._audio.currentTime = seconds;
  }

  updateVolume(volume: number) {
    if (volume > 100) {
      volume = 100;
    } else if (volume <= 0) {
      volume = 0;
      this.state.muted = true;
    } else {
      this.state.muted = false;
    }
    this._audio.volume = volume / 100;
  }

  toggleRepeat() {
    this.state.repeat = !this.state.repeat;
  }

  toggleShuffle() {
    this.state.shuffle = !this.state.shuffle;
  }

  private updateStateEvents(event: Event): void {
    switch (event.type) {
      case 'canplay':
        this.state.duration = Math.round(this._audio.duration * 1000);
        this.state.readableDuration = formatDuration(this.state.duration);
        this.state.canPlay = true;
        break;
      case 'playing':
        this.state.playing = true;
        break;
      case 'pause':
        this.state.playing = false;
        break;
      case 'timeupdate':
        this.state.currentTime = this._audio.currentTime * 1000;
        this.state.readableCurrentTime = formatDuration(this.state.currentTime);
        break;
      case 'volumechange':
        this.state.volume = this._audio.volume * 100;
        break;
      case 'ended':
        this.state.ended = true
        this.state.playing = false;
        if (this.state.shuffle) {
          this.playRandomSong();
        }
        break;
      case 'error':
        this.resetState();
        this.state.error = true;
        break;
    }

    this.stateChange.next(this.state);
  }

  private resetState() {
    this.state = {
      song: undefined,
      playlist: undefined,
      repeat: this.state.repeat ?? false,
      shuffle: this.state.shuffle ?? false,
      playing: false,
      ended: false,
      readableDuration: '',
      readableCurrentTime: '',
      duration: 0,
      currentTime: 0,
      volume: 30,
      muted: false,
      canPlay: false,
      error: false
    }
  }

  public saveState() {
    localStorage.setItem("playlistId", this.state.playlist?.playlistId.toString() ?? '');
    localStorage.setItem("songId", this.state.song?.songId.toString() ?? '');
    localStorage.setItem("songTime", (this.state.currentTime / 1000).toString());
    localStorage.setItem("repeat", this.state.repeat ? 'true' : 'false');
    localStorage.setItem("volume", this.state.volume.toString());
  }

  private streamObservable(song: SongResponse, playlist?: PlaylistResponse, play: boolean = true): Observable<Event> {
    return new Observable(subscriber => {
      this._audio.src = song.url;
      this._audio.load();

      if (play) {
        const playPromise = this._audio.play();

        if (playPromise !== undefined) {
          playPromise.then(_ => {

          }).catch(error => {
            this.state.playing = false;
          })
        }
      }

      this.state.song = song;
      this.state.playlist = playlist;

      const handler = (event: Event) => {
        this.updateStateEvents(event);
        subscriber.next(event);
      }

      this.addEvents(this._audio, this.audioEvents, handler);
      return () => {
        this._audio.pause();
        this._audio.currentTime = 0;

        this.removeEvents(this._audio, this.audioEvents, handler);
        this.resetState();
      }
    })
  }

  private addEvents(obj: HTMLAudioElement, events: string[], handler: EventListenerOrEventListenerObject) {
    events.forEach(event => {
      obj.addEventListener(event, handler);
    })
  }

  private removeEvents(obj: HTMLAudioElement, events: string[], handler: EventListenerOrEventListenerObject) {
    events.forEach(event => {
      obj.removeEventListener(event, handler);
    })
  }

  private playRandomSong() {
    if (!this.state.playlist || !this.state.song) {
      return;
    }

    const randomSongIndex = Math.floor(Math.random() * this.state.playlist.songs.length);
    const nextSong = this.state.playlist.songs[randomSongIndex].song;

    this.playStream(nextSong, this.state.playlist, true).subscribe();

    if (this.state.repeat) this.state.repeat = false;
  }

  private setPlaylistFromLocalStorage() {
    const playlistId = localStorage.getItem('playlistId');
    if (!playlistId) return;
    this.playlistService.getPlaylist(+playlistId).subscribe({
      next: (playlist: PlaylistResponse) => {
        this.state.playlist = playlist;
        this.stateChange.next(this.state);
      },
      error: () => localStorage.removeItem('playlistId')
    })
  }

  public setSongFromLocalStorage() {
    const songId = localStorage.getItem('songId');
    const songTime = localStorage.getItem('songTime');
    if (!songId) return;
    this.songService.getSong(+songId).subscribe({
      next: (song: SongResponse) => {
        this.playStream(song, this.state.playlist, false).subscribe();
        this.seekTo(songTime ? +songTime : 0)
        this.setPlaylistFromLocalStorage();
      },
      error: () => localStorage.removeItem('songId')
    })
  }
}