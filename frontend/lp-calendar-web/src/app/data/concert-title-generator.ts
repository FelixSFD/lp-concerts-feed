/// Class to generate titles based on concert data
import {ConcertDto} from '../modules/lpshows-api';
import {ConcertDetailsDto} from '../modules/lpshows-api/v3';

export class ConcertTitleGenerator {
  public static getTitleFor(concert: ConcertDto | ConcertDetailsDto): string {
    if (concert.customTitle) {
      return concert.customTitle;
    }
    if ('tourName' in concert && typeof concert.tourName === 'string') {
      return concert.tourName + ": " + (concert.city ?? '');
    }
    if ('tour' in concert && concert.tour?.name) {
      const city = concert.venue?.city?.name ?? '';
      return concert.tour.name + ": " + city;
    }
    const city = 'city' in concert && typeof concert.city === 'string'
      ? concert.city
      : ('venue' in concert && typeof concert.venue === 'object' && concert.venue !== null ? concert.venue.city?.name : '');
    return "Linkin Park Concert: " + (city ?? '');
  }

  public static getVenueTitleFor(concert: ConcertDto | ConcertDetailsDto): string {
    if (concert.customTitle) {
      return concert.customTitle;
    }
    if ('venue' in concert && typeof concert.venue === 'string') {
      return "Linkin Park · " + concert.venue;
    }
    if ('venue' in concert && typeof concert.venue === 'object' && concert.venue?.currentName) {
      return "Linkin Park · " + concert.venue.currentName;
    }
    return this.getTitleFor(concert);
  }
}
