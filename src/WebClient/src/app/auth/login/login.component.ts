import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { AuthService } from '../auth.service';
import { catchError, throwError } from 'rxjs';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CardModule, PasswordModule, InputTextModule, ReactiveFormsModule, Button, RouterLink, CheckboxModule],
  templateUrl: './login.component.html',
  styles: ``
})
export class LoginComponent implements OnInit {
  form!: FormGroup;
  isFormSubmitted = signal(false);
  emailDefault: string | null = null;
  error = signal('');

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit(): void {
    this.emailDefault = localStorage.getItem('email');
    this.initForm();
  }

  initForm() {
    this.form = this.fb.group({
      email: [this.emailDefault, [Validators.required, Validators.email, Validators.maxLength(32)]],
      password: [null, [Validators.required]],
      rememberMe: [this.emailDefault ? true : false]
    })

    console.log(this.form)
  }

  onSubmit() {
    this.isFormSubmitted.set(true);

    if (!this.form.valid) {
      this.form.updateValueAndValidity();
      return;
    }

    this.authService.login(this.form.value).pipe(catchError(error => {
      this.error.set(error.message);
      return throwError(() => error);
    })).subscribe(() => {
      this.error.set('')
      this.form.controls['rememberMe'] ? localStorage.setItem('email', this.email.value) : localStorage.removeItem('email')
      this.router.navigate(['/'])
    });
  }

  get email() {
    return this.form.controls['email'];
  }

  get password() {
    return this.form.controls['password'];
  }
}
