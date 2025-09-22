import { Component, DestroyRef, ElementRef, inject, OnInit, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { SongResponse } from '../../core/models/song/songResponse';
import { AudioService } from '../../core/services/audio.service';
import { SongService } from '../../core/services/song.service';
import { CommonModule } from '@angular/common';
import { Button } from 'primeng/button';
import { FormatDurationPipe } from '../../shared/pipes/format-duration.pipe';
import { AuthService } from '../../auth/auth.service';
import { take } from 'rxjs';
import { AudioState } from '../../core/models/audioState';
import { isNullOrUndefined } from 'is-what';

@Component({
  selector: 'app-track-page',
  standalone: true,
  imports: [CommonModule, Button, FormatDurationPipe],
  templateUrl: './track-page.component.html',
  styleUrl: './track-page.component.scss'
})
export class TrackPageComponent implements OnInit {
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly audioService = inject(AudioService);
  private readonly songService = inject(SongService);
  private readonly authService = inject(AuthService);

  public readonly song = signal<SongResponse | null>(null);
  public readonly lyrics = signal<Lyrics[]>([]);
  public readonly state$ = this.audioService.state$;
  contentHeaderEl = viewChild.required<ElementRef<HTMLElement>>('contentHeader');
  animationFrameRequested = false;
  randomColor = signal<string>('white');
  isLiked = signal<boolean>(false);
  isOwner = signal<boolean>(false);
  randomColors: string[] = [
    '80, 56, 160',
    '16, 208, 240',
    '160, 32, 24',
    '224, 32, 112'
  ];

  get contentHeader() {
    return this.contentHeaderEl().nativeElement;
  }

  ngOnInit(): void {
    this.registerChanges();
  }

  registerChanges() {
    this.activatedRoute.data.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((data) => {
      this.song.set(data['song']);
      const songItem = data['song'];
      this.lyrics.set(this.splitLyrics(songItem.lyrics));
      if (songItem && songItem.artist.email === this.authService.authData()?.email) {
        this.isOwner.set(true);
      } else if (songItem) {
        this.songService.isLiked(songItem.songId).subscribe((isLiked) => this.isLiked.set(isLiked));
      }
      this.randomColor.set(this.randomColors[Math.floor(Math.random() * this.randomColors.length)]);
    })

    this.songService.songUpdated$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((song: SongResponse) => {
        if (this.song()?.songId === song.songId) {
          this.song.set(song);
        }
      })
  }

  toggleLike() {
    this.songService.toggleLike(this.song()!.songId).subscribe((isLiked) => this.isLiked.set(isLiked));
  }

  onPlay() {
    this.audioService.state$.pipe(take(1)).subscribe((state: AudioState) => {
      state.song?.songId === this.song()!.songId && isNullOrUndefined(state.playlist)
        ? this.audioService.play()
        : this.audioService.playStream(this.song()!, undefined, true).subscribe();
    })
  }

  onPause() {
    this.audioService.pause();
  }

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

  splitLyrics(text: string) {
    if (isNullOrUndefined(text) || text.length === 0) {
      return [];
    }

    const matches = text.match(/\[[^\]]*\]|[^[\]]+/g);
    let lyrics: Lyrics[] = [];
    let currentLyrics: Lyrics | null = null;

    if (matches) {
      matches.forEach((part) => {
        if (part.startsWith("[")) {
          if (currentLyrics) lyrics.push(currentLyrics);
          currentLyrics = { title: part.slice(1, -1), chores: "" };
        } else if (currentLyrics) {
          currentLyrics.chores = part;
        }
      });
    }

    if (currentLyrics) lyrics.push(currentLyrics);

    return lyrics;
  }

  updateStylesOnScroll(element: HTMLElement) {
    const scrollTop = element.scrollTop;
    const headerHeight = this.contentHeader.clientHeight;
    let opacity = 0;
    let contentOpacity = 0;
    const startChange = headerHeight - 70;
    const endChange = headerHeight - 30;

    if (scrollTop <= headerHeight - 50) {
      opacity = 0;
      contentOpacity = 0;
    } else if (scrollTop >= headerHeight - 50 && scrollTop <= headerHeight) {
      opacity = (scrollTop - startChange) / (endChange - startChange);
      contentOpacity = 1
    } else if (scrollTop > headerHeight) {
      opacity = 1;
      contentOpacity = 1
    }
    element.style.setProperty('--header-content-opacity', contentOpacity.toString());
    element.style.setProperty('--header-opacity', opacity.toString());
  }

}

interface Lyrics {
  title: string,
  chores: string
}

