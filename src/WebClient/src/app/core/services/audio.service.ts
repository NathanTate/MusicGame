import { Injectable, signal } from "@angular/core";
import { SongResponse } from "../models/songResponse";

@Injectable({
  providedIn: 'root'
})
export class AudioService {
  private _audio = new Audio();
  currentSong = signal<SongResponse | null>(null);

  initialize(audioEl: HTMLAudioElement) {
    this._audio = audioEl;
  }

  play(from: number = -1) {
    if (from !== -1) {
      this._audio.currentTime = this._audio.duration < from ? 0 : from;
    }
    this._audio.play();
  }

  pause() {
    this._audio.pause();
  }

  setTrack(song: SongResponse, play: boolean = true) {
    this.currentSong.set(song);
    this._audio.src = song.url;
    if (play) {
      this.play();
    }
  }

  setTime() {

  }
}