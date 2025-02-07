import { Component, DestroyRef, inject, OnInit, signal, viewChild } from '@angular/core';
import { ContextMenu, ContextMenuModule } from 'primeng/contextmenu';
import { MenuItem } from 'primeng/api';
import { AuthService } from '../../../auth/auth.service';
import { PlaylistService } from '../../../core/services/playlist.service';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { DialogModule } from 'primeng/dialog';
import { ContextMenuEvent, SongContextService } from '../../../core/services/context-menu.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SongResponse } from '../../../core/models/song/songResponse';
import { SongService } from '../../../core/services/song.service';
import { EditSongModalComponent } from '../modals/edit-song-modal/edit-song-modal.component';
import { EMPTY, from, map, Observable, switchMap, take } from 'rxjs';
import { OwnPlaylistsModalComponent } from '../modals/own-playlists-modal/own-playlists-modal.component';
import { AudioService } from '../../../core/services/audio.service';
import { AudioState } from '../../../core/models/audioState';

@Component({
  selector: 'app-song-context-menu',
  standalone: true,
  imports: [ContextMenuModule, DialogModule],
  templateUrl: './song-context-menu.component.html',
  styles: ``,
  providers: [DialogService]
})
export class SongContextMenuComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly playlistService = inject(PlaylistService);
  private readonly songService = inject(SongService);
  private readonly dialogService = inject(DialogService);
  private readonly contextMenu = inject(SongContextService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly audioService = inject(AudioService);


  private readonly cm = viewChild.required(ContextMenu);
  public selectedSong = signal<SongResponse | null>(null);
  public contextItems = signal<MenuItem[] | undefined>(undefined);
  private ref: DynamicDialogRef | undefined;

  ngOnInit(): void {
    this.contextMenu.onMenuOpened.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((event: ContextMenuEvent<SongResponse>) => {
        this.onContextMenu(event.event, event.item);
      });
  }

  toggleLike() {
    const song = this.selectedSong()!;
    this.songService.toggleLike(song.songId).subscribe(() => {
      this.contextMenu.itemDeleted(song);
    })
  }

  deleteSong() {
    const song = this.selectedSong()!;
    this.songService.deleteSong(song.songId).subscribe(() => {
      this.contextMenu.itemDeleted(song);
    });
  }

  removeSongFromPlaylist() {
    const song = this.selectedSong()!;
    if (this.contextMenu.playlist) {
      this.playlistService.removeSongFromPlaylist(this.contextMenu.playlist.playlistId, song.songId).subscribe(() => this.contextMenu.itemDeleted(song))
    } else {
      this.audioService.state$.pipe(take(1),
        switchMap((state: AudioState) => {
          return state.playlist
            ? this.playlistService.removeSongFromPlaylist(state.playlist?.playlistId, song.songId)
            : EMPTY
        })
      ).subscribe(() => this.contextMenu.itemDeleted(song))
    }
  }


  onHide() {
    this.contextMenu.hide();
  }

  openModal() {
    this.ref = this.dialogService.open(OwnPlaylistsModalComponent, {
      header: 'Add song to playlist',
      width: '524px',
      modal: true,
      breakpoints: {
        '768px': '96vw',
      },
      contentStyle: { 'padding': '0' },
      data: {
        songId: this.selectedSong()!.songId,
        userId: this.authService.authData()?.userId
      }
    })
  }


  openEditModal() {
    this.ref = this.dialogService.open(EditSongModalComponent, {
      header: 'Edit details',
      width: '800px',
      modal: true,
      breakpoints: {
        '1024px': '96vw',
      },
      data: {
        song: this.selectedSong(),
      },

      // footer: '<p class="text-sm">By proceeding, you agree to give SoundCloud access to the image you choose to upload. Please make sure you have the right to upload the image<p>'
    })
  }

  onContextMenu(event: MouseEvent, song: SongResponse): void {
    this.selectedSong.set(song);
    this.contextItems.set([
      { label: 'Loading...', disabled: true }
    ]);

    this.cm().show(event);

    this.getMenuItemsForSong(song).subscribe((menuItems) => {
      this.contextItems.set(menuItems);
    });
  }

  getMenuItemsForSong(song: SongResponse): Observable<MenuItem[]> {
    const menuItems: MenuItem[] = [
      {
        icon: 'pi pi-plus',
        label: 'Add to playlist',
        command: () => this.openModal()
      },
      {
        icon: 'pi pi-check-circle',
        label: 'Remove from playlist',
        command: () => this.removeSongFromPlaylist()
      }
    ];

    if (song.artist.email === this.authService.authData()?.email) {
      menuItems.splice(3, 0,
        {
          icon: 'pi pi-pencil',
          label: 'Edit details',
          command: () => this.openEditModal()
        },
        {
          icon: 'pi pi-trash',
          label: 'Delete',
          command: () => this.deleteSong()
        }
      );
    } else {
      return this.songService.isLiked(song.songId).pipe(map((isLiked) => {
        menuItems.splice(2, 0,
          isLiked
            ? {
              icon: 'pi pi-heart-fill',
              label: 'Removed from liked',
              command: () => this.toggleLike()
            }
            : {
              icon: 'pi pi-heart',
              label: 'Add to liked',
              command: () => this.toggleLike()
            }
        );
        return menuItems;
      }));

    }

    return from(Promise.resolve(menuItems));
  }
}
