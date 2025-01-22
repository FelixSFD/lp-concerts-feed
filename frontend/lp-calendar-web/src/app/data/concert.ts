export class Concert {
  id: string | undefined;
  status: string = "PUBLISHED";
  postedStartTime: string | undefined;
  timeZoneId: string | undefined;

  country: string | undefined;
  state: string | undefined;
  city: string | undefined;
  venue: string | undefined;

  tourName: string | undefined;
}
