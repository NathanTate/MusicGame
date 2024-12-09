import { Component, inject, OnInit, signal } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { noWhiteSpacesValidator } from '../../shared/validators/no-whitespaces-validator';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CardModule, ButtonModule, PasswordModule, InputTextModule, ReactiveFormsModule, DividerModule, RouterLink],
  templateUrl: './register.component.html',
  styles: ``
})
export class RegisterComponent implements OnInit {
  form!: FormGroup;
  isFormSubmitted = signal(false);
  mediumRegex = '^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9])(?=.{6,})'
  strongRegex = '^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9])(?=.{8,})';

  private fb = inject(FormBuilder);

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
