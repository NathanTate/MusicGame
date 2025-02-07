import { AfterViewInit, Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Button } from 'primeng/button';
import { FileSelectEvent, FileUploadModule } from 'primeng/fileupload';
import { noWhiteSpacesValidator } from '../../shared/validators/no-whitespaces-validator';
import { PrimeNGConfig } from 'primeng/api';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { InputTextModule } from 'primeng/inputtext';
import { RadioButtonModule } from 'primeng/radiobutton';
import { CalendarModule } from 'primeng/calendar';
import { MultiSelectModule } from 'primeng/multiselect';
import { ActivatedRoute, Router } from '@angular/router';
import { GenreResponse } from '../../core/models/genre/genreResponse';
import { PhotoFormComponent } from "../../shared/components/photo-form/photo-form.component";
import { SongService } from '../../core/services/song.service';
import { of, switchMap } from 'rxjs';
import { SongResponse } from '../../core/models/song/songResponse';
import { FormatDurationPipe } from '../../shared/pipes/format-duration.pipe';

@Component({
  selector: 'app-upload-track',
  standalone: true,
  imports: [FileUploadModule, Button, ReactiveFormsModule, InputTextModule, RadioButtonModule,
    CalendarModule, MultiSelectModule, PhotoFormComponent, PhotoFormComponent, FormatDurationPipe],
  templateUrl: './upload-track.component.html',
  styleUrl: './upload-track.component.scss'
})
export class UploadTrackComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly config = inject(PrimeNGConfig);
  private readonly destroyRef = inject(DestroyRef);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly songService = inject(SongService);

  public form!: FormGroup;
  public isFormSubmitted = signal<boolean>(false);
  public genres = signal<GenreResponse[]>([]);
  public photoUrl = signal<string | undefined>(undefined);
  public photoFile = signal<File | null>(null);

  isImage = false;
  public song = signal<File | null>(null);

  ngOnInit(): void {
    this.genres.set(this.activatedRoute.snapshot.data['genres']?.items || [])
    this.initForm();
  }

  onSubmit() {
    this.isFormSubmitted.set(true);

    if (!this.form.valid) {
      this.form.updateValueAndValidity();
      return;
    }

    const photo = this.photoFile();

    this.songService.createSong(this.form.value).pipe(
      switchMap((song: SongResponse) => {
        if (photo) {
          return this.songService.upsertPhoto(song.songId, photo);
        } else {
          return of(song);
        }
      })
    ).subscribe((song: SongResponse) => {
      this.router.navigate(['/track', song.songId]);
    })
  }

  initForm() {
    this.form = this.fb.group({
      name: [null, [Validators.required, Validators.maxLength(100), noWhiteSpacesValidator()]],
      duration: [null, [Validators.required]],
      releaseDate: [null, [Validators.required, noWhiteSpacesValidator()]],
      songFile: [null, Validators.required],
      genreIds: [[], Validators.required],
      isPrivate: [false, Validators.required],
    })

    this.songFile.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((file: File) => {
      this.song.set(file);
    })
  }

  onSongSelected(event: FileSelectEvent) {
    if (event.currentFiles && event.currentFiles.length > 0) {
      const file = event.currentFiles[0];
      this.songFile.setValue(file);
      const url = URL.createObjectURL(file);
      const audio = new Audio(url);
      audio.addEventListener('loadedmetadata', () => {
        this.duration.setValue(Math.round(audio.duration * 1000));
      })
    }
  }

  onPhotoSelected(photo: File) {
    this.photoFile.set(photo);
  }

  onClear(event: Event, callback: any) {
    this.songFile.setValue(null);
    callback();
  }

  formatSize(bytes: number) {
    const k = 1024;
    const dm = 3;
    const sizes = this.config.translation.fileSizeTypes;
    if (!sizes) return;
    if (bytes === 0) {
      return `0 ${sizes[0]}`;
    }

    const i = Math.floor(Math.log(bytes) / Math.log(k));
    const formattedSize = parseFloat((bytes / Math.pow(k, i)).toFixed(dm));

    return `${formattedSize} ${sizes[i]}`;
  }

  get name() {
    return this.form.controls['name'];
  }

  get releaseDate() {
    return this.form.controls['releaseDate'];
  }

  get duration() {
    return this.form.controls['duration'];
  }

  get songFile() {
    return this.form.controls['songFile'];
  }

  get genreIds() {
    return this.form.controls['genreIds'] as FormArray;
  }

  get isPrivate() {
    return this.form.controls['isPrivate'];
  }
}
