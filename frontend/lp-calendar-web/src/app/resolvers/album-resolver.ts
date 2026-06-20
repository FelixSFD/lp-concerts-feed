import {ResolveFn} from '@angular/router';
import {inject} from '@angular/core';
import {AlbumDto, ErrorResponseDto} from '../modules/lpshows-api';
import {catchError, of} from 'rxjs';
import {AlbumsService} from '../services/music/albums.service';

export const albumResolver: ResolveFn<AlbumDto | ErrorResponseDto> = (route) => {
  const albumsService = inject(AlbumsService);
  const albumId = route.paramMap.get('id')!;
  return albumsService.getAlbum(Number(albumId), false).pipe(
    catchError((err) => {
      let errorResponse: ErrorResponseDto = err.error;
      console.error('Failed to load album:', errorResponse);
      return of(errorResponse);
    }),
  );
};
