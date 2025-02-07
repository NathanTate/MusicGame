import { Component, computed, input } from '@angular/core';
import { GenreResponse } from '../../../core/models/genre/genreResponse';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-genre-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './genre-card.component.html',
  styleUrl: './genre-card.component.scss'
})
export class GenreCardComponent {
  rounded = input<boolean>(false);
  genre = input.required<GenreResponse>();
  labelById = computed(() => `x-card-title-${this.genre().genreId}`)
}
