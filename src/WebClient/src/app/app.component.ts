import { Component, HostListener, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PrimeNGConfig } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { PlaybackStateService } from './core/services/playbackState.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  private readonly primengConfig = inject(PrimeNGConfig);
  private readonly playbackStateService = inject(PlaybackStateService);

  ngOnInit(): void {
    this.primengConfig.ripple = true;
    this.playbackStateService.initialize();
  }

  @HostListener('window:beforeunload', ['$event'])
  onAppClosed(event: Event) {
    this.playbackStateService.saveState();
  }
}
