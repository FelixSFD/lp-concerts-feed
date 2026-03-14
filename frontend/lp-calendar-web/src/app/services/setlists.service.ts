import {inject, Injectable} from '@angular/core';
import {AuthService} from '../auth/auth.service';
import {HttpClient} from '@angular/common/http';
import {
  CreateSetlistRequestDto,
  CreateSetlistResponseDto, SetlistDto,
  SetlistsService as SetlistsApiClient,
  ConcertsService as ConcertsApiClient,
} from '../modules/lpshows-api';
import {Observable} from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SetlistsService {
  private readonly authService = inject(AuthService);

  constructor(private httpClient: HttpClient, private setlistsApiClient: SetlistsApiClient, private concertsApiClient: ConcertsApiClient) { }


  /**
   * Creates a new setlist
   * @param request Data of the setlist
   */
  public createSetlist(request: CreateSetlistRequestDto) : Observable<CreateSetlistResponseDto> {
    return this.setlistsApiClient.createSetlist(request);
  }

  /**
   * Returns a setlists by its ID
   * @param setlistId ID of the setlist
   */
  public getSetlist(setlistId: number) : Observable<SetlistDto> {
    return this.setlistsApiClient.getCompleteSetlist(setlistId);
  }

  /**
   * Returns all setlists for a given concert
   * @param concertId ID of the concert
   */
  public getSetlistsForConcert(concertId: string) : Observable<SetlistDto[]> {
    return this.concertsApiClient.getSetlistsForConcert(concertId);
  }

  /**
   * Returns all setlists
   */
  public getSetlists() : Observable<SetlistDto[]> {
    return this.setlistsApiClient.getSetlists();
  }
}
