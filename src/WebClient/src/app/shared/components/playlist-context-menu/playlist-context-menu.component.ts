import { Component, DestroyRef, inject, OnInit, output, signal, viewChild } from '@angular/core';
import { ContextMenu, ContextMenuModule } from 'primeng/contextmenu';
import { PlaylistResponse } from '../../../core/models/playlist/playlistResponse';
import { MenuItem } from 'primeng/api';
import { AuthService } from '../../../auth/auth.service';
import { PlaylistService } from '../../../core/services/playlist.service';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { EditPlaylistModalComponent } from '../modals/edit-playlist-modal/edit-playlist-modal.component';
import { DialogModule } from 'primeng/dialog';
import { ContextMenuEvent, PlaylistContextService } from '../../../core/services/context-menu.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-playlist-context-menu',
  standalone: true,
  imports: [ContextMenuModule, DialogModule],
  templateUrl: './playlist-context-menu.component.html',
  styles: ``,
  providers: [DialogService]
})
export class PlaylistContextMenuComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly playlistService = inject(PlaylistService);
  private readonly dialogService = inject(DialogService);
  private readonly contextMenu = inject(PlaylistContextService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly cm = viewChild.required(ContextMenu);
  public selectedPlaylist = signal<PlaylistResponse | null>(null);
  public contextItems = signal<MenuItem[] | undefined>(undefined);
  private ref: DynamicDialogRef | undefined;

  ngOnInit(): void {
    this.contextMenu.onMenuOpened.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((event: ContextMenuEvent<PlaylistResponse>) => {
        this.onContextMenu(event.event, event.item);
      });
  }

  toggleLike() {
    const playlist = this.selectedPlaylist()!;
    this.playlistService.toggleLike(playlist.playlistId).subscribe(() => {
      this.contextMenu.itemDeleted(playlist);
    })
  }

  deletePlaylist() {
    const playlist = this.selectedPlaylist()!;
    this.playlistService.deletePlaylist(playlist.playlistId).subscribe(() => {
      this.contextMenu.itemDeleted(playlist);
    });
  }

  onHide() {
    this.contextMenu.hide();
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
        playlist: this.selectedPlaylist()
      },

      // footer: '<p class="text-sm">By proceeding, you agree to give SoundCloud access to the image you choose to upload. Please make sure you have the right to upload the image<p>'
    })
  }

  onContextMenu(event: MouseEvent, playlist: PlaylistResponse): void {
    this.selectedPlaylist.set(playlist);
    this.contextItems.set([
      { label: 'Loading...', disabled: true }
    ]);

    this.cm().show(event);

    this.getMenuItemsForPlaylist(playlist).then((menuItems) => {
      this.contextItems.set(menuItems);
    });
  }

  async getMenuItemsForPlaylist(playlist: PlaylistResponse): Promise<MenuItem[]> {
    const menuItems: MenuItem[] = [];

    if (playlist.user.email === this.authService.authData()?.email) {
      menuItems.push(
        {
          icon: 'pi pi-pencil',
          label: 'Edit details',
          command: () => this.openModal()
        },
        {
          icon: 'pi pi-trash',
          label: 'Delete',
          command: () => this.deletePlaylist()
        }
      );
    } else {
      const isLiked = await this.playlistService.isLiked(playlist.playlistId).toPromise();
      menuItems.push(
        isLiked
          ? {
            icon: 'pi pi-check-circle',
            label: 'Remove from your Library',
            command: () => this.toggleLike()
          }
          : {
            icon: 'pi pi-plus',
            label: 'Add to your Library',
            command: () => this.toggleLike()
          }
      );
    }

    return menuItems;
  }
}
