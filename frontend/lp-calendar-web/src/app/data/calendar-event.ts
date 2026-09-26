import {DateTime} from 'luxon';
import {ConcertDto} from '../modules/lpshows-api';
import {ConcertTitleGenerator} from './concert-title-generator';

/**
 * Builds a single-event .ics file in the browser.
 *
 * The API only exposes a subscription feed (/feed/ical), which covers every
 * concert. Adding one specific show to a calendar therefore has to happen
 * client-side, from the concert we already have loaded.
 */

/** iCalendar wants UTC stamps in the form 20260815T190000Z. */
function toIcsUtc(dateTime: DateTime): string {
  return dateTime.toUTC().toFormat("yyyyLLdd'T'HHmmss'Z'");
}

/** Commas, semicolons and newlines are structural in iCalendar and must be escaped. */
function escapeText(value: string): string {
  return value
    .replace(/\\/g, "\\\\")
    .replace(/;/g, "\\;")
    .replace(/,/g, "\\,")
    .replace(/\r?\n/g, "\\n");
}

/**
 * Lines longer than 75 octets must be folded, continued with a leading space.
 * Calendar apps reject or mangle the event otherwise.
 */
function foldLine(line: string): string {
  if (line.length <= 75) {
    return line;
  }

  const parts: string[] = [line.slice(0, 75)];
  let rest = line.slice(75);

  while (rest.length > 74) {
    parts.push(" " + rest.slice(0, 74));
    rest = rest.slice(74);
  }

  if (rest.length > 0) {
    parts.push(" " + rest);
  }

  return parts.join("\r\n");
}


export function buildConcertIcs(concert: ConcertDto): string | null {
  const start = concert.doorsTime ?? concert.mainStageTime ?? concert.postedStartTime;
  if (!start) {
    return null;
  }

  const zone = concert.timeZoneId ?? undefined;
  const startDateTime = zone ? DateTime.fromISO(start, {zone}) : DateTime.fromISO(start);
  if (!startDateTime.isValid) {
    return null;
  }

  // Prefer stage time + set length for the end; otherwise assume a three hour evening.
  const stage = concert.mainStageTime
    ? (zone ? DateTime.fromISO(concert.mainStageTime, {zone}) : DateTime.fromISO(concert.mainStageTime))
    : null;

  const endDateTime = stage?.isValid && concert.expectedSetDuration
    ? stage.plus({minutes: concert.expectedSetDuration})
    : startDateTime.plus({hours: 3});

  const location = [concert.venue, concert.city, concert.country]
    .filter((part): part is string => (part?.length ?? 0) > 0)
    .join(", ");

  const description: string[] = [];
  if (concert.doorsTime) {
    description.push("Doors: " + (zone ? DateTime.fromISO(concert.doorsTime, {zone}) : DateTime.fromISO(concert.doorsTime)).toFormat("HH:mm"));
  }
  if (concert.lpuEarlyEntryTime) {
    const label = concert.lpuEarlyEntryConfirmed ? "LPU early entry" : "LPU early entry (not confirmed)";
    description.push(label + ": " + (zone ? DateTime.fromISO(concert.lpuEarlyEntryTime, {zone}) : DateTime.fromISO(concert.lpuEarlyEntryTime)).toFormat("HH:mm"));
  }
  if (stage?.isValid) {
    description.push("Linkin Park on stage: " + stage.toFormat("HH:mm"));
  }
  description.push("Times from lpshows.live — a fan-maintained calendar.");

  const lines = [
    "BEGIN:VCALENDAR",
    "VERSION:2.0",
    "PRODID:-//lpshows.live//Concert Calendar//EN",
    "CALSCALE:GREGORIAN",
    "METHOD:PUBLISH",
    "BEGIN:VEVENT",
    "UID:" + (concert.id ?? startDateTime.toMillis().toString()) + "@lpshows.live",
    "DTSTAMP:" + toIcsUtc(DateTime.utc()),
    "DTSTART:" + toIcsUtc(startDateTime),
    "DTEND:" + toIcsUtc(endDateTime),
    "SUMMARY:" + escapeText(ConcertTitleGenerator.getVenueTitleFor(concert)),
    "DESCRIPTION:" + escapeText(description.join("\n")),
  ];

  if (location.length > 0) {
    lines.push("LOCATION:" + escapeText(location));
  }

  if (concert.venueLatitude && concert.venueLongitude) {
    lines.push(`GEO:${concert.venueLatitude};${concert.venueLongitude}`);
  }

  lines.push("END:VEVENT", "END:VCALENDAR");

  return lines.map(foldLine).join("\r\n");
}


/** Hands the generated file to the browser as a download. */
export function downloadConcertIcs(concert: ConcertDto): boolean {
  const ics = buildConcertIcs(concert);
  if (ics == null) {
    return false;
  }

  const blob = new Blob([ics], {type: "text/calendar;charset=utf-8"});
  const url = URL.createObjectURL(blob);

  const link = document.createElement("a");
  link.href = url;
  link.download = (concert.city ?? "linkin-park-show").toLowerCase().replace(/[^a-z0-9]+/g, "-") + ".ics";
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);

  URL.revokeObjectURL(url);
  return true;
}
