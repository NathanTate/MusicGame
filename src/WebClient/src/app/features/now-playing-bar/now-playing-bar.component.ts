import { Component, DestroyRef, HostListener, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { MenuItem } from 'primeng/api';
import { AudioService } from '../../core/services/audio.service';
import { AsyncPipe, NgIf } from '@angular/common';
import { ContextMenuModule } from 'primeng/contextmenu';
import { DraggableBarComponent } from '../../shared/components/draggable-bar/draggable-bar.component';
import { take } from 'rxjs';
import { PlaybackState, PlaybackStateService } from '../../core/services/playbackState.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-now-playing-bar',
  standalone: true,
  imports: [Button, ContextMenuModule, RouterLink, AsyncPipe, NgIf, DraggableBarComponent],
  templateUrl: './now-playing-bar.component.html',
  styleUrl: './now-playing-bar.component.scss'
})
export class NowPlayingBarComponent implements OnInit {
  public items = signal<MenuItem[] | undefined>(undefined);
  private readonly audioService = inject(AudioService);
  private readonly playbackStateService = inject(PlaybackStateService);
  private readonly destroyRef = inject(DestroyRef);
  readonly state$ = this.audioService.state$;
  playbackState = signal<PlaybackState | undefined>(undefined);

  private get previousVolume() {
    const volumeString = localStorage.getItem("previousVolume");
    const volume = volumeString ? +volumeString : 30;
    return volume;
  }

  @HostListener('document:keyup.Space', ['$event'])
  onSpaceUp(event: Event) {
    const target = event.target as HTMLElement;
    const isInputField = ['INPUT', 'TEXTAREA'].includes(target.tagName) || target.isContentEditable;

    if (isInputField) return;

    this.state$.pipe(take(1)).subscribe((state) => {
      state.playing ? this.onPause() : this.onPlay();
    })
  }

  ngOnInit(): void {
    this.audioService.updateVolume(this.previousVolume);
    this.playbackStateService.state$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((state) => {
      this.playbackState.set(state);
    })

    this.items.set([
      {
        icon: 'pi pi-discord',
        label: 'text-goes-here'
      }
    ])
  }

  onPlayingBarValueChange(value: number) {
    this.audioService.seekTo(value);
  }

  onVolumeValueChange(value: number) {
    this.audioService.updateVolume(value);
    localStorage.setItem('previousVolume', value.toString());
  }

  toggleMute() {
    this.state$.pipe(take(1)).subscribe((state) => {
      state.muted ? this.audioService.updateVolume(this.previousVolume) : this.audioService.updateVolume(0);
    })
  }

  onPlay() {
    this.audioService.play();
  }

  onPause() {
    this.audioService.pause();
  }
}
