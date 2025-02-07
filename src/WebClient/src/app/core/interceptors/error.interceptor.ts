import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { catchError, Observable } from 'rxjs';

export function errorInterceptor(req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> {
  const messageService = inject(MessageService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error && !req.url.includes('login') && !req.url.includes('register')) {
        if (error.status === 400) {
          messageService.add({ severity: 'error', summary: error.error.title, detail: error.error.message });
          throw error;
        }
        if (error.status === 500) {
          messageService.add({ severity: 'error', summary: `Internal Error ${error.status}`, detail: error.error })
          throw error;
        }
        if (error.status === 0) {
          messageService.add({ severity: 'error', summary: `Server is offline ${error.status}`, detail: error.message })
          throw error;
        }
        if (error.status === 404) {
          router.navigate(['/not-found']);
          throw error;
        }
      }

      throw error;
    })
  )
};


