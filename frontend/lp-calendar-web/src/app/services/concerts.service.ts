import { inject, Service } from '@angular/core';
import {
  ConcertDetailsDto,
  ConcertFileUploadResponseDto,
  ConcertsApi,
  ConcertScheduleUploadRequestDto,
  LinkinpediaImportStatusDto
} from '../modules/lpshows-api/v3';
import { addAuthentication } from '../auth/auth.config';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';

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

  async getLinkinpediaImportStatus(): Promise<LinkinpediaImportStatusDto> {
    console.debug("getLinkinpediaImportStatus");
    return firstValueFrom(
      this.concertsApi.getConcertImportStatus()
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
    return firstValueFrom(this.concertsApi.getUrlForConcertFileUpload(concertId, undefined, request));
  }

  /**
   * Returns the details of a concert
   * @param concertId ID of the concert
   */
  getDetailsById(concertId: string): Promise<ConcertDetailsDto> {
    return firstValueFrom(this.concertsApi.getConcertById(concertId));
  }
}
