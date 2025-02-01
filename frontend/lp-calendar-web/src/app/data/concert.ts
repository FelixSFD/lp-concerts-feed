export class Concert {
  id: string | undefined;
  status: string = "PUBLISHED";
  postedStartTime: string | undefined;
  timeZoneId: string | undefined;

  lpuEarlyEntryConfirmed: boolean = false;

  lpuEarlyEntryTime: string | undefined;
  doorsTime: string | undefined;
  mainStageTime: string | undefined;

  country: string | undefined;
  state: string | undefined;
  city: string | undefined;
  venue: string | undefined;

  tourName: string | undefined;

  // Location of the venue
  venueLatitude: number | undefined;
  venueLongitude: number | undefined;
}
