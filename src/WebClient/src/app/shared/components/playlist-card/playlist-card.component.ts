import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PlaylistResponse } from '../../../core/models/playlist/playlistResponse';

@Component({
  selector: 'app-playlist-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './playlist-card.component.html',
  styleUrl: './playlist-card.component.scss'
})
export class PlaylistCardComponent {
  showTitle = input<boolean>(true);
  playlist = input.required<PlaylistResponse>();
  labelById = computed(() => `x-card-title-${this.playlist().playlistId}`)
}
