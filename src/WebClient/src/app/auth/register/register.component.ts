import { Component, inject, OnInit, signal } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { noWhiteSpacesValidator } from '../../shared/validators/no-whitespaces-validator';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';
import { catchError, throwError } from 'rxjs';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CardModule, ButtonModule, PasswordModule, InputTextModule, ReactiveFormsModule, DividerModule, RouterLink],
  templateUrl: './register.component.html',
  styles: ``
})
export class RegisterComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly messageService = inject(MessageService);

  public form!: FormGroup;
  public isFormSubmitted = signal(false);
  public readonly mediumRegex = '^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9])(?=.{6,})';
  public readonly strongRegex = '^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9])(?=.{8,})';
  public error = signal<string | null>(null);

  ngOnInit(): void {
    this.initForm();
  }

  initForm() {
    this.form = this.fb.group({
      email: [null, [Validators.required, Validators.email, Validators.maxLength(256)]],
      username: [null, [Validators.required, Validators.maxLength(100), noWhiteSpacesValidator()]],
      password: [null, [Validators.required, Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9])(?=.{6,})/)]],
    })
  }

  onSubmit() {
    this.isFormSubmitted.set(true)

    if (!this.form.valid) {
      this.form.updateValueAndValidity();
    }

    this.authService.register(this.form.value).pipe(catchError(error => {
      this.error.set(error.error.message || error.error.errors ? error.error.errors.ValidationError[0] : 'Unknown Error');
      return throwError(() => error);
    })).subscribe(() => {
      localStorage.setItem("email", this.email.value);
      this.error.set(null);
      this.messageService.add({ severity: 'success', summary: 'Confirm your email', detail: 'Please check your email for confirmation link.' })
      this.router.navigate(['/login'])
    })
  }

  get email() {
    return this.form.controls['email'];
  }

  get username() {
    return this.form.controls['username'];
  }

  get password() {
    return this.form.controls['password'];
  }
}
