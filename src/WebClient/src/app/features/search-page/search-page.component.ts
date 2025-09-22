import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { SearchService } from '../../core/services/search.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SearchData } from '../../core/models/searchData';
import { PlaylistCardComponent } from '../../shared/components/playlist-card/playlist-card.component';
import { TrackCardComponent } from '../../shared/components/track-card/track-card.component';
import { Button } from 'primeng/button';
import { RouterLink } from '@angular/router';
import { TitleCasePipe } from '@angular/common';
import { UserCardComponent } from '../../shared/components/user-card/user-card.component';
import { GenreCardComponent } from '../../shared/components/genre-card/genre-card.component';
import { User } from '../../core/models/user/user';
import { isNullOrUndefined } from 'is-what'
import { SongResponse } from '../../core/models/song/songResponse';
import { ArtistResponse } from '../../core/models/user/artistResponse';
import { PlaylistResponse } from '../../core/models/playlist/playlistResponse';
import { GenreResponse } from '../../core/models/genre/genreResponse';
import { CardMiniComponent } from '../../shared/components/song-playlist-card-mini/card-mini.component';

export const filterTypesArr = ['All', 'Songs', 'Playlists', 'Genres', 'Artists'] as const;
export type FilterType = typeof filterTypesArr[number];

@Component({
  selector: 'app-search-page',
  standalone: true,
  imports: [PlaylistCardComponent, TrackCardComponent, UserCardComponent, GenreCardComponent, CardMiniComponent, Button, RouterLink, TitleCasePipe],
  templateUrl: './search-page.component.html',
  styleUrl: './search-page.component.scss'
})
export class SearchPageComponent implements OnInit {
  private readonly searchService = inject(SearchService);
  private readonly destroyRef = inject(DestroyRef);
  public readonly filterTypes = filterTypesArr;
  public currentFilter = signal<FilterType>('All');
  public searchData = signal<SearchData | null>(null);

  public readonly bestFitItem = computed<BestFitItem | null>(() => {
    const searchData = this.searchData();
    if (!searchData || isNullOrUndefined(searchData.bestFitType) || searchData.bestFitType === '') {
      return null;
    }

    const item: BestFitItem = {
      id: '',
      name: '',
      url: ''
    }

    try {
      switch (searchData.bestFitType) {
        case 'Song':
          const song = searchData.bestFitItem as SongResponse;
          item.id = song.songId;
          item.name = song.name;
          item.photoUrl = song.photoUrl;
          item.artist = song.artist;
          item.url = `/track/${song.songId}`;
          break;
        case 'Playlist':
          const playlist = searchData.bestFitItem as PlaylistResponse;
          item.id = playlist.playlistId;
          item.name = playlist.name;
          item.photoUrl = playlist.photoUrl;
          item.artist = playlist.user;
          item.url = `/playlist/${playlist.playlistId}`;
          break;
        case 'User':
          const user = searchData.bestFitItem as User;
          item.id = user.userId;
          item.name = user.displayName;
          item.photoUrl = user?.photo?.url;
          item.url = `/users/${user.userId}`;
          break;
        case 'Genre':
          const genre = searchData.bestFitItem as GenreResponse;
          item.id = genre.genreId;
          item.name = genre.name
          item.url = `/genres/${genre.genreId}`;
          break;
      }

      return item;
    } catch (err) {
      console.error(err);
      return null;
    }
  })

  ngOnInit(): void {
    this.searchService.searchData$.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((data: SearchData | null) => {
        this.searchData.set(data);
      })
  }

  setFilter(filter: FilterType) {
    this.currentFilter.set(filter);
  }
}

export interface BestFitItem {
  id: string | number;
  name: string;
  url: string;
  artist?: ArtistResponse;
  photoUrl?: string;
}
