import { Component, DestroyRef, HostListener, inject, OnInit, signal, viewChild } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { PlaylistService } from '../../services/playlist.service';
import { SkeletonModule } from 'primeng/skeleton';
import { Ripple } from 'primeng/ripple';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { OverflowScrollDirective } from '../../../shared/directives/overflow-scroll.directive';
import { PlaylistResponse } from '../../models/playlist/playlistResponse';
import { AuthService } from '../../../auth/auth.service';
import { AudioService } from '../../services/audio.service';
import { AsyncPipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin, take } from 'rxjs';
import { PlaylistListResponse } from '../../models/playlist/playlistListResponse';
import { UserService } from '../../services/user.service';
import { PlaylistContextService } from '../../services/context-menu.service';
import { AudioState } from '../../models/audioState';

@Component({
  selector: 'app-sidenav',
  standalone: true,
  imports: [ButtonModule, RouterLink, RouterLinkActive, MenuModule, SkeletonModule, Ripple,
    ScrollPanelModule, OverflowScrollDirective, AsyncPipe],
  templateUrl: './sidenav.component.html',
  styleUrl: './sidenav.component.scss'
})
export class SidenavComponent implements OnInit {
  private readonly playlistService = inject(PlaylistService)
  private readonly authService = inject(AuthService);
  public readonly audioService = inject(AudioService);
  private readonly userService = inject(UserService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly playlistContextMenu = inject(PlaylistContextService);

  public allPlaylists = signal<PlaylistResponse[]>([]);
  public userPlaylists = signal<PlaylistListResponse | null>(null);
  public likedPlaylists = signal<PlaylistListResponse | null>(null);
  public expanded = signal(window.innerWidth > 1024);
  public items = signal<MenuItem[] | undefined>(undefined);

  public isLoading = signal(false);
  public selectedPlaylist = signal<PlaylistResponse | null>(null);
  private timeoutId: ReturnType<typeof setTimeout> | undefined;

  onContextMenu(event: MouseEvent, playlist: PlaylistResponse) {
    this.playlistContextMenu.open(event, playlist);
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: Event) {
    window.innerWidth < 1024
      ? this.expanded.set(false)
      : this.expanded.set(true);
  }

  ngOnInit(): void {
    this.playlistService.playlistUpdated$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.getPlaylists();
    })

    this.playlistContextMenu.onItemDeleted.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.getPlaylists(true);
    })

    this.getPlaylists(true);
    this.items.set([
      {
        label: 'Logout',
        command: () => this.createPlaylist()
      }
    ])
  }

  createPlaylist() {
    this.playlistService.createPlaylist().subscribe((playlist) => {
      this.router.navigate(['/playlist', playlist.playlistId]);
      this.getPlaylists();
    })
  }

  onPlaylistPlay(playlist: PlaylistResponse) {
    this.audioService.state$.pipe(take(1)).subscribe((state: AudioState) => {
      if (state.playlist && state.playlist.playlistId === playlist.playlistId) {
        state.playing ? this.audioService.pause() : this.audioService.play();
      } else {
        this.playlistService.getPlaylist(playlist.playlistId).subscribe((response: PlaylistResponse) => {
          const song = response.songs[0].song;
          this.audioService.playStream(song, response, true).subscribe();
        })
      }
    })
  }

  getPlaylists(includeLiked: boolean = false) {
    this.timeoutId = setTimeout(() => this.isLoading.set(true), 500)
    const authData = this.authService.authData();
    if (authData) {
      const userPlaylists$ = this.userService.getUserPlaylists(authData.userId, { ...this.userService.playlistsQuery });
      const likedPlaylists$ = this.userService.getUserLikedPlaylists(authData.userId, { ...this.userService.playlistsQuery });
      const likedSongs$ = this.userService.getUserLikedSongs(authData.userId, { ...this.userService.songsQuery })

      includeLiked ?
        forkJoin([userPlaylists$, likedPlaylists$, likedSongs$])
          .subscribe(([userPlaylists, likedPlaylists, likedSongs]) => {
            const user = this.authService.authDataToArtist(authData);
            const likedSongsPlaylist = this.userService.createPlaylistFromSongs(likedSongs, user);
            this.userService.likedSongsPlaylist.set(likedSongsPlaylist);

            this.userPlaylists.set(userPlaylists);
            this.likedPlaylists.set(likedPlaylists);
            this.allPlaylists.set([likedSongsPlaylist, ...userPlaylists.items, ...likedPlaylists.items]);
          })
        : forkJoin([userPlaylists$, likedSongs$])
          .subscribe(([userPlaylists, likedSongs]) => {
            const user = this.authService.authDataToArtist(authData);
            const likedSongsPlaylist = this.userService.createPlaylistFromSongs(likedSongs, user);
            this.userService.likedSongsPlaylist.set(likedSongsPlaylist);

            this.userPlaylists.set(userPlaylists);
            this.allPlaylists.set([likedSongsPlaylist, ...userPlaylists.items, ...this.likedPlaylists()?.items || []]);
          })
    }
  }

}

