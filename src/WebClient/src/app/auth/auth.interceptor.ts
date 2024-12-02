import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpRequest } from "@angular/common/http";
import { inject } from "@angular/core";
import { BehaviorSubject, catchError, filter, Observable, switchMap, take, throwError } from "rxjs";
import { AuthService } from "./auth.service";
import { TokenDto } from "./models/tokenDto";

export function authInterceptor(req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> {
  const authService = inject(AuthService);

  let isRefreshing = false;
  const refreshTokenSubject = new BehaviorSubject<null | string>(null);
  const token = authService.authData()?.tokens.accessToken.token;
  let authReq = req;

  if (token) {
    authReq = addTokenHeader(req, token)
  }

  return next(authReq).pipe(catchError(error => {
    if (error instanceof HttpErrorResponse && error.status === 401) {
      return handle401Error(authReq, next);
    }

    return throwError(() => error);
  }))

  function handle401Error(req: HttpRequest<unknown>, next: HttpHandlerFn) {
    if (!isRefreshing) {
      isRefreshing = true;
      refreshTokenSubject.next(null);

      const tokenDto = authService.authData()?.tokens
      if (tokenDto) {
        return authService.refreshToken(tokenDto).pipe(
          switchMap((tokenDto: TokenDto) => {
            isRefreshing = false;

            refreshTokenSubject.next(tokenDto.accessToken.token);

            return next(addTokenHeader(req, tokenDto.accessToken.token))
          }),
          catchError((err) => {
            isRefreshing = false;

            authService.logout();
            return throwError(() => err);
          })
        );
      }
    }

    return refreshTokenSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap((token) => next(addTokenHeader(req, token)))
    )
  }

  function addTokenHeader(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
    return req.clone({ headers: req.headers.append('Authorization', `Bearer ${token}`) });
  }
}
