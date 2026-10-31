import { ResolveFn } from '@angular/router';
import { ConcertDetailsDto, ProblemDetailsDto, SetlistDto } from '../modules/lpshows-api/v3';
import { ErrorResponseDto } from '../modules/lpshows-api';
import { inject } from '@angular/core';
import { ToursService } from '../services/tours.service';
import { getUseCacheFromRouteData } from '../helper/cache-parameter-helper';
import { catchError, of } from 'rxjs';
import { SetlistsService } from '../services/setlists.service';

export const setlistsForConcertResolver: ResolveFn<SetlistDto | ProblemDetailsDto> = async (route) => {
  const setlistsService = inject(SetlistsService);
  const concertId = route.paramMap.get('id')!;
  try {
    const setlists = await setlistsService.getSetlistsForConcert(concertId, getUseCacheFromRouteData(route.data));
    return setlists;
  } catch (e) {
    console.error('Failed to load setlists for concert:', e);
    let problemDetails = e as ProblemDetailsDto;
    return problemDetails;
  }
};
