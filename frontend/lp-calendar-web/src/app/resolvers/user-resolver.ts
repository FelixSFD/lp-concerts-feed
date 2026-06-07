import {ResolveFn} from '@angular/router';
import {inject} from '@angular/core';
import {ErrorResponseDto, UserDto, UsersService} from '../modules/lpshows-api';
import {catchError, of} from 'rxjs';

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
