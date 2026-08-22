import {ResolveFn} from '@angular/router';
import {inject} from '@angular/core';
import { ConcertDto, ConcertsService, ConcertWithSetlistsDto, ErrorResponseDto } from '../modules/lpshows-api';
import {catchError, of} from 'rxjs';
import { ConcertDetailsDto } from '../modules/lpshows-api/v3';
import { ToursService } from '../services/tours.service';

export const concertResolver: ResolveFn<ConcertDetailsDto | ErrorResponseDto> = (route) => {
  const toursService = inject(ToursService);
  const concertId = route.paramMap.get('id')!;
  return toursService.getConcertById(concertId).pipe(
    catchError((err) => {
      let errorResponse: ErrorResponseDto = err.error;
      console.error('Failed to load concert:', errorResponse);
      return of(errorResponse);
    }),
  );
};

export const legacyConcertResolver: ResolveFn<ConcertWithSetlistsDto | ErrorResponseDto> = (route) => {
  const concertService = inject(ConcertsService);
  const concertId = route.paramMap.get('id')!;
  return concertService.getConcertById(concertId).pipe(
    catchError((err) => {
      let errorResponse: ErrorResponseDto = err.error;
      console.error('Failed to load concert form OLD database:', errorResponse);
      return of(errorResponse);
    }),
  );
};
