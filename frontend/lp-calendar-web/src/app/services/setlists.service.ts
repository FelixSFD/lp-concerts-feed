import {inject, Injectable} from '@angular/core';
import {AuthService} from '../auth/auth.service';
import {HttpClient} from '@angular/common/http';
import {
  CreateSetlistRequestDto,
  CreateSetlistResponseDto,
  SetlistsService as SetlistsApiClient
} from '../modules/lpshows-api';
import {Observable} from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SetlistsService {
  private readonly authService = inject(AuthService);

  constructor(private httpClient: HttpClient, private setlistsApiClient: SetlistsApiClient) { }


  /**
   * Creates a new setlist
   * @param request Data of the setlist
   */
  public createSetlist(request: CreateSetlistRequestDto) : Observable<CreateSetlistResponseDto> {
    return this.setlistsApiClient.createSetlist(request);
  }
}
