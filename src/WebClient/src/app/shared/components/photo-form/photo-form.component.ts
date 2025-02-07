import { Component, ElementRef, inject, input, model, output, viewChild } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { DomSanitizer } from '@angular/platform-browser';
import { MenuItem } from 'primeng/api';
import { Button } from 'primeng/button';
import { MenuModule } from 'primeng/menu';

@Component({
  selector: 'app-photo-form',
  standalone: true,
  imports: [MenuModule, Button, ReactiveFormsModule],
  templateUrl: './photo-form.component.html',
  styleUrl: './photo-form.component.scss'
})
export class PhotoFormComponent {
  private readonly sanitizer = inject(DomSanitizer);

  public readonly photoUrl = model<string | undefined>('');
  public readonly showMenu = input<boolean>(true);
  public readonly onDeleted = output<void>();
  public readonly onFileChanged = output<File>();
  public readonly imageControl = new FormControl(null);
  public readonly inputEl = viewChild.required<ElementRef<HTMLInputElement>>('inputEl');

  items: MenuItem[] = [
    {
      label: 'Change photo',
      icon: 'pi pi-image',
      command: () => this.inputEl().nativeElement.click()
    },
    {
      label: 'Delete photo',
      icon: 'pi pi-trash',
      command: () => {
        this.photoUrl.set(undefined)
        this.onDeleted.emit();
      }
    }
  ];

  onImageChanged(event: Event) {
    const element = event.currentTarget as HTMLInputElement;
    let file: File | null = element.files && element.files[0];
    if (file) {
      const photoUrl = this.sanitizer.bypassSecurityTrustUrl(
        window.URL.createObjectURL(file)
      );
      this.photoUrl.set(photoUrl as string);
      this.onFileChanged.emit(file);
    }
  }
}
