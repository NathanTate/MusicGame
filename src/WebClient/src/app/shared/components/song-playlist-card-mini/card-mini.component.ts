import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-card-mini',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './card-mini.component.html',
  styleUrl: './card-mini.component.scss'
})
export class CardMiniComponent {
  imgSrc = input<string>();
  imageSizePx = input<number>(48);
  title = input.required<string>();
  link = input.required<string>();
  labelById = `x-card-title-${Math.random()})}`
}
