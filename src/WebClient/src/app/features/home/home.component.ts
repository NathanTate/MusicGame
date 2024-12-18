import { Component, inject, OnInit, signal } from '@angular/core';
import { SongService } from '../../core/services/song.service';
import { SongResponse } from '../../core/models/song/songResponse';
import { Button } from 'primeng/button';
import { ActivatedRoute } from '@angular/router';
import { CardMiniComponent } from '../../shared/components/song-playlist-card-mini/card-mini.component';
import { SkeletonModule } from 'primeng/skeleton';
import { SongListResponse } from '../../core/models/song/songListResponse';
import { PlaylistService } from '../../core/services/playlist.service';
import { PlaylistListResponse } from '../../core/models/playlist/playlistListResponse';
import { TrackCardComponent } from '../../shared/components/track-card/track-card.component';
import { PlaylistCardComponent } from '../../shared/components/playlist-card/playlist-card.component';

export const filterTypesArr = ['All', 'Songs', 'Playlists'] as const;
export type FilterType = typeof filterTypesArr[number];

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [TrackCardComponent, PlaylistCardComponent, CardMiniComponent, Button, SkeletonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  private readonly songService = inject(SongService);
  private readonly playlistService = inject(PlaylistService);
  private readonly activatedRoute = inject(ActivatedRoute);

  images = signal<string[]>([
    'https://i.scdn.co/image/ab67616d00001e02761776ec62c9f8a6be00a244',
    'https://i.scdn.co/image/ab67616d00001e0203c58779dfc6029341086829',
    'https://i.scdn.co/image/ab67616d00001e02cae4cab66ee0f893fb458080',
    'https://i.scdn.co/image/ab67616d00001e025f406f061c4ad484a9faf0ad',
    'https://pickasso.spotifycdn.com/image/ab67c0de0000deef/dt/v1/img/daily/4/ab6761610000e5ebd72746814d48d99f588f2ab9/uk',
    'https://i.scdn.co/image/ab67616d0000485198a32f8e79dc0f3ee5b8e735',
    'https://i.scdn.co/image/ab67616d00004851c09f7d089be6dac618cf178f',
    'https://i.scdn.co/image/ab67616d0000485105153032430054873bb5571c',
  ])
  songs = signal<SongListResponse | null>(null);
  playlists = signal<PlaylistListResponse | null>(null);
  animationFrameRequested = false;
  readonly filterTypes = filterTypesArr;
  currentFilter = signal<FilterType>('All');

  ngOnInit(): void {
    const songs = this.activatedRoute.snapshot.data['songs'];
    songs.items.map((s: SongResponse) => {
      s.photoUrl = this.images()[Math.floor(Math.random() * this.images().length)]
      return s;
    })
    this.getPlaylists();
    this.songs.set(songs);
  }

  getSongs() {
    this.songService.getSongs(this.songService.songsQuery).subscribe((songs) => {
      this.songs.set(songs);
    })
  }

  getPlaylists() {
    this.playlistService.getPlaylists(this.playlistService.playlistsQuery).subscribe((playlists) => {
      this.playlists.set(playlists);
    })
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
