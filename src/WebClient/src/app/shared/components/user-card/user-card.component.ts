import { Component, computed, input } from '@angular/core';
import { User } from '../../../core/models/user/user';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-user-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './user-card.component.html',
  styleUrl: './user-card.component.scss'
})
export class UserCardComponent {
  type = input<'Profile' | 'Author'>('Author');
  user = input.required<User>();
  rounded = input<boolean>(true);
  labelById = computed(() => `x-card-title-${this.user().userId}`)
}
