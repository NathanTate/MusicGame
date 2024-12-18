import { Component, input } from '@angular/core';

@Component({
  selector: 'app-card-mini',
  standalone: true,
  imports: [],
  templateUrl: './card-mini.component.html',
  styleUrl: './card-mini.component.scss'
})
export class CardMiniComponent {
  imgSrc = input<string>('https://i.scdn.co/image/ab67616d00001e0234a959327e30cec2455cef55');
  imageSizePx = input<number>(48);
  title = input.required<string>();
  link = input<string>();
  labelById = `x-card-title-${Math.random()})}`
}
