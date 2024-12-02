import { Component, inject, OnInit, signal } from '@angular/core';
import { PlaylistResponse } from '../../core/models/playlist/playlistResponse';
import { ActivatedRoute } from '@angular/router';
import { PlaylistService } from '../../core/services/playlist.service';

@Component({
  selector: 'app-playlist',
  standalone: true,
  imports: [],
  templateUrl: './playlist.component.html',
  styleUrl: './playlist.component.scss'
})
export class PlaylistComponent implements OnInit {
  playlist = signal<PlaylistResponse | null>(null);

  private activateRoute = inject(ActivatedRoute);
  private playlistService = inject(PlaylistService);

  ngOnInit(): void {
    this.playlist.set(this.activateRoute.snapshot.data['playlist'])
  }
}
