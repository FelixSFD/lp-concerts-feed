import {ResolveFn} from '@angular/router';
import {inject} from '@angular/core';
import {ErrorResponseDto, UserDto, UsersService as UsersLegacyApi} from '../modules/lpshows-api';
import { catchError, firstValueFrom, of } from 'rxjs';
import { ProblemDetailsDto, PublicUserDto, UsersApi } from '../modules/lpshows-api/v3';
import { UsersService } from '../services/users.service';

export const userResolver: ResolveFn<UserDto | ErrorResponseDto> = (route) => {
  const usersService = inject(UsersLegacyApi);
  const userId = route.paramMap.get('id')!;
  return usersService.getUserById(userId).pipe(
    catchError((err) => {
      let errorResponse: ErrorResponseDto = err.error;
      console.error('Failed to load user:', errorResponse);
      return of(errorResponse);
    }),
  );
};

export const publicUserResolver: ResolveFn<PublicUserDto | ProblemDetailsDto> = async (route) => {
  try {
    const usersService = inject(UsersService);
    let userId = route.paramMap.get('userId')!;
    if (userId == "me") {
      console.debug("Loading current user");
      const currentUser = await usersService.getCurrentUser();
      userId = currentUser.id;
      console.debug("Loaded current user ID:", userId);
    }

    return await usersService.getPublicProfileById(userId);
  } catch (err) {
    let errorResponse = err as ProblemDetailsDto;
    console.error('Failed to load public user:', errorResponse);
    return errorResponse;
  }
};
