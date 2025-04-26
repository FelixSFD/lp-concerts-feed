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
}
