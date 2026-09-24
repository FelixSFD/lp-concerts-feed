import { inject, Service } from '@angular/core';
import {
  ConcertDetailsDto,
  ConcertFileUploadResponseDto, ConcertListResponseDto,
  ConcertsApi,
  ConcertScheduleUploadRequestDto,
  LinkinpediaImportStatusDto, ProblemDetailsDto
} from '../modules/lpshows-api/v3';
import { addAuthentication } from '../auth/auth.config';
import { firstValueFrom, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { getRequestIdParameter } from '../helper/cache-parameter-helper';
import { ConcertFilter } from '../data/concert-filter';
import { HttpErrorResponse } from '@angular/common/http';

@Service()
export class ConcertsService {
  private concertsApi: ConcertsApi = inject(ConcertsApi);

  constructor() {
    this.concertsApi.configuration.basePath = environment.apiBaseUrl;
    addAuthentication(this.concertsApi);
  }

  /**
   * Returns the data that can be imported to create a concert
   * @param wikiPageId ID of the page in Linkinpedia
   */
  async getImportConcertPlanForConcert(wikiPageId: string) {
    console.debug("getImportConcertPlanForConcert", wikiPageId);
    return firstValueFrom(
      this.concertsApi.getConcertImportPlan(wikiPageId)
    );
  }

  async getLinkinpediaImportStatus(cached: boolean = true): Promise<LinkinpediaImportStatusDto> {
    console.debug("getLinkinpediaImportStatus");
    return firstValueFrom(
      this.concertsApi.getConcertImportStatus(getRequestIdParameter(cached))
    );
  }

  /**
   * Returns a presigned URL for uploading a concert schedule file
   * @param concertId ID of the concert
   * @param contentType MIME type of the file
   */
  getScheduleUploadUrlForConcertId(concertId: string, contentType: string): Promise<ConcertFileUploadResponseDto> {
    let request: ConcertScheduleUploadRequestDto = {
      contentType: contentType
    };

    return firstValueFrom(this.concertsApi.getUrlForConcertFileUpload(concertId, getRequestIdParameter(true), request));
  }

  /**
   * Returns the details of a concert
   * @param concertId ID of the concert
   * @param cached Whether to use cached data
   */
  getDetailsById(concertId: string, cached: boolean = true): Promise<ConcertDetailsDto> {
    return firstValueFrom(this.concertsApi.getConcertById(concertId, getRequestIdParameter(cached)));
  }

  /**
   * Returns the details of the next concert
   */
  async getNext(): Promise<ConcertDetailsDto | null> {
    return firstValueFrom(this.concertsApi.getNextConcert()).catch(err => {
      const problem = err as ProblemDetailsDto | null;
      if (problem?.status === 404) {
        console.info('No upcoming concert found');
        return null;
      }

      throw problem;
    });
  }

  /**
   * Returns the details of upcoming concerts
   * @param count Number of concerts to return (maximum 10)
   * @param cached Whether to use cached data
   */
  getUpcoming(count: number = 5, cached: boolean = true): Promise<ConcertDetailsDto[]> {
    return firstValueFrom(this.concertsApi.getUpcomingConcerts(getRequestIdParameter(cached), count));
  }

  /**
   * Returns the details of recent concerts
   * @param count Number of concerts to return (maximum 10)
   * @param cached Whether to use cached data
   */
  getRecent(count: number = 5, cached: boolean = true): Promise<ConcertDetailsDto[]> {
    return firstValueFrom(this.concertsApi.getRecentConcerts(getRequestIdParameter(cached), count));
  }

  getFilteredConcerts(filter: ConcertFilter, limit: number = 100, skip: number = 0, cached: boolean = true): Promise<ConcertListResponseDto> {
    return firstValueFrom(this.concertsApi.getConcerts(getRequestIdParameter(cached), filter.countryCode, filter.country, filter.city, filter.venue, filter.customTitle, undefined, undefined, limit, skip, filter.orderBy));
  }
}
