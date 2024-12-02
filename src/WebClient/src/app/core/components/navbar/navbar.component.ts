import { Component, inject, OnInit } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../../../auth/auth.service';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { ToolbarModule } from 'primeng/toolbar';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [ButtonModule, ToolbarModule, MenuModule, IconFieldModule, InputIconModule, InputTextModule],
  templateUrl: './navbar.component.html',
  styles: `
  .avatar {
    height: 100%;
    width: 100%;
    border-radius: 50%;
    object-fit: cover;
    object-position: center center;
  }
  `
})
export class NavbarComponent implements OnInit {
  items: MenuItem[] | undefined;

  public authService = inject(AuthService);

  ngOnInit(): void {
    this.items = [
      {
        label: 'Profile',
        icon: 'pi pi-user',
        routerLink: '/profile'
      },
      {
        label: 'Logout',
        icon: 'pi pi-sign-out',
        command: () => this.logout()
      }
    ]
  }

  logout() {
    this.authService.logout();
  }
}
