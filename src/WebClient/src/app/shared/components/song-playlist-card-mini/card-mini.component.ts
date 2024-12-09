import { Component, input } from '@angular/core';

@Component({
  selector: 'app-card-mini',
  standalone: true,
  imports: [],
  templateUrl: './card-mini.component.html',
  styleUrl: './card-mini.component.scss'
})
export class CardMiniComponent {
  imgSrc = input<string>('https://i.scdn.co/image/ab67616d0000b273cae4cab66ee0f893fb458080');
  imageSizePx = input<number>(48);
  title = input.required<string>();
  link = input<string>();
  labelById = `x-card-title-${Math.random()})}`
}
