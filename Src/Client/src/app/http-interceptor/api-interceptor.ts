import { Injectable } from '@angular/core';
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { environment } from '../../environments/environment';
import { AuthService } from '../shared/services';

export const rootApi = `${environment.apiUrl}`;

@Injectable()
export class ApiInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {

  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const apiReq = req.clone({
      url: `${rootApi}${req.url}`
    });
    return next.handle(apiReq).pipe(catchError(err => {
      if (err.status === 401) {
        this.authService.logOut();
      }

      // const error = err.error.message || err.statusText;
      return throwError(err);
    }));
  }
}

