import { Component, OnInit } from '@angular/core';
import { NavbarComponent } from '../components/navbar/navbar.component';
import { RouterOutlet } from '@angular/router';
import { SidenavComponent } from "../components/sidebar/sidenav.component";
import { NowPlayingBarComponent } from '../../features/now-playing-bar/now-playing-bar.component';

@Component({
  selector: 'app-main',
  standalone: true,
  imports: [NavbarComponent, NowPlayingBarComponent, RouterOutlet, SidenavComponent],
  templateUrl: './main.component.html',
  styleUrl: './main.component.scss'
})
export class MainComponent implements OnInit {


  ngOnInit(): void {
  }
}
