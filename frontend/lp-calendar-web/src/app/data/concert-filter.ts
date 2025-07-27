export class ConcertFilter {
  // Name of the tour to filter by
  tour: string | undefined | null;

  // if true, only future concerts will be returned
  onlyFuture: boolean = true;
}
