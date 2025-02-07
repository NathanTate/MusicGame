import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../../../auth/auth.service';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { ToolbarModule } from 'primeng/toolbar';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { noWhiteSpacesValidator } from '../../../shared/validators/no-whitespaces-validator';
import { SearchService } from '../../services/search.service';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [ButtonModule, ToolbarModule, MenuModule, IconFieldModule, InputIconModule, InputTextModule, ReactiveFormsModule],
  templateUrl: './navbar.component.html',
  styles: `
  .avatar {
    height: 100%;
    width: 100%;
    border-radius: 50%;
    object-fit: cover;
    object-position: center center;
  }
  `
})
export class NavbarComponent implements OnInit {
  public items: MenuItem[] | undefined;
  public form!: FormGroup;

  public readonly authService = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly searchService = inject(SearchService);
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.initMenuItems();
    this.initForm();
  }

  initForm() {
    this.form = this.fb.group({
      searchTerm: [null, [Validators.required, noWhiteSpacesValidator()]]
    })

    this.searchTerm.valueChanges.pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.performSearch();
      })
  }

  performSearch() {
    if (!this.form.valid) {
      return;
    }
    this.searchService.search({ ...this.searchService.searchQuery, searchTerm: this.searchTerm.value })
  }

  initMenuItems() {
    this.items = [
      {
        label: 'Profile',
        icon: 'pi pi-user',
        routerLink: '/profile'
      },
      {
        label: 'Upload',
        icon: 'pi pi-upload',
        routerLink: 'upload-track'
      },
      {
        label: 'Logout',
        icon: 'pi pi-sign-out',
        command: () => this.logout()
      }
    ]
  }

  logout() {
    this.authService.logout();
  }

  get searchTerm() {
    return this.form.controls['searchTerm'];
  }
}
