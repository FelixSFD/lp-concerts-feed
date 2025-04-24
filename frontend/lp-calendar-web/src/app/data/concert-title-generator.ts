/// Class to generate titles based on concert data
import {Concert} from './concert';

export class ConcertTitleGenerator {
  public static getTitleFor(concert: Concert) {
    if (concert.customTitle) {
      return concert.customTitle;
    } else {
      if (concert.tourName != undefined) {
        return concert.tourName + ": " + concert.city;
      } else {
        return "Linkin Park Concert: " + concert.city
      }
    }
  }
}
