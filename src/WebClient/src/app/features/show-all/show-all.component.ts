import { Component, signal } from '@angular/core';
import { TrackCardComponent } from '../../shared/components/track-card/track-card.component';
import { PlaylistCardComponent } from '../../shared/components/playlist-card/playlist-card.component';
import { Button } from 'primeng/button';
import { SongResponse } from '../../core/models/song/songResponse';
import { PlaylistResponse } from '../../core/models/playlist/playlistResponse';

@Component({
  selector: 'app-show-all',
  standalone: true,
  imports: [TrackCardComponent, PlaylistCardComponent, Button],
  templateUrl: './show-all.component.html',
  styleUrl: './show-all.component.scss'
})
export class ShowAllComponent {
  songs = signal<SongResponse[]>([]);
  playlists = signal<PlaylistResponse[]>([]);
  title = signal<string>('All items')
}
