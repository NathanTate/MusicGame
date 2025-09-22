import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpRequest } from "@angular/common/http";
import { inject } from "@angular/core";
import { BehaviorSubject, catchError, filter, Observable, switchMap, take, throwError } from "rxjs";
import { AuthService } from "./auth.service";
import { TokenDto, TokenWrapper } from "./models/tokenDto";

export function authInterceptor(req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> {
  const authService = inject(AuthService);

  let isRefreshing = false;
  const refreshTokenSubject = new BehaviorSubject<null | string>(null);
  let authReq = req;

  authReq = addCookieToRequest(authReq);

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

      const refreshToken = authService.authData()?.tokens.refreshToken
      if (refreshToken) {
        return authService.refreshToken(refreshToken).pipe(
          switchMap((refreshToken: TokenWrapper) => {
            isRefreshing = false;

            refreshTokenSubject.next(refreshToken.refreshToken.token);

            return next(addCookieToRequest(req))
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
      switchMap(() => next(addCookieToRequest(req)))
    )
  }

  function addCookieToRequest(req: HttpRequest<unknown>): HttpRequest<unknown> {
    return req.clone({ withCredentials: true })
  }
}
