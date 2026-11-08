import {ResolveFn} from '@angular/router';
import {inject} from '@angular/core';
import {ErrorResponseDto, UserDto, UsersService} from '../modules/lpshows-api';
import {catchError, of} from 'rxjs';
import { ProblemDetailsDto, PublicUserDto, UsersApi } from '../modules/lpshows-api/v3';

export const userResolver: ResolveFn<UserDto | ErrorResponseDto> = (route) => {
  const usersService = inject(UsersService);
  const userId = route.paramMap.get('id')!;
  return usersService.getUserById(userId).pipe(
    catchError((err) => {
      let errorResponse: ErrorResponseDto = err.error;
      console.error('Failed to load user:', errorResponse);
      return of(errorResponse);
    }),
  );
};

export const publicUserResolver: ResolveFn<PublicUserDto | ProblemDetailsDto> = (route) => {
  const usersApi = inject(UsersApi);
  const userId = route.paramMap.get('userId')!;
  return usersApi.getPublicUserById(userId).pipe(
    catchError((err) => {
      console.error("Error loading public user:", err);
      let errorResponse = err as ProblemDetailsDto;
      console.error('Failed to load user:', errorResponse);
      return of(errorResponse);
    }),
  );
};
