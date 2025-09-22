import { Component, computed, HostListener, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SongResponse } from '../../../core/models/song/songResponse';
import { AudioService } from '../../../core/services/audio.service';
import { distinctUntilChanged, map, take } from 'rxjs';
import { AudioState } from '../../../core/models/audioState';
import { AsyncPipe } from '@angular/common';
import { isNullOrUndefined } from 'is-what';
import { SongContextService } from '../../../core/services/context-menu.service';

@Component({
  selector: 'app-track-card',
  standalone: true,
  imports: [RouterLink, AsyncPipe],
  templateUrl: './track-card.component.html',
  styleUrl: './track-card.component.scss'
})
export class TrackCardComponent {
  private readonly audioService = inject(AudioService);
  private readonly songContextService = inject(SongContextService);

  showTitle = input<boolean>(true);
  rounded = input<boolean>(false);
  song = input.required<SongResponse>();
  labelById = computed(() => `x-card-title-${this.song().songId}`)
  public readonly isPlaying$ = this.audioService.state$.pipe(map((state: AudioState) => {
    return state.playing && state.song?.songId === this.song().songId && isNullOrUndefined(state.playlist);
  }), distinctUntilChanged())

  @HostListener('contextmenu', ['$event'])
  contextMenu(event: MouseEvent) {
    this.onContextMenu(event, this.song());
  }

  onContextMenu(event: MouseEvent, song: SongResponse) {
    this.songContextService.open(event, song);
  }

  OnPlay(e: Event) {
    e.stopPropagation();
    this.audioService.state$.pipe(take(1)).subscribe((state: AudioState) => {
      state.song?.songId === this.song().songId && isNullOrUndefined(state.playlist)
        ? this.audioService.play()
        : this.audioService.playStream(this.song(), undefined, true).subscribe();
    })
  }

  onPause(e: Event) {
    e.stopPropagation();
    this.audioService.pause();
  }
}
