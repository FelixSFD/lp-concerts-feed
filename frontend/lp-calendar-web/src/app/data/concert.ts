import {ConcertDto} from '../modules/lpshows-api';

export class Concert {
  id: string | undefined;
  status: string = "PUBLISHED";
  postedStartTime: string | undefined;
  isPast: boolean | undefined;
  timeZoneId: string | undefined;

  lpuEarlyEntryConfirmed: boolean = false;

  lpuEarlyEntryTime: string | undefined;
  doorsTime: string | undefined;
  mainStageTime: string | undefined;
  expectedSetDuration: number | undefined;

  country: string | undefined;
  state: string | undefined;
  city: string | undefined;
  venue: string | undefined;

  showType: string | undefined;
  tourName: string | undefined;
  customTitle: string | undefined;

  // Location of the venue
  venueLatitude: number | undefined;
  venueLongitude: number | undefined;

  // images
  scheduleImageFile: string | undefined;


  static fromApi(other: ConcertDto): Concert {
    return {
      id: other.id,
      status: other.status ?? "PUBLISHED",
      postedStartTime: other.postedStartTime,
      isPast: other.isPast == "true", // TODO: fix OpenAPI definition
      timeZoneId: other.timeZoneId,
      lpuEarlyEntryConfirmed: other.lpuEarlyEntryConfirmed ?? false,
      lpuEarlyEntryTime: other.lpuEarlyEntryTime,
      doorsTime: other.doorsTime,
      mainStageTime: other.mainStageTime,
      expectedSetDuration: other.expectedSetDuration,
      country: other.country,
      state: other.state ?? undefined,
      city: other.city,
      venue: other.venue ?? undefined,
      showType: other.showType,
      tourName: other.tourName ?? undefined,
      customTitle: other.customTitle ?? undefined,
      venueLatitude: other.venueLatitude,
      venueLongitude: other.venueLongitude,
      scheduleImageFile: other.scheduleImageFile ?? undefined
    }
  }
}
