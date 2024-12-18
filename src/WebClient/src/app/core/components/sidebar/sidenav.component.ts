import { Component, DestroyRef, HostListener, inject, OnInit, signal, viewChild } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { PlaylistService } from '../../services/playlist.service';
import { ContextMenu, ContextMenuModule } from 'primeng/contextmenu';
import { SkeletonModule } from 'primeng/skeleton';
import { Ripple } from 'primeng/ripple';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { OverflowScrollDirective } from '../../../shared/directives/overflow-scroll.directive';
import { DialogModule } from 'primeng/dialog';
import { PlaylistResponse } from '../../models/playlist/playlistResponse';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { PlaylistListResponse } from '../../models/playlist/playlistListResponse';
import { EditPlaylistModalComponent } from '../../../shared/components/edit-playlist-modal/edit-playlist-modal.component';
import { AuthService } from '../../../auth/auth.service';
import { PlaybackState, PlaybackStateService } from '../../services/playbackState.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-sidenav',
  standalone: true,
  imports: [ButtonModule, RouterLink, RouterLinkActive, MenuModule,
    ContextMenuModule, SkeletonModule, Ripple, ScrollPanelModule, OverflowScrollDirective, DialogModule],
  templateUrl: './sidenav.component.html',
  styleUrl: './sidenav.component.scss',
  providers: [DialogService]
})
export class SidenavComponent implements OnInit {
  private playlistService = inject(PlaylistService)
  private dialogService = inject(DialogService)
  private router = inject(Router);
  private authService = inject(AuthService);
  private playbackStateService = inject(PlaybackStateService);
  private destroyRef = inject(DestroyRef);

  cm = viewChild.required(ContextMenu);
  expanded = signal(window.innerWidth > 1024);
  items = signal<MenuItem[] | undefined>(undefined);
  contextItems = signal<MenuItem[] | undefined>(undefined);
  playlists = signal<PlaylistListResponse | null>(null);
  playbackState = signal<PlaybackState | undefined>(undefined);
  isLoading = signal(false);
  selectedPlaylist = signal<PlaylistResponse | null>(null);
  timeoutId: ReturnType<typeof setTimeout> | undefined;
  ref: DynamicDialogRef | undefined;

  onContextMenu(event: MouseEvent, playlist: PlaylistResponse): void {
    this.selectedPlaylist.set(playlist);
    this.contextItems.set(this.getMenuItemsForPlaylist(playlist));
    this.cm().show(event);
  }

  getMenuItemsForPlaylist(playlist: PlaylistResponse): MenuItem[] {
    let menuItems: MenuItem[] | undefined;
    if (playlist.user.email === this.authService.authData()?.email) {
      menuItems = [
        {
          icon: 'pi pi-pencil',
          label: 'Edit details',
          command: () => this.openModal()
        },
        {
          icon: 'pi pi-trash',
          label: 'Delete',
          command: () => this.deletePlaylist(this.selectedPlaylist()!.playlistId)
        }
      ]
    } else {
      menuItems = [
        {
          icon: 'pi pi-check-circle',
          label: 'Remove from your Library',
          command: () => console.log('removed')
        }
      ]
    }

    return menuItems;
  }

  onHide() {
    this.selectedPlaylist.set(null);
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: Event) {
    window.innerWidth < 1024
      ? this.expanded.set(false)
      : this.expanded.set(true);
  }

  ngOnInit(): void {
    this.getPlaylists();
    this.playbackStateService.state$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((state) => {
      this.playbackState.set(state);
    })
    this.items.set([
      {
        label: 'Logout',
        command: () => this.createPlaylist()
      }
    ])
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


  createPlaylist() {
    this.playlistService.createPlaylist().subscribe((playlist) => {
      this.router.navigate(['/playlist', playlist.playlistId]);
      this.getPlaylists();
    })
  }

  deletePlaylist(playlistId: number) {
    this.playlistService.deletePlaylist(playlistId).subscribe(() => {
      this.getPlaylists();
    });
  }

  getPlaylists() {
    this.timeoutId = setTimeout(() => this.isLoading.set(true), 500)
    this.playlistService.getPlaylists(this.playlistService.playlistsQuery).subscribe({
      next: (playlists) => {
        this.playlists.set(playlists);
        clearTimeout(this.timeoutId);
        this.isLoading.set(false);
      },
      error: () => {
        clearTimeout(this.timeoutId)
        this.isLoading.set(false)
      }
    })
  }

}

