import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { ProblemDetailsDto } from '../modules/lpshows-api/v3';

/**
 * Intercepts errors from the API, logs them to the console and if it's a problem details DTO, throws it.
 * @param request
 * @param next
 */
export const apiErrorInterceptor: HttpInterceptorFn = (request, next) => {
  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error instanceof HttpErrorResponse) {
        const problem = error.error as ProblemDetailsDto;

        console.warn('API error:', {
          status: error.status,
          type: problem.type,
          title: problem.title,
          detail: problem.detail,
          instance: problem.instance
        });

        throw problem;
      }

      return throwError(() => error);
    })
  );
}
