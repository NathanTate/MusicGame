import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Button } from 'primeng/button';
import { PhotoFormComponent } from '../photo-form/photo-form.component';
import { DialogService, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { PlaylistResponse } from '../../../core/models/playlist/playlistResponse';
import { noWhiteSpacesValidator } from '../../validators/no-whitespaces-validator';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { CheckboxModule } from 'primeng/checkbox';
import { PlaylistService } from '../../../core/services/playlist.service';
import { debounceTime, forkJoin } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-edit-playlist-modal',
  standalone: true,
  imports: [ReactiveFormsModule, Button, CheckboxModule, PhotoFormComponent, InputTextModule, InputTextareaModule],
  templateUrl: './edit-playlist-modal.component.html',
  styleUrl: './edit-playlist-modal.component.scss',
  providers: [DialogService]
})
export class EditPlaylistModalComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly dialogService = inject(DynamicDialogConfig);
  private readonly playlistService = inject(PlaylistService);
  private readonly destroyRef = inject(DestroyRef);

  public form!: FormGroup;
  public readonly isFormSubmitted = signal<boolean>(false);
  public readonly playlistData: PlaylistResponse = this.dialogService.data.playlist;
  private photoFile = signal<File | null | undefined>(undefined);

  ngOnInit(): void {
    this.initForm();
    this.onChanges();
  }

  onChanges() {
    this.name.valueChanges.pipe(debounceTime(200), takeUntilDestroyed(this.destroyRef)).subscribe((name: string) => {
      if (!this.name.hasError('required') && !this.name.hasError('maxlength') && !this.name.hasError('whitespaces')) {
        this.playlistService.isNameAvailable(name).subscribe((available: boolean) => {
          available ? this.name.setErrors(null) : this.name.setErrors({ nameUnavailable: true })
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
    this.form = this.fb.group({
      playlistId: [this.playlistData.playlistId, Validators.required],
      name: [this.playlistData.name, [Validators.required, Validators.maxLength(100), noWhiteSpacesValidator()]],
      description: [this.playlistData.description, [Validators.maxLength(500), noWhiteSpacesValidator()]],
      isPrivate: [this.playlistData.isPrivate, Validators.required]
    })
  }

  onSubmit() {
    this.isFormSubmitted.set(true);

    if (!this.form.valid) {
      this.form.updateValueAndValidity();
      return;
    }

    const photo = this.photoFile();
    const upsertPhoto$ = this.playlistService.upsertPhoto(this.playlistData.playlistId, this.photoFile()!);
    const deletePhoto$ = this.playlistService.deletePhoto(this.playlistData.playlistId);
    const editPlaylist$ = this.playlistService.updatePlaylist(this.form.value);

    if (photo) {
      forkJoin([editPlaylist$, upsertPhoto$]).subscribe()
    } else if (photo === null) {
      forkJoin([editPlaylist$, deletePhoto$]).subscribe()
    } else {
      editPlaylist$.subscribe()
    }
  }

  get name() {
    return this.form.controls['name'];
  }

  get description() {
    return this.form.controls['description'];
  }

  get isPrivate() {
    return this.form.controls['isPrivate'];
  }
}
