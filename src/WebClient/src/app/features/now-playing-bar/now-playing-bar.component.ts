import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { ProgressBarModule } from 'primeng/progressbar';
import { SongResponse } from '../../core/models/songResponse';
import { SongService } from '../../core/services/song.service';
import { MenuItem } from 'primeng/api';
import { AudioService } from '../../core/services/audio.service';

@Component({
  selector: 'app-now-playing-bar',
  standalone: true,
  imports: [Button, MenuModule, RouterLink, ProgressBarModule],
  templateUrl: './now-playing-bar.component.html',
  styleUrl: './now-playing-bar.component.scss'
})
export class NowPlayingBarComponent implements OnInit {
  song = signal<SongResponse | null>(null);
  items = signal<MenuItem[] | undefined>(undefined);
  isPlaying: boolean = false;

  private songService = inject(SongService);
  private audioService = inject(AudioService);

  ngOnInit(): void {
    this.items.set([
      {
        icon: 'pi pi-discord',
        label: 'text-goes-here'
      }
    ])
    this.getSong(2);
  }

  playSong() {
    this.audioService.play();
  }

  onPlayStop() {
    if (this.isPlaying) {
      this.audioService.pause();
      this.isPlaying = false;
    } else {
      this.audioService.play();
      this.isPlaying = true;
    }
  }

  getSong(songId: number) {
    this.songService.getSong(songId).subscribe((song) => {
      this.audioService.setTrack(song, true);
      this.isPlaying = true;
      this.song.set(song);
    })
  }
}
