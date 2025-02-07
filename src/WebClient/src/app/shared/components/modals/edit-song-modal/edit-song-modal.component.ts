import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { SongService } from '../../../../core/services/song.service';
import { DialogService, DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { SongResponse } from '../../../../core/models/song/songResponse';
import { noWhiteSpacesValidator } from '../../../validators/no-whitespaces-validator';
import { debounceTime, distinctUntilChanged, EMPTY, forkJoin, Observable } from 'rxjs';
import { PhotoFormComponent } from '../../photo-form/photo-form.component';
import { Button } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CalendarModule } from 'primeng/calendar';
import { MultiSelectModule } from 'primeng/multiselect';
import { RadioButtonModule } from 'primeng/radiobutton';
import { GenreService } from '../../../../core/services/genre.service';
import { GenreListResponse } from '../../../../core/models/genre/genreListResponse';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-edit-song-modal',
  standalone: true,
  imports: [PhotoFormComponent, Button, RadioButtonModule, ReactiveFormsModule, InputTextModule, CalendarModule, MultiSelectModule, AsyncPipe],
  templateUrl: './edit-song-modal.component.html',
  styleUrl: './edit-song-modal.component.scss',
  providers: [DialogService]
})
export class EditSongModalComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly songService = inject(SongService);
  private readonly config = inject(DynamicDialogConfig);
  private readonly dialogRef = inject(DynamicDialogRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly genreService = inject(GenreService);

  public form!: FormGroup;
  public readonly isFormSubmitted = signal<boolean>(false);
  public readonly songData = signal<SongResponse>(this.config.data.song);
  public genres$: Observable<GenreListResponse> = EMPTY;
  public readonly photoFile = signal<File | null | undefined>(undefined);

  ngOnInit(): void {
    const genresQuery = { ...this.genreService.genresQuery };
    genresQuery.pageSize = 100;
    genresQuery.isSystemDefined = null;

    this.genres$ = this.genreService.getGenres(genresQuery);
    this.initForm();
    this.onChanges();
  }

  onChanges() {
    this.name.valueChanges.pipe(debounceTime(200), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef)).subscribe((name: string) => {
      if (!this.name.hasError('required') && !this.name.hasError('maxlength') && !this.name.hasError('whitespaces') && this.songData().name !== name) {
        this.songService.isNameAvailable(name).subscribe((available: boolean) => {
          available ? this.name.setErrors(null) : this.name.setErrors({ nameUnavailable: true });
        })
      }
    })
  }

  onDeleted() {
    this.photoFile.set(null);
  }

  onPhotoChanged(photo: File) {
    this.photoFile.set(photo);
  }

  initForm() {
    const genreIds = this.songData().genres.map(g => g.genreId);

    this.form = this.fb.group({
      songId: [this.songData().songId, Validators.required],
      name: [this.songData().name, [Validators.required, Validators.maxLength(100), noWhiteSpacesValidator()]],
      releaseDate: [new Date(this.songData().releaseDate), [Validators.required, noWhiteSpacesValidator()]],
      genreIds: [genreIds, [Validators.required]],
      isPrivate: [this.songData().isPrivate, Validators.required]
    });
  }

  onSubmit() {
    this.isFormSubmitted.set(true);

    if (!this.form.valid) {
      this.form.updateValueAndValidity();
      return;
    }

    const photo = this.photoFile();
    const deletePhoto$ = this.songService.deletePhoto(this.songData().songId);
    const editPlaylist$ = this.songService.updateSong(this.form.value);

    if (photo) {
      const upsertPhoto$ = this.songService.upsertPhoto(this.songData().songId, photo);
      forkJoin([editPlaylist$, upsertPhoto$]).subscribe()
    } else if (photo === null) {
      forkJoin([editPlaylist$, deletePhoto$]).subscribe()
    } else {
      editPlaylist$.subscribe()
    }

    this.dialogRef.close();
  }


  get name() {
    return this.form.controls['name'];
  }

  get releaseDate() {
    return this.form.controls['releaseDate'];
  }

  get genreIds() {
    return this.form.controls['genreIds'] as FormArray;
  }

  get isPrivate() {
    return this.form.controls['isPrivate'];
  }

}
