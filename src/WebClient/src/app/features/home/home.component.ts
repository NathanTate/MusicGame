import { AfterViewInit, Component, DestroyRef, HostListener, inject, OnInit, signal } from '@angular/core';
import { SongService } from '../../core/services/song.service';
import { SongResponse } from '../../core/models/song/songResponse';
import { Button } from 'primeng/button';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CardMiniComponent } from '../../shared/components/song-playlist-card-mini/card-mini.component';
import { SkeletonModule } from 'primeng/skeleton';
import { SongListResponse } from '../../core/models/song/songListResponse';
import { PlaylistService } from '../../core/services/playlist.service';
import { PlaylistListResponse } from '../../core/models/playlist/playlistListResponse';
import { TrackCardComponent } from '../../shared/components/track-card/track-card.component';
import { PlaylistCardComponent } from '../../shared/components/playlist-card/playlist-card.component';
import { LoadingService } from '../../core/services/loading.service';
import { audit, auditTime, distinctUntilChanged, finalize, fromEvent, startWith, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Guid } from 'guid-typescript';

export const filterTypesArr = ['All', 'Songs', 'Playlists'] as const;
export type FilterType = typeof filterTypesArr[number];

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [TrackCardComponent, PlaylistCardComponent, CardMiniComponent, Button, SkeletonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit, AfterViewInit {
  private readonly songService = inject(SongService);
  private readonly playlistService = inject(PlaylistService);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly loadingService = inject(LoadingService);
  private readonly destroyRef = inject(DestroyRef);

  songs = signal<SongListResponse | null>(null);
  playlists = signal<PlaylistListResponse | null>(null);
  animationFrameRequested = false;
  readonly filterTypes = filterTypesArr;
  currentFilter = signal<FilterType>('All');
  contentSections = signal<Element[]>([]);

  ngOnInit(): void {
    const songs = this.activatedRoute.snapshot.data['songs'];
    this.getPlaylists();
    this.songs.set(songs);
    this.registerChanges();
  }

  ngAfterViewInit(): void {
    this.contentSections.set(Array.from(document.querySelectorAll('.content-section')));

    fromEvent(window, 'resize')
      .pipe(distinctUntilChanged(), auditTime(300), startWith(0), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.onResize();
      })
  }

  private onResize() {
    if (this.contentSections().length === 0) return;

    this.contentSections().map(section => {
      const gridContainer = section.getElementsByClassName('grid-container')[0];
      if (!gridContainer) return;

      const maxElementsCapacity = Math.floor(gridContainer.clientWidth / 160);
      const currentAmountOfElement = gridContainer.children.length;

      currentAmountOfElement > maxElementsCapacity
        ? section.setAttribute('overflow', '')
        : section.removeAttribute('overflow');
    })
  }

  registerChanges() {
    this.songService.songUpdated$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((song: SongResponse) => {
        const index = this.songs()?.items.findIndex(x => x.songId === song.songId)
        if (index !== -1 && index && this.songs()?.items) {
          this.songs()!.items[index] = song;
        }
      })
  }

  getSongs() {
    this.loadingService.busy()
    this.songService.getSongs(this.songService.songsQuery)
      .pipe(finalize(() => this.loadingService.idle()))
      .subscribe((songs) => {
        this.songs.set(songs);
      })
  }

  getPlaylists() {
    this.playlistService.getPlaylists(this.playlistService.playlistsQuery)
      .pipe(tap(() => this.loadingService.busy()), finalize(() => this.loadingService.idle()))
      .subscribe((playlists) => {
        this.playlists.set(playlists);
      })
  }

  onHide() {

  }

  setFilter(filter: FilterType) {
    this.currentFilter.set(filter);
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
    let opacity = 0;

    if (scrollTop === 0) {
      opacity = 0;
    } else if (scrollTop >= 100) {
      opacity = 1;
    } else if (scrollTop > 0 && scrollTop <= 100) {
      opacity = scrollTop / 100;
    }

    element.style.setProperty('--header-opacity', opacity.toString());
  }
}
