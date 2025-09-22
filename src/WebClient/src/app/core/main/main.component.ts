import { Component, HostListener, inject, OnInit } from '@angular/core';
import { NavbarComponent } from '../components/navbar/navbar.component';
import { RouterOutlet } from '@angular/router';
import { SidenavComponent } from "../components/sidebar/sidenav.component";
import { NowPlayingBarComponent } from '../../features/now-playing-bar/now-playing-bar.component';
import { PlaylistContextMenuComponent } from '../../shared/components/playlist-context-menu/playlist-context-menu.component';
import { SongContextMenuComponent } from "../../shared/components/song-context-menu/song-context-menu.component";
import { LoadingService } from '../services/loading.service';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-main',
  standalone: true,
  imports: [NavbarComponent, NowPlayingBarComponent, RouterOutlet, SidenavComponent, PlaylistContextMenuComponent, SongContextMenuComponent, AsyncPipe],
  templateUrl: './main.component.html',
  styleUrl: './main.component.scss'
})
export class MainComponent {
  public readonly loadingService = inject(LoadingService);

  @HostListener('document:contextmenu', ['$event'])
  onContextMenu(event: MouseEvent) {
    event.preventDefault();
  }
}
