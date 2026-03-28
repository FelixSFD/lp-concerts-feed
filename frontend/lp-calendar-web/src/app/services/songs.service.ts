import {inject, Injectable} from '@angular/core';
import {AuthService} from '../auth/auth.service';
import {HttpClient} from '@angular/common/http';
import {
  CreateSongMashupRequestDto, CreateSongRequestDto,
  SongDto,
  SongMashupDto,
  SongsService as SongsApiClient,
  SongVariantDto, UpdateSongMashupRequestDto, UpdateSongRequestDto
} from '../modules/lpshows-api';
import {Observable} from 'rxjs';
import {Guid} from 'guid-typescript';

@Injectable({
  providedIn: 'root',
})
export class SongsService {
  private readonly authService = inject(AuthService);

  constructor(private httpClient: HttpClient, private songsApiClient: SongsApiClient) { }


  /**
   * Creates a new song
   * @param request Song data
   */
  public createSong(request: CreateSongRequestDto): Observable<SongDto> {
    return this.songsApiClient.createSong(request);
  }


  /**
   * Updates a song
   * @param id ID of the song to modify
   * @param request new Song data
   */
  public updateSong(id: number, request: UpdateSongRequestDto): Observable<SongDto> {
    return this.songsApiClient.updateSong(id, request);
  }


  /**
   * Returns a song by its ID
   * @param id song to find
   * @param cached true if cache can be used
   */
  public getSong(id: number, cached: boolean): Observable<SongDto> {
    if (cached) {
      return this.songsApiClient.getSong(id);
    } else {
      return this.songsApiClient.getSong(id, Guid.create().toString(), "body", false);
    }
  }


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
   * Deletes a song
   * @param id Song to delete
   */
  public deleteSong(id: number): Observable<any> {
    return this.songsApiClient.deleteSong(id);
  }


  /**
   * Returns all mashups
   */
  public getAllMashups(cached: boolean) : Observable<SongMashupDto[]> {
    if (cached) {
      return this.songsApiClient.getSongMashups();
    } else {
      return this.songsApiClient.getSongMashups(Guid.create().toString(), "body", false);
    }
  }


  /**
   * Returns all variants of a song
   */
  public getVariantsOfSong(songId: number) : Observable<SongVariantDto[]> {
    return this.songsApiClient.getVariantsOfSong(songId);
  }


  /**
   * Creates a mashup of two or more songs
   * @param request Mashup data
   */
  public createMashup(request: CreateSongMashupRequestDto): Observable<SongMashupDto> {
    return this.songsApiClient.createSongMashup(request);
  }


  /**
   * Updates a mashup of two or more songs
   * @param id Mashup to update
   * @param request Mashup data
   */
  public updateMashup(id: number, request: UpdateSongMashupRequestDto): Observable<SongMashupDto> {
    return this.songsApiClient.updateSongMashup(id, request);
  }


  /**
   * Deletes a mashup
   * @param id Mashup to delete
   */
  public deleteMashup(id: number): Observable<any> {
    return this.songsApiClient.deleteSongMashup(id);
  }


  /**
   * Returns a mashup by its ID
   * @param id Mashup to find
   * @param cached true if cache can be used
   */
  public getMashup(id: number, cached: boolean): Observable<SongMashupDto> {
    if (cached) {
      return this.songsApiClient.getSongMashupById(id);
    } else {
      return this.songsApiClient.getSongMashupById(id, Guid.create().toString(), "body", false);
    }
  }
}
