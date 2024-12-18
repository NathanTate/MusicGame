import { Component, DestroyRef, ElementRef, inject, OnInit, signal, viewChild } from '@angular/core';
import { PlaylistResponse, PlaylistSongResponse } from '../../core/models/playlist/playlistResponse';
import { ActivatedRoute } from '@angular/router';
import { TableModule } from 'primeng/table';
import { Button } from 'primeng/button';
import { AuthService } from '../../auth/auth.service';
import { AsyncPipe, DatePipe, NgIf } from '@angular/common';
import { FormatDurationPipe } from '../../shared/pipes/format-duration.pipe';
import { SongResponse } from '../../core/models/song/songResponse';
import { AudioService } from '../../core/services/audio.service';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PlaybackState, PlaybackStateService } from '../../core/services/playbackState.service';

@Component({
  selector: 'app-playlist',
  standalone: true,
  imports: [TableModule, Button, DatePipe, FormatDurationPipe, IconFieldModule, InputIconModule, InputTextModule, NgIf, AsyncPipe, MenuModule],
  templateUrl: './playlist.component.html',
  styleUrl: './playlist.component.scss'
})
export class PlaylistComponent implements OnInit {
  private readonly activateRoute = inject(ActivatedRoute);
  public readonly authService = inject(AuthService);
  private readonly audioService = inject(AudioService);
  private readonly playbackStateService = inject(PlaybackStateService);
  private readonly destroyRef = inject(DestroyRef);

  playlistSignal = signal<PlaylistResponse | undefined>(undefined);
  playbackState = signal<PlaybackState | undefined>(undefined);
  contentHeaderEl = viewChild.required<ElementRef<HTMLElement>>('contentHeader');
  animationFrameRequested = false;
  items = signal<MenuItem[] | undefined>(undefined);
  repeat = signal<boolean>(false);
  playlistPlayed = signal<boolean>(false);
  randomColors: string[] = [
    '80, 56, 160',
    '16, 208, 240',
    '160, 32, 24',
    '224, 32, 112'
  ]
  randomColor = signal<string>('white');
  sizes = signal<unknown[]>([]);

  public readonly state$ = this.audioService.state$;

  get contentHeader() {
    return this.contentHeaderEl().nativeElement;
  }

  ngOnInit(): void {
    this.playbackStateService.state$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((state) => {
      this.playbackState.set(state);
    })

    this.activateRoute.data.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((data) => {
      this.playlistSignal.set(data['playlist']);
      this.playlistPlayed.set(false)
      this.randomColor.set(this.randomColors[Math.floor(Math.random() * this.randomColors.length)]);
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
      this.setSong(playlist.songs[0].song, play);
    }
  }

  setSong(song: SongResponse, play: boolean = true) {
    this.playbackStateService.setCurrentSong(song);
    this.playbackStateService.setCurrentPlaylist(this.playlistSignal());
    this.audioService.playStream(song, play).subscribe((event) => {
      if (event.type === 'ended') {
        const nextSongId = this.hasNextSong();
        nextSongId !== -1
          ? this.setSong(this.playlistSignal()!.songs[nextSongId + 1].song)
          : this.repeat() ? this.setDefaultSong(true) : '';
      }
    });
  }

  hasNextSong(): number {
    const playlist = this.playlistSignal();
    const song = this.playbackState()?.song;
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
    this.playlistPlayed() ? this.audioService.play() : this.setDefaultSong(true);
    this.playlistPlayed.set(true);
  }

  onPlay(song: SongResponse | null) {
    if (song === null) return;

    if (this.playbackState()?.playlistId === this.playlistSignal()?.playlistId && song.songId === this.playbackState()?.song?.songId) {
      this.audioService.play()
    } else {
      this.playbackStateService.setCurrentPlaylist(this.playlistSignal());
      this.setSong(song);
    }
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
