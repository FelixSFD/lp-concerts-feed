import { ResolveFn } from '@angular/router';
import { inject } from '@angular/core';
import { ErrorResponseDto } from '../modules/lpshows-api';
import { catchError, of } from 'rxjs';
import { TourDto } from '../modules/lpshows-api/v3';
import { ToursService } from '../services/tours.service';

export const tourResolver: ResolveFn<TourDto | ErrorResponseDto> = (route) => {
  const toursService = inject(ToursService);
  const tourId = route.paramMap.get('tourId')!;
  return toursService.getTour(tourId).pipe(
    catchError((err) => {
      let errorResponse: ErrorResponseDto = err.error;
      console.error('Failed to load tour:', errorResponse);
      return of(errorResponse);
    }),
  );
};
