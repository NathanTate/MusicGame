import { Injectable } from "@angular/core";
import { BehaviorSubject, Observable, Subject, takeUntil } from "rxjs";
import { AudioState } from "../models/audioState";
import { SongResponse } from "../models/songResponse";


@Injectable({
  providedIn: 'root'
})
export class AudioService {
  audioEvents = [
    "ended",
    "error",
    "play",
    "playing",
    "pause",
    "timeupdate",
    "canplay",
    "loadedmetadata",
    "loadstart"
  ]

  private state: AudioState = {
    song: undefined,
    playing: false,
    ended: false,
    readableDuration: '',
    redableCurrentTime: '',
    duration: 0,
    currentTime: 0,
    canPlay: false,
    error: false
  }

  private stateChange = new BehaviorSubject(this.state);
  state$ = this.stateChange.asObservable();
  private stop$ = new Subject<void>();
  private _audio = new Audio();

  playStream(song: SongResponse, play: boolean = true) {
    this._audio.volume = 0.15;
    this.stop();
    return this.streamObservable(song, play).pipe(takeUntil(this.stop$));
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

  formatTime(time: number) {
    const dateObj = new Date(time * 1000);
    const hours = dateObj.getUTCHours();
    const minutes = dateObj.getUTCMinutes();
    const seconds = dateObj.getSeconds();
    const formattedTime = (hours ? hours.toString().padStart(2, '0') + ':' : '') +
      minutes.toString().padStart(2, '0') + ':' +
      seconds.toString().padStart(2, '0');

    return formattedTime;
  }

  private updateStateEvents(event: Event): void {
    switch (event.type) {
      case 'canplay':
        this.state.duration = this._audio.duration;
        this.state.readableDuration = this.formatTime(this.state.duration);
        this.state.canPlay = true;
        break;
      case 'playing':
        this.state.playing = true;
        break;
      case 'pause':
        this.state.playing = false;
        break;
      case 'timeupdate':
        this.state.currentTime = this._audio.currentTime;
        this.state.redableCurrentTime = this.formatTime(this.state.currentTime);
        break;
      case 'ended':
        this.state.ended = true
        this.state.playing = false;
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
      playing: false,
      ended: false,
      readableDuration: '',
      redableCurrentTime: '',
      duration: 0,
      currentTime: 0,
      canPlay: false,
      error: false
    }
  }

  private streamObservable(song: SongResponse, play: boolean = true): Observable<Event> {
    return new Observable(subscriber => {
      this._audio.src = song.url;
      this._audio.load();
      this.state.song = song;
      if (play) {
        this._audio.play();
      }

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
}