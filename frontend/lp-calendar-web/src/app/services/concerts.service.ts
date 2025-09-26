import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable, switchMap} from 'rxjs';
import { Guid } from 'guid-typescript';
import {ConcertFilter} from '../data/concert-filter';
import {
  AdjacentConcertsResponseDto, ConcertBookmarkUpdateRequestDto,
  ConcertDto,
  ConcertFileUploadRequestDto, ConcertFileUploadResponseDto,
  ConcertsService as ConcertsApiClient, GetConcertBookmarkCountsResponseDto
} from '../modules/lpshows-api';
import {AuthService} from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class ConcertsService {
  private readonly authService = inject(AuthService);

  constructor(private httpClient: HttpClient, private concertsApiClient: ConcertsApiClient) { }


  /**
   * @deprecated Use getFilteredConcerts instead
   */
  getConcerts(cached: boolean, onlyFuture: boolean) : Observable<ConcertDto[]> {
    return this.getFilteredConcerts(null, cached);
  }


  getFilteredConcerts(filter: ConcertFilter | null, cached: boolean) : Observable<ConcertDto[]> {
    if (!cached) {
      // disable caching
      return this.concertsApiClient.getConcerts(filter?.tour ?? undefined, filter?.dateFrom?.toISO() ?? undefined, filter?.dateTo?.toISO() ?? undefined, Guid.create().toString(), "body", false);
    }

    return this.concertsApiClient.getConcerts(filter?.tour ?? undefined, filter?.dateFrom?.toISO() ?? undefined, filter?.dateTo?.toISO() ?? undefined, undefined, "body", false);
  }


  getNextConcert() : Observable<ConcertDto> {
    return this.concertsApiClient.getNextConcert("body", false);
  }


  getConcert(concertId: string, cached: boolean = true) : Observable<ConcertDto> {
    if (!cached) {
      // disable caching
      return this.concertsApiClient.getConcertById(concertId, Guid.create().toString(), "body", false);
    }

    return this.concertsApiClient.getConcertById(concertId, undefined, "body", false);
  }


  addConcert(concert: ConcertDto) {
    concert.status = "PUBLISHED";
    return this.concertsApiClient.addOrUpdateConcert(concert);
  }


  deleteConcert(concertId: string) {
    return this.concertsApiClient.deleteConcert(concertId);
  }


  uploadConcertSchedule(concertId: string, imageFile: File) {
    let getUrlRequest: ConcertFileUploadRequestDto = {};
    getUrlRequest.concertId = concertId;
    getUrlRequest.contentType = imageFile.type;
    getUrlRequest.type = "ConcertSchedule";

    return this.concertsApiClient.getUrlForConcertFileUpload(getUrlRequest)
      .pipe(
        switchMap((response) => {
          return this.httpClient.put(response.uploadUrl!, imageFile);
        })
      )
  }


  /**
   * Returns the previous and next ID based on the ID passed into the method
   * @param currentId ID of the current concert
   */
  getAdjacentConcerts(currentId: string) : Observable<AdjacentConcertsResponseDto> {
    return this.concertsApiClient.getAdjacentConcertsForId(currentId);
  }


  /**
   * Returns the number of bookmarks for a concert and the status the current user has set
   * @param concertId ID of the concert
   */
  getBookmarksForConcert(concertId: string): Observable<GetConcertBookmarkCountsResponseDto> {
    return this.authService.isAuthenticated().pipe(
      switchMap((isAuthenticated) => {
        if (isAuthenticated) {
          return this.concertsApiClient.getBookmarkStatusForConcert(concertId);
        } else {
          return this.concertsApiClient.getBookmarkCountForConcert(concertId);
        }
      })
    )
  }


  /**
   * Set the bookmark status for a concert and the current user
   * @param concertId ID of the concert
   * @param status status
   */
  setBookmarksForConcert(concertId: string, status: ConcertBookmarkUpdateRequestDto.StatusEnum) {
    console.log("setBookmarksForConcert ", concertId, status);

    let req: ConcertBookmarkUpdateRequestDto = {
      status: status
    };

    return this.concertsApiClient.setBookmarkOnConcert(concertId, req);
  }
}
