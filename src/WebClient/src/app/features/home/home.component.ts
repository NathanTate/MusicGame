import { Component, inject, OnInit, signal } from '@angular/core';
import { TrackPlaylistCardComponent } from '../../shared/components/track-playlist-card/track-playlist-card.component';
import { SongService } from '../../core/services/song.service';
import { SongResponse } from '../../core/models/songResponse';
import { Button } from 'primeng/button';
import { ActivatedRoute } from '@angular/router';
import { CardMiniComponent } from '../../shared/components/song-playlist-card-mini/card-mini.component';
import { SkeletonModule } from 'primeng/skeleton';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [TrackPlaylistCardComponent, CardMiniComponent, Button, SkeletonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  songs = signal<SongResponse[]>([]);
  images = signal<string[]>([
    'https://i.scdn.co/image/ab67616d00001e02761776ec62c9f8a6be00a244',
    'https://i.scdn.co/image/ab67616d00001e0203c58779dfc6029341086829',
    'https://i.scdn.co/image/ab67616d00001e02cae4cab66ee0f893fb458080',
    'https://i.scdn.co/image/ab67616d00001e025f406f061c4ad484a9faf0ad',
    'https://pickasso.spotifycdn.com/image/ab67c0de0000deef/dt/v1/img/daily/4/ab6761610000e5ebd72746814d48d99f588f2ab9/uk',
    'https://i.scdn.co/image/ab67616d0000485198a32f8e79dc0f3ee5b8e735',
    'https://i.scdn.co/image/ab67616d00004851c09f7d089be6dac618cf178f',
    'https://i.scdn.co/image/ab67616d0000485105153032430054873bb5571c',
  ])
  animationFrameRequested = false;

  onScroll(event: Event) {
    const element = event.target as HTMLElement;

    if (!this.animationFrameRequested) {
      window.requestAnimationFrame(() => {
        this.updateStylesOnScroll(element)
        this.animationFrameRequested = false;
      })
    } else {
      this.animationFrameRequested = true;
    }
  }

  updateStylesOnScroll(element: HTMLElement) {
    const scrollTop = element.scrollTop;
    let opacity = 0;

    if (scrollTop === 0) {
      opacity = 0;
    } else if (scrollTop >= 100) {
      opacity = 1;
    } else if (scrollTop > 0 && scrollTop <= 100) {
      opacity = scrollTop / 100;
    }

    element.style.setProperty('--header-opacity', opacity.toString());
  }

  private songService = inject(SongService);
  private activatedRoute = inject(ActivatedRoute);

  ngOnInit(): void {
    this.songs.set(this.activatedRoute.snapshot.data['songs'].map((s: SongResponse) => {
      s.photoUrl = this.images()[Math.floor(Math.random() * this.images().length)]
      return s;
    }))
  }

  getSongs() {
    this.songService.getSongs().subscribe((songs) => {
      this.songs.set(songs.map(s => {
        s.photoUrl = 'https://i.scdn.co/image/ab67616d00001e0203c58779dfc6029341086829'
        return s;
      }));
    })
  }
}
