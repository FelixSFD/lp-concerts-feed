import {ResolveFn} from '@angular/router';
import {inject} from '@angular/core';
import {ErrorResponseDto, SongDto} from '../modules/lpshows-api';
import {catchError, of} from 'rxjs';
import {CommandError} from '@angular/cli/src/commands/mcp/host';
import {SongsService} from '../services/songs.service';

export const songResolver: ResolveFn<SongDto | CommandError> = (route) => {
  const songsService = inject(SongsService);
  const songId = route.paramMap.get('songId')!;
  return songsService.getSong(Number(songId), false).pipe(
    catchError((err) => {
      let errorResponse: ErrorResponseDto = err.error;
      console.error('Failed to load song:', errorResponse);
      return of(new CommandError(errorResponse.message ?? "", [], null));
    }),
  );
};
