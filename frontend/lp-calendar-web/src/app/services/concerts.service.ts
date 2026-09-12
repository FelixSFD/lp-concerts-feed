import { inject, Service } from '@angular/core';
import { ConcertsApi, LinkinpediaImportStatusDto } from '../modules/lpshows-api/v3';
import { addAuthentication } from '../auth/auth.config';
import { firstValueFrom } from 'rxjs';

@Service()
export class ConcertsService {
  private concertsApi: ConcertsApi = inject(ConcertsApi);
  constructor() {
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
}
