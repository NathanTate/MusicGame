import { HttpClient } from "@angular/common/http";
import { inject, Injectable, signal } from "@angular/core";
import { environment } from "../../environments/environment.development";
import { ResetPassword } from "./models/resetPassword";
import { Register } from "./models/register";
import { Login } from "./models/login";
import { TokenDto, TokenWrapper } from "./models/tokenDto";
import { tap } from "rxjs";
import { AuthData } from "./models/authData";
import { Router } from "@angular/router";
import { AudioService } from "../core/services/audio.service";
import { PlaybackStateService } from "../core/services/playbackState.service";

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private _baseUrl = environment.apiUrl + 'account/'
  private _authData = signal<AuthData | null>(null);
  authData = this._authData.asReadonly();

  private http = inject(HttpClient);
  private router = inject(Router);
  private audioService = inject(AudioService);
  private playbackStateService = inject(PlaybackStateService);

  constructor() {
    const authDataString = localStorage.getItem("authData");
    if (authDataString) {
      this._authData.set(JSON.parse(authDataString) as AuthData);
    }

    setInterval(() => {
      const refreshToken = this.authData()?.tokens?.refreshToken;
      if (refreshToken) {
        this.refreshToken(refreshToken).subscribe();
      }
    }, 250000)
  }

  register(model: Register) {
    return this.http.post<string>(this._baseUrl + 'register', model)
  }

  login(login: Login) {
    return this.http.post<AuthData>(this._baseUrl + 'login', login).pipe(
      tap((data) => {
        this.setAuthData(data)
      })
    )
  }

  logout() {
    this._authData.set(null);
    this.audioService.stop();
    this.playbackStateService.resetState();
    localStorage.removeItem('authData');
    this.router.navigate(['/login']);
  }

  refreshToken(model: TokenDto) {
    return this.http.post<TokenWrapper>(this._baseUrl + 'refreshToken', model).pipe(
      tap((tokens) => {
        this._authData.update(val => val ? ({ ...val, tokens }) : val)
        this.setAuthData(this._authData());
      })
    )
  }

  confirmEmail(email: string, token: string) {
    return this.http.post<void>(this._baseUrl + 'confirm-email', { email: email, token: token })
  }

  resendConfirmationEmail(email: string) {
    return this.http.post<void>(this._baseUrl + 'resend-confirmation-email', { email: email })
  }

  resetPassword(model: ResetPassword) {
    this.http.post<void>(this._baseUrl + 'reset-password', model)
  }

  sendResetPasswordCode(email: string) {
    this.http.post<void>(this._baseUrl + 'send-reset-passwordCode', { email: email });
  }

  setAuthData(data: any) {
    this._authData.set(data);
    localStorage.setItem('authData', JSON.stringify(this.authData()));
  }

  isAuthenticated() {
    return this.authData() && Date.now() < new Date(this.authData()!.tokens?.refreshToken?.expiresAt)?.getTime();
  }
}