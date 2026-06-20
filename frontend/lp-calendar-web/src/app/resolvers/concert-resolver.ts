import {ResolveFn} from '@angular/router';
import {inject} from '@angular/core';
import {ConcertDto, ConcertsService, ErrorResponseDto} from '../modules/lpshows-api';
import {catchError, of} from 'rxjs';

export const concertResolver: ResolveFn<ConcertDto | ErrorResponseDto> = (route) => {
  const concertService = inject(ConcertsService);
  const concertId = route.paramMap.get('id')!;
  return concertService.getConcertById(concertId).pipe(
    catchError((err) => {
      let errorResponse: ErrorResponseDto = err.error;
      console.error('Failed to load concert:', errorResponse);
      return of(errorResponse);
    }),
  );
};
