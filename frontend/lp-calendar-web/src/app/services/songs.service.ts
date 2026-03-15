import {inject, Injectable} from '@angular/core';
import {AuthService} from '../auth/auth.service';
import {HttpClient} from '@angular/common/http';
import {SongDto, SongsService as SongsApiClient, SongVariantDto} from '../modules/lpshows-api';
import {Observable} from 'rxjs';
import {Guid} from 'guid-typescript';

@Injectable({
  providedIn: 'root',
})
export class SongsService {
  private readonly authService = inject(AuthService);

  constructor(private httpClient: HttpClient, private songsApiClient: SongsApiClient) { }


  /**
   * Returns all songs
   */
  public getAllSongs(cached: boolean) : Observable<SongDto[]> {
    if (cached) {
      return this.songsApiClient.getAllSongs();
    } else {
      return this.songsApiClient.getAllSongs(Guid.create().toString(), "body", false);
    }
  }


  /**
   * Returns all variants of a song
   */
  public getVariantsOfSong(songId: number) : Observable<SongVariantDto[]> {
    return this.songsApiClient.getVariantsOfSong(songId);
  }
}
