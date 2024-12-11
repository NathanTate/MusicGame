import { Component, computed, input, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SongResponse } from '../../../core/models/songResponse';

@Component({
  selector: 'app-track-playlist-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './track-playlist-card.component.html',
  styleUrl: './track-playlist-card.component.scss'
})
export class TrackPlaylistCardComponent {
  showTitle = input<boolean>(true);
  song = input.required<SongResponse>();
  labelById = computed(() => `x-card-title-${this.song().songId}`)
}
