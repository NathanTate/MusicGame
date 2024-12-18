import { Component, computed, inject, input, OnInit, output, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SongResponse } from '../../../core/models/song/songResponse';
import { PlaybackState, PlaybackStateService } from '../../../core/services/playbackState.service';
import { AudioService } from '../../../core/services/audio.service';
import { take } from 'rxjs';

@Component({
  selector: 'app-track-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './track-card.component.html',
  styleUrl: './track-card.component.scss'
})
export class TrackCardComponent {
  private readonly playbackStateService = inject(PlaybackStateService);
  private readonly audioService = inject(AudioService);

  showTitle = input<boolean>(true);
  song = input.required<SongResponse>();
  labelById = computed(() => `x-card-title-${this.song().songId}`)
  isPlaying = signal<boolean>(false);
  playbackState = signal<PlaybackState | undefined>(undefined);

  OnPlay(e: Event) {
    e.stopPropagation();
    this.playbackStateService.setCurrentPlaylist(undefined);
    this.playbackStateService.state$.pipe(take(1)).subscribe((state: PlaybackState) => this.playbackState.set(state));
    if (this.playbackState()?.song?.songId === this.song().songId) {
      this.audioService.play();
    } else {
      this.audioService.playStream(this.song(), true).subscribe();
      this.playbackStateService.setCurrentSong(this.song());
    }
    this.isPlaying.set(true)
  }

  onPause(e: Event) {
    e.stopPropagation();
    this.audioService.pause();
    this.isPlaying.set(false);
  }
}
