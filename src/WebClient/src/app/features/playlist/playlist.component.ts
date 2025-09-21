import { Component, DestroyRef, ElementRef, inject, OnInit, signal, viewChild } from '@angular/core';
import { PlaylistResponse, PlaylistSongResponse } from '../../core/models/playlist/playlistResponse';
import { ActivatedRoute } from '@angular/router';
import { TableModule } from 'primeng/table';
import { Button } from 'primeng/button';
import { AuthService } from '../../auth/auth.service';
import { AsyncPipe, DatePipe, NgClass, NgIf } from '@angular/common';
import { FormatDurationPipe } from '../../shared/pipes/format-duration.pipe';
import { SongResponse } from '../../core/models/song/songResponse';
import { AudioService } from '../../core/services/audio.service';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { switchMap, take } from 'rxjs';
import { AudioState } from '../../core/models/audioState';
import { PlaylistService } from '../../core/services/playlist.service';
import { SongContextService } from '../../core/services/context-menu.service';
import { SongService } from '../../core/services/song.service';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { DialogModule } from 'primeng/dialog';
import { EditPlaylistModalComponent } from '../../shared/components/modals/edit-playlist-modal/edit-playlist-modal.component';

@Component({
  selector: 'app-playlist',
  standalone: true,
  imports: [DialogModule, TableModule, Button, DatePipe, FormatDurationPipe, IconFieldModule, InputIconModule, InputTextModule, NgIf, AsyncPipe, MenuModule, NgClass],
  templateUrl: './playlist.component.html',
  styleUrl: './playlist.component.scss',
  providers: [DialogService]
})
export class PlaylistComponent implements OnInit {
  private readonly activateRoute = inject(ActivatedRoute);
  public readonly authService = inject(AuthService);
  private readonly audioService = inject(AudioService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly playlistService = inject(PlaylistService);
  private readonly songService = inject(SongService);
  private readonly songContextMenu = inject(SongContextService);
  private readonly dialogService = inject(DialogService);

  private ref: DynamicDialogRef | undefined;
  playlist = signal<PlaylistResponse | undefined>(undefined);
  contentHeaderEl = viewChild.required<ElementRef<HTMLElement>>('contentHeader');
  animationFrameRequested = false;
  repeat = signal<boolean>(false);
  playlistPlayed = signal<boolean>(false);
  selectedSong = signal<PlaylistSongResponse | undefined>(undefined);
  listFormat = signal<ListFormat>(ListFormat.List);
  randomColors: string[] = [
    '80, 56, 160',
    '16, 208, 240',
    '160, 32, 24',
    '224, 32, 112'
  ]
  items = signal<MenuItem[]>([{
    label: 'View as',
    items: [
      {
        label: 'Compact',
        icon: 'pi pi-bars',
        format: ListFormat.Compact
      },
      {
        label: 'List',
        icon: 'pi pi-list',
        format: ListFormat.List
      }
    ]
  }]);
  randomColor = signal<string>('white');
  sizes = signal<unknown[]>([]);

  public readonly state$ = this.audioService.state$;

  get contentHeader() {
    return this.contentHeaderEl().nativeElement;
  }

  setListFormat(format: ListFormat) {
    this.listFormat.set(format);
  }

  openModal() {
    this.ref = this.dialogService.open(EditPlaylistModalComponent, {
      header: 'Edit details',
      width: '524px',
      modal: true,
      breakpoints: {
        '768px': '96vw',
      },
      data: {
        playlist: this.playlist()
      },

      // footer: '<p class="text-sm">By proceeding, you agree to give SoundCloud access to the image you choose to upload. Please make sure you have the right to upload the image<p>'
    })
  }

  ngOnInit(): void {
    this.activateRoute.data.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((data) => {
      this.playlist.set(data['playlist']);
      this.playlistPlayed.set(false)
      this.randomColor.set(this.randomColors[Math.floor(Math.random() * this.randomColors.length)]);
    })
    this.registerChanges();
  }

  registerChanges() {
    this.songContextMenu.onItemDeleted.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        if (this.playlist()) {
          this.playlistService.getPlaylist(this.playlist()!.playlistId)
        }
      })
    this.playlistService.playlistUpdated$.pipe(
      switchMap((playlistId: number) => {
        return this.playlistService.getPlaylist(playlistId)
      }),
      takeUntilDestroyed(this.destroyRef))
      .subscribe((playlist: PlaylistResponse) => {
        console.log('updated')
        this.playlist.set(playlist);
      })

    this.songService.songUpdated$.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((song: SongResponse) => {
        const playlist = this.playlist();
        const songIndex = playlist?.songs.findIndex(s => s.song.songId === song.songId)
        if ((songIndex || songIndex === 0) && playlist) {
          this.playlist()!.songs[songIndex].song = song;
        }
      })
  }

  onContextMenu(event: MouseEvent, song?: SongResponse, playlist?: PlaylistResponse) {
    if (song) {
      this.songContextMenu.playlist = playlist;
      this.songContextMenu.open(event, song)
    }
  }

  trackByFn(index: number, plSong: PlaylistSongResponse) {
    plSong.song.songId;
  }

  removeSongFromPlaylist(songId: number) {
    if (this.playlist()) {
      this.playlistService.removeSongFromPlaylist(this.playlist()!.playlistId, songId).subscribe(() => {
        const filteredSongs = this.playlist()!.songs.filter(x => x.song.songId !== songId);
        this.playlist()!.songs = filteredSongs;
      });
    }
  }

  setDefaultSong(play: boolean = false) {
    const playlist = this.playlist();
    if (playlist && playlist?.songs.length > 0) {
      this.setSong(playlist.songs[0].song, play);
    }
  }

  setSong(song: SongResponse, play: boolean = true) {
    this.audioService.playStream(song, this.playlist(), play).subscribe((event) => {
      if (event.type === 'ended') {
        const nextSongId = this.hasNextSong();
        nextSongId !== -1
          ? this.setSong(this.playlist()!.songs[nextSongId + 1].song)
          : this.repeat() ? this.setDefaultSong(true) : '';
      }
    });
  }

  hasNextSong(): number {
    const playlist = this.playlist();
    let song: SongResponse | undefined;
    this.state$.pipe(take(1)).subscribe((state: AudioState) => {
      this.repeat.set(state.repeat);
      song = state.song;
    });
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

    this.state$.pipe(take(1)).subscribe((state: AudioState) => {
      state.playlist?.playlistId === this.playlist()?.playlistId && state.song?.songId === song.songId
        ? this.audioService.play()
        : this.setSong(song);
    })
  }

  onScroll(event: Event) {
    const element = event.target as HTMLElement;

    if (!this.animationFrameRequested) {
      this.animationFrameRequested = true;
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

  get ListFormat() {
    return ListFormat;
  }

}

enum ListFormat {
  Compact = 1,
  List = 2
}
