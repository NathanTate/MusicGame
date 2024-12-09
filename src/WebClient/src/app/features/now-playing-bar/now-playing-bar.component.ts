import { Component, HostListener, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { SongResponse } from '../../core/models/songResponse';
import { SongService } from '../../core/services/song.service';
import { MenuItem } from 'primeng/api';
import { AudioService } from '../../core/services/audio.service';
import { AsyncPipe, NgIf } from '@angular/common';
import { ContextMenuModule } from 'primeng/contextmenu';
import { DraggableBarComponent } from '../../shared/components/draggable-bar/draggable-bar.component';
import { take } from 'rxjs';

@Component({
  selector: 'app-now-playing-bar',
  standalone: true,
  imports: [Button, ContextMenuModule, RouterLink, AsyncPipe, NgIf, DraggableBarComponent],
  templateUrl: './now-playing-bar.component.html',
  styleUrl: './now-playing-bar.component.scss'
})
export class NowPlayingBarComponent implements OnInit {
  items = signal<MenuItem[] | undefined>(undefined);
  private songService = inject(SongService);
  private audioService = inject(AudioService);
  state$ = this.audioService.state$;

  @HostListener('document:keyup.Space')
  onSpaceUp() {
    this.state$.pipe(take(1)).subscribe((state) => {
      state.playing ? this.onPause() : this.onPlay();
    })
  }

  ngOnInit(): void {
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

  onPlay() {
    this.audioService.play();
  }

  onPause() {
    this.audioService.pause();
  }

  getSong(songId: number) {
    
  }
}
