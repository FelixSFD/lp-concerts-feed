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
import {Guid} from 'guid-typescript';

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
   * Deletes a setlists by its ID
   * @param setlistId ID of the setlist to delete
   */
  public deleteSetlist(setlistId: number) : Observable<any> {
    return this.setlistsApiClient.deleteSetlist(setlistId);
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
  public getSetlists(cached: boolean) : Observable<SetlistDto[]> {
    if (cached) {
      return this.setlistsApiClient.getSetlists();
    } else {
      return this.setlistsApiClient.getSetlists(Guid.create().toString(), "body", false);
    }
  }
}
