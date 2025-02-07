import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { ListboxChangeEvent, ListboxModule } from 'primeng/listbox';
import { DialogService, DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { debounceTime, distinctUntilChanged, EMPTY, filter, Observable, startWith, Subject, switchMap } from 'rxjs';
import { PlaylistService } from '../../../../core/services/playlist.service';
import { UserService } from '../../../../core/services/user.service';
import { UpsertSongPlaylistRequest } from '../../../../core/models/playlist/upsertSongPlaylistRequest';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PlaylistListResponse } from '../../../../core/models/playlist/playlistListResponse';
import { Button } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { InputTextModule } from 'primeng/inputtext';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { isNullOrUndefined } from 'is-what';

@Component({
  selector: 'app-own-playlists-modal',
  standalone: true,
  imports: [ReactiveFormsModule, ListboxModule, Button, DividerModule, InputTextModule, IconFieldModule, InputIconModule],
  templateUrl: './own-playlists-modal.component.html',
  styleUrl: './own-playlists-modal.scss',
  providers: [DialogService]
})
export class OwnPlaylistsModalComponent implements OnInit {
  private readonly playlistService = inject(PlaylistService);
  private readonly userService = inject(UserService);
  private readonly config = inject(DynamicDialogConfig);
  private readonly dialogRef = inject(DynamicDialogRef);
  private readonly songId = this.config.data.songId;
  private readonly userId = this.config.data.userId;
  public readonly playlists = signal<PlaylistListResponse | null>(null);
  private readonly destroyRef = inject(DestroyRef);
  private readonly playlistQuery = { ...this.userService.playlistsQuery };
  public readonly searchTerm = new FormControl('');

  ngOnInit(): void {
    this.searchTerm.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      startWith(''),
      filter(x => !isNullOrUndefined(x)),
      switchMap((value: string) => {
        this.playlistQuery.searchTerm = value;
        return this.userId ? this.userService.getUserPlaylists(this.userId, this.playlistQuery) : EMPTY
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((value) => {
      this.playlists.set(value);
    });
  }

  loadMore() {
    this.playlistQuery.page = this.playlists()!.page + 1;
    this.userService.getUserPlaylists(this.userId, this.playlistQuery).subscribe((value) => {
      value.items = [...this.playlists()!.items, ...value.items];
      this.playlists.set(value);
    });
  }

  addToPlaylist(event: ListboxChangeEvent) {
    const { originalEvent, value } = event;

    const model: UpsertSongPlaylistRequest = {
      playlistId: value.playlistId,
      songId: this.songId,
    }

    this.playlistService.addSongToPlaylist(model).subscribe();
    this.dialogRef.close();
  }

  get lastPlaylistItem() {
    const playlists = this.playlists()?.items;
    return playlists && playlists.length > 0 ? playlists[playlists.length - 1] : null;
  }
}
