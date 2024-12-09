import { Component, ElementRef, inject, OnInit, signal, viewChild } from '@angular/core';
import { PlaylistResponse, PlaylistSongResponse } from '../../core/models/playlist/playlistResponse';
import { ActivatedRoute } from '@angular/router';
import { TableModule } from 'primeng/table';
import { Button } from 'primeng/button';
import { AuthService } from '../../auth/auth.service';
import { AsyncPipe, DatePipe, NgIf } from '@angular/common';
import { FormatDurationPipe } from '../../shared/pipes/format-duration.pipe';
import { SongResponse } from '../../core/models/songResponse';
import { AudioService } from '../../core/services/audio.service';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { Subscription, take } from 'rxjs';

@Component({
  selector: 'app-playlist',
  standalone: true,
  imports: [TableModule, Button, DatePipe, FormatDurationPipe, IconFieldModule, InputIconModule, InputTextModule, NgIf, AsyncPipe, MenuModule],
  templateUrl: './playlist.component.html',
  styleUrl: './playlist.component.scss'
})
export class PlaylistComponent implements OnInit {
  playlistSignal = signal<PlaylistResponse | null>(null);
  currentSong = signal<SongResponse | null>(null);
  contentHeaderEl = viewChild.required<ElementRef<HTMLElement>>('contentHeader');
  animationFrameRequested = false;
  items = signal<MenuItem[] | undefined>(undefined);
  currentStreamSub: Subscription | undefined;
  repeat = signal<boolean>(false);
  playlistPlayed = signal<boolean>(false);
  randomColors: string[] = [
    'rgb(80, 56, 160)',
    'rgb(16, 208, 240)',
    'rgb(160, 32, 24)',
    'rgb(224, 32, 112)'
  ]
  randomColor = signal<string>('white');
  sizes = signal<unknown[]>([]);

  private activateRoute = inject(ActivatedRoute);
  authService = inject(AuthService);
  private audioService = inject(AudioService);
  state$ = this.audioService.state$;

  get contentHeader() {
    return this.contentHeaderEl().nativeElement;
  }

  ngOnInit(): void {
    this.randomColor.set(this.randomColors[Math.floor(Math.random() * this.randomColors.length)]);
    this.activateRoute.data.subscribe((data) => {
      this.playlistSignal.set(data['playlist'])
      this.playlistPlayed.set(false)
      this.state$.pipe(take(1)).subscribe(state => {
        if (!state.song) {
          this.setDefaultSong();
        }
      })
      console.log(this.playlistSignal()?.user.email);
    })
    this.items.set([
      {
        label: 'View as',
        items: [
          {
            label: 'Compact',
            icon: 'pi pi-bars'
          },
          {
            label: 'List',
            icon: 'pi pi-list'
          }
        ]
      }
    ])
  }

  trackByFn(index: number, plSong: PlaylistSongResponse) {
    plSong.song.songId;
  }

  setDefaultSong(play: boolean = false) {
    const playlist = this.playlistSignal();
    if (playlist && playlist?.songs.length > 0) {
      this.setSong(playlist.songs[0].song, play)
    }
  }

  setSong(song: SongResponse, play: boolean = true) {
    this.currentSong.set(song);
    this.audioService.playStream(song, play).subscribe((event) => {
      if (event.type === 'ended') {
        this.hasNextSong() !== -1
          ? this.setSong(this.playlistSignal()!.songs[this.hasNextSong() + 1].song)
          : this.repeat() ? this.setDefaultSong(true) : '';
      }
    });
  }

  hasNextSong(): number {
    const playlist = this.playlistSignal();
    const song = this.currentSong();
    if (playlist && song) {
      const songIndex = playlist.songs.map(s => s.song.songId).indexOf(song?.songId);
      return playlist.songs.length > songIndex + 1 ? songIndex : -1;
    }
    return -1;
  }

  onPause() {
    this.audioService.pause();
  }

  onPlaylistPlay() {
    this.playlistPlayed() ? this.audioService.play() : this.setDefaultSong();
    this.playlistPlayed.set(true);
  }

  onPlay(song: SongResponse | null) {
    if (song === null) return;
    song.songId === this.currentSong()?.songId ? this.audioService.play() : this.setSong(song);
  }

  onScroll(event: Event) {
    const element = event.target as HTMLElement;

    if (!this.animationFrameRequested) {
      window.requestAnimationFrame(() => {
        this.updateStylesOnScroll(element)
        this.animationFrameRequested = false;
      })
    } else {
      this.animationFrameRequested = true;
    }
  }

  updateStylesOnScroll(element: HTMLElement) {
    const scrollTop = element.scrollTop;
    const headerHeight = this.contentHeader.clientHeight;
    let opacity = 0;
    let contentOpacity = 0;
    const startChange = headerHeight - 70;
    const endChange = headerHeight - 30;

    if (scrollTop <= headerHeight - 50) {
      opacity = 0;
      contentOpacity = 0;
    } else if (scrollTop >= headerHeight - 50 && scrollTop <= headerHeight) {
      opacity = (scrollTop - startChange) / (endChange - startChange);
      contentOpacity = 1
    } else if (scrollTop > headerHeight) {
      opacity = 1;
      contentOpacity = 1
    }
    element.style.setProperty('--header-content-opacity', contentOpacity.toString());
    element.style.setProperty('--header-opacity', opacity.toString());
  }

}
