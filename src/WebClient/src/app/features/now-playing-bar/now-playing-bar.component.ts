import { Component, HostListener, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { AudioService } from '../../core/services/audio.service';
import { AsyncPipe, NgIf } from '@angular/common';
import { ContextMenuModule } from 'primeng/contextmenu';
import { DraggableBarComponent } from '../../shared/components/draggable-bar/draggable-bar.component';
import { take } from 'rxjs';
import { AudioState } from '../../core/models/audioState';
import { SongContextService } from '../../core/services/context-menu.service';
import { SongResponse } from '../../core/models/song/songResponse';

@Component({
  selector: 'app-now-playing-bar',
  standalone: true,
  imports: [Button, ContextMenuModule, RouterLink, AsyncPipe, NgIf, DraggableBarComponent],
  templateUrl: './now-playing-bar.component.html',
  styleUrl: './now-playing-bar.component.scss'
})
export class NowPlayingBarComponent implements OnInit {
  private readonly audioService = inject(AudioService);
  private readonly songContextMenu = inject(SongContextService)
  public readonly state$ = this.audioService.state$;

  private get previousVolume() {
    const volumeString = localStorage.getItem("previousVolume");
    const volume = volumeString ? +volumeString : 30;
    return volume;
  }

  @HostListener('document:keyup.Space', ['$event'])
  onSpaceUp(event: Event) {
    const target = event.target as HTMLElement;
    const isInputField = ['INPUT', 'TEXTAREA'].includes(target.tagName) || target.isContentEditable;

    if (isInputField) return;

    this.state$.pipe(take(1)).subscribe((state) => {
      state.playing ? this.onPause() : this.onPlay();
    })
  }

  @HostListener('window:beforeunload', ['$event'])
  onWindowClosed(event: Event) {
    this.audioService.saveState();
  }

  ngOnInit(): void {
    this.audioService.updateVolume(this.previousVolume);
    this.audioService.setSongFromLocalStorage();
  }

  onContextMenu(event: MouseEvent) {
    let song: SongResponse | undefined;

    this.audioService.state$.pipe(take(1)).subscribe((state: AudioState) => {
      song = state.song;
    })
    if (song) {
      this.songContextMenu.open(event, song)
    }
  }

  onPlayingBarValueChange(value: number) {
    this.audioService.seekTo(value);
  }

  onVolumeValueChange(value: number) {
    this.audioService.updateVolume(value);
    localStorage.setItem('previousVolume', value.toString());
  }

  toggleMute() {
    this.state$.pipe(take(1)).subscribe((state: AudioState) => {
      state.muted ? this.audioService.updateVolume(this.previousVolume) : this.audioService.updateVolume(0);
    })
  }

  onPlay() {
    this.audioService.play();
  }

  toggleRepeat() {
    this.audioService.toggleRepeat();
  }

  onPause() {
    this.audioService.pause();
  }

  nextSong() {
    this.state$.pipe(take(1)).subscribe((state: AudioState) => {
      if (!state.playlist || !state.song) {
        return;
      }

      if (!state.playlist) {
        //play random song
        return;
      }

      state.playlist.playlistId;
      const currentSongIndex = state.playlist.songs.map(x => x.song.songId).indexOf(state.song.songId)
      const songsLeftCount = state.playlist.songs.length - currentSongIndex - 1;

      if (songsLeftCount === 0 && state.repeat) {
        this.audioService.playStream(state.playlist.songs[0].song, state.playlist, true).subscribe();
        return;
      } else if (songsLeftCount === 0) {
        //play random song
        return;
      }

      const nextSong = state.playlist.songs[currentSongIndex + 1].song;
      this.audioService.playStream(nextSong, state.playlist, true).subscribe();
    })
  }

  previousSong() {
    this.state$.pipe(take(1)).subscribe((state: AudioState) => {
      if (!state.playlist || !state.song) {
        return;
      }

      const currentSongIndex = state.playlist.songs.map(x => x.song.songId).indexOf(state.song?.songId);

      if (currentSongIndex === 0) {
        return;
      }

      const previousSong = state.playlist.songs[currentSongIndex - 1].song;
      this.audioService.playStream(previousSong, state.playlist, true).subscribe();
    })
  }

  shuffle() {
    this.audioService.toggleShuffle();
  }
}
