import { AfterViewInit, Component, HostListener, inject, OnInit, signal, viewChild } from '@angular/core';
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
import { PlaylistUpdateRequest } from '../../models/playlist/PlaylistUpdateRequest';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';

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
  cm = viewChild.required(ContextMenu);
  expanded = signal(window.innerWidth > 1024);
  items = signal<MenuItem[] | undefined>(undefined);
  contextItems = signal<MenuItem[] | undefined>(undefined);
  playlists = signal<PlaylistResponse[]>([]);
  isLoading = signal(false);
  selectedPlaylist = signal<PlaylistResponse | null>(null);
  timeoutId: ReturnType<typeof setTimeout> | undefined;
  visible = signal(false)
  ref: DynamicDialogRef | undefined;

  private playlistService = inject(PlaylistService)
  private dialogService = inject(DialogService)
  private router = inject(Router);


  onContextMenu(event: MouseEvent, playlist: PlaylistResponse): void {
    this.selectedPlaylist.set(playlist);
    this.cm().show(event);
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
    this.items.set([
      {
        label: 'Logout',
        command: () => this.createPlaylist()
      }
    ])
    this.contextItems.set([
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
    ])
  }

  openModal() {
    this.visible.set(true);
  }

  updatePlaylist(playlist: PlaylistUpdateRequest) {
    this.playlistService.updatePlaylist(playlist)
  }

  updatePhoto() {

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
    this.isLoading.set(true);
    this.playlistService.getPlaylists().subscribe({
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

