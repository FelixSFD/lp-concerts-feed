import { Injectable } from '@angular/core';
import {
  AlbumDto,
  AlbumsService as AlbumsApiClient, CreateAlbumRequestDto, UpdateAlbumRequestDto,
} from '../../modules/lpshows-api';
import {Observable} from 'rxjs';
import {Guid} from 'guid-typescript';

@Injectable({
  providedIn: 'root',
})
export class AlbumsService {
  constructor(private albumsApiClient: AlbumsApiClient) {
  }

  /**
   * Creates a new album
   * @param request Album data
   */
  public createAlbum(request: CreateAlbumRequestDto): Observable<AlbumDto> {
    return this.albumsApiClient.createAlbum(request);
  }

  /**
   * Updates an album
   * @param id ID of the album to modify
   * @param request new Album data
   */
  public updateAlbum(id: number, request: UpdateAlbumRequestDto): Observable<AlbumDto> {
    return this.albumsApiClient.updateAlbum(id, request);
  }

  /**
   * Returns an album by its ID
   * @param id album to find
   * @param cached true if cache can be used
   */
  public getAlbum(id: number, cached: boolean): Observable<AlbumDto> {
    if (cached) {
      return this.albumsApiClient.getAlbum(id);
    } else {
      return this.albumsApiClient.getAlbum(id, Guid.create().toString(), "body", false);
    }
  }


  /**
   * Returns all albums
   */
  public getAllAlbums(cached: boolean) : Observable<AlbumDto[]> {
    if (cached) {
      return this.albumsApiClient.getAllAlbums();
    } else {
      return this.albumsApiClient.getAllAlbums(Guid.create().toString(), "body", false);
    }
  }


  /**
   * Deletes an album
   * @param id Album to delete
   */
  public deleteAlbum(id: number): Observable<any> {
    return this.albumsApiClient.deleteAlbum(id);
  }
}
