import {DateTime} from 'luxon';
import {TourConfig} from './tour-config';

export class ConcertFilter {
  // Name of the tour to filter by
  tour: TourConfig | undefined | null;

  // if true, only future concerts will be returned
  onlyFuture: boolean = false;

  dateFrom: DateTime | null = null;
  dateTo: DateTime | null  = null;

  countryCode?: string | undefined;
  country?: string | undefined;
  city?: string | undefined;
  venue?: string | undefined;
  customTitle?: string | undefined;

  orderBy?: string[] | undefined;
}
