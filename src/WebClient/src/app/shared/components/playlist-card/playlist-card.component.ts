import { Component, computed, HostListener, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PlaylistResponse } from '../../../core/models/playlist/playlistResponse';
import { PlaylistContextService } from '../../../core/services/context-menu.service';

@Component({
  selector: 'app-playlist-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './playlist-card.component.html',
  styleUrl: './playlist-card.component.scss'
})
export class PlaylistCardComponent {
  private readonly playlistContextService = inject(PlaylistContextService);

  showTitle = input<boolean>(true);
  rounded = input<boolean>(false);
  playlist = input.required<PlaylistResponse>();
  labelById = computed(() => `x-card-title-${this.playlist().playlistId}`)

  @HostListener('contextmenu', ['$event'])
  contextMenu(event: MouseEvent) {
    this.onContextMenu(event, this.playlist());
  }

  onContextMenu(event: MouseEvent, playlist: PlaylistResponse) {
    this.playlistContextService.open(event, playlist);
  }
}
