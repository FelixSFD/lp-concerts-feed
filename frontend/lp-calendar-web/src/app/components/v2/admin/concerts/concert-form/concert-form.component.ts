import { Component, EventEmitter, inject, Input, OnInit, Output, signal } from '@angular/core';
import { MessageService } from 'primeng/api';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConcertDetailsDto, ConcertStatusValueDto, TourDto, VenueDto } from '../../../../../modules/lpshows-api/v3';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Divider } from 'primeng/divider';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { NgTemplateOutlet } from '@angular/common';
import { SelectConcertTypeComponent } from '../select-concert-type/select-concert-type.component';
import { SelectTourComponent } from '../select-tour/select-tour.component';
import { SelectTourLegComponent } from '../select-tour-leg/select-tour-leg.component';
import { SelectVenueComponent } from '../select-venue/select-venue.component';
import { DatePicker } from 'primeng/datepicker';
import { Select } from 'primeng/select';
import timezones, { TimeZone } from 'timezones-list';
import { DateTime } from 'luxon';
import { ConcertStatus } from '../../../../../data/concert-status';

@Component({
  selector: 'app-concert-form',
  imports: [
    Button,
    Card,
    Divider,
    FloatLabel,
    InputText,
    NgTemplateOutlet,
    ReactiveFormsModule,
    SelectConcertTypeComponent,
    SelectTourComponent,
    SelectTourLegComponent,
    SelectVenueComponent,
    DatePicker,
    Select,
  ],
  templateUrl: './concert-form.component.html',
  styleUrl: './concert-form.component.css',
})
export class ConcertFormComponent implements OnInit {
  private messageService = inject(MessageService);
  private formBuilder = inject(FormBuilder);

  @Input("is-saving")
  isSaving$: boolean = false;

  /**
   * true, if the form is "standalone", meaning it manages its own layout and has a save-button
   */
  @Input("standalone")
  standalone$: boolean = true;

  @Input("is-edit")
  isEdit$: boolean = false;

  @Output("saveClicked")
  saveClicked = new EventEmitter<ConcertFormContent>();

  venueTimezone = signal<TimeZone | null>(null);

  selectedTour = signal<TourDto | null>(null);

  concertForm = this.formBuilder.group({
    concertStatus: new FormControl<ConcertStatusValueDto>(ConcertStatusValueDto.Planned, [Validators.required]),
    customTitle: new FormControl<string>(''),
    concertTypeId: new FormControl<number | null>(null, [Validators.required]),
    tour: new FormControl<TourDto | null>(null, []),
    tourLegId: new FormControl<string | null>(null),
    venue: new FormControl<VenueDto | null>(null, [Validators.required]),
    postedStartTime: new FormControl<Date | null>(null, [Validators.required]),
    lpuEarlyEntryConfirmed: new FormControl(false, []),
    lpuEarlyEntryTime: new FormControl('', []),
    doorsTime: new FormControl('', []),
    lpStageTime: new FormControl('', []),
    expectedSetDuration: new FormControl('', []),
  });

  protected concertStatusValues: ConcertStatus[] = ConcertStatus.allValues;

  ngOnInit() {
    this.concertForm.controls.tour.valueChanges.subscribe((tour) => {
      console.debug('ConcertFormComponent tour changed', tour);
      this.selectedTour.set(tour);
    });

    this.concertForm.controls.venue.valueChanges.subscribe((venue) => {
      console.debug("Venue changed: ", venue);
      console.debug("All timezones:", timezones);
      this.venueTimezone.set(timezones.find(tz => tz.tzCode === venue?.timeZoneId) ?? null);
    });
  }

  onSaveClicked() {
    let content = this.readFromForm();
    if (content) {
      this.saveClicked.emit(content);
    }
  }

  public readFromForm(): ConcertFormContent | null {
    const status = this.concertForm.controls.concertStatus.value;
    let customTitle = this.concertForm.controls.customTitle.value?.valueOf()?.trim();
    let concertTypeId = this.concertForm.controls.concertTypeId.value;
    let tourId = this.concertForm.controls.tour.value?.id;
    let tourLegId = this.concertForm.controls.tourLegId.value;
    let venueId = this.concertForm.controls.venue.value?.id;
    let timezone = this.concertForm.controls.venue.value?.timeZoneId;
    const postedStartTime = this.concertForm.value.postedStartTime!;
    const doorTime = this.concertForm.value.doorsTime;
    const mainStageTime = this.concertForm.value.lpStageTime;

    // Expected set duration
    let expectedSetDuration = this.convertH2M(this.concertForm.value.expectedSetDuration?.valueOf() ?? "00:00");

    if (status == null) {
      this.messageService.add({
        severity: 'error',
        summary: 'Concert status is required',
      });
      return null;
    }

    if (concertTypeId == null) {
      this.messageService.add({
        severity: 'error',
        summary: 'Concert type is required',
      });
      return null;
    }

    if (venueId == null) {
      this.messageService.add({
        severity: 'error',
        summary: 'Venue is required',
      });
      return null;
    }

    if (timezone == null) {
      this.messageService.add({
        severity: 'error',
        summary: 'Timezone is required',
      });
      return null;
    }

    // Convert to the selected timezone
    const localDateTime = DateTime.fromJSDate(postedStartTime); // Interpret as local datetime
    const zonedDateTime = localDateTime.setZone(timezone!, {keepLocalTime: true});

    console.log('Original datetime-local value:', postedStartTime);
    console.log('Converted datetime in selected timezone:', zonedDateTime.toString());

    // Normal Doors Time
    let doorsTime = this.concertForm.value.doorsTime?.valueOf();
    let doorsDateTime: DateTime | null = null;
    if (doorsTime != null && doorsTime.length > 0) {
      doorsDateTime = zonedDateTime.set(DateTime.fromFormat(doorsTime, 'hh:mm').toObject());
      // weird timezone issues can cause the doors time to be on the next day. That's why we need to fix the date just to be sure
      doorsDateTime = doorsDateTime.set({day: localDateTime.day, month: localDateTime.month, year: localDateTime.year});
    }

    // LP stage time
    let lpStageTime = this.concertForm.value.lpStageTime?.valueOf();
    let lpStageDateTime: DateTime | null = null;
    console.debug("lpStageTime:", lpStageTime);
    if (lpStageTime != null && lpStageTime.length > 0) {
      lpStageDateTime = zonedDateTime.set(DateTime.fromFormat(lpStageTime, 'hh:mm').toObject());
      // weird timezone issues can cause the LPU time to be on the next day. That's why we need to fix the date just to be sure
      lpStageDateTime = lpStageDateTime.set({day: localDateTime.day, month: localDateTime.month, year: localDateTime.year});
    }

    return {
      status: status,
      customTitle: customTitle,
      concertTypeId: concertTypeId,
      tourId: tourId ?? null,
      tourLegId: tourLegId ?? null,
      venueId: venueId ?? null,
      postedStartTime: zonedDateTime,
      timezone: timezone,
      mainStageTime: lpStageDateTime,
      doorsTime: doorsDateTime,
      expectedSetDuration: expectedSetDuration,
    };
  }

  public fillFormWith(concert: ConcertDetailsDto) {
    console.debug("Filling form with concert: ", concert);
    let postedStartDateTimeUtc = concert.postedStartTime == undefined ? null : DateTime.fromISO(concert.postedStartTime);
    let postedStartDateTime = postedStartDateTimeUtc?.setZone(concert.venue.timeZoneId!, {keepLocalTime: false})
    console.debug("Posted start time: " + postedStartDateTime);

    let lpuEarlyEntryDateTimeUtc = concert.lpuEarlyEntryTime == undefined ? null : DateTime.fromISO(concert.lpuEarlyEntryTime);
    let lpuEarlyEntryDateTime = lpuEarlyEntryDateTimeUtc?.setZone(concert.venue.timeZoneId!, {keepLocalTime: false})
    let lpuEarlyEntryDateTimeIsoStr = lpuEarlyEntryDateTime?.toISOTime();
    console.log("LPU EE: " + lpuEarlyEntryDateTimeIsoStr);

    let doorsDateTimeUtc = concert.doorsTime == undefined ? null : DateTime.fromISO(concert.doorsTime);
    let doorsDateTime = doorsDateTimeUtc?.setZone(concert.venue.timeZoneId!, {keepLocalTime: false})
    let doorsDateTimeIsoStr = doorsDateTime?.toISOTime();
    console.log("Doors at: " + doorsDateTimeIsoStr);

    let lpStageDateTimeUtc = concert.mainStageTime == undefined ? null : DateTime.fromISO(concert.mainStageTime);
    let lpStageDateTime = lpStageDateTimeUtc?.setZone(concert.venue.timeZoneId!, {keepLocalTime: false})
    let lpStageDateTimeIsoStr = lpStageDateTime?.toISOTime();
    console.log("LP on stage at: " + lpStageDateTimeIsoStr);

    let setDurationStr = this.convertMinutesToString(Number(concert.expectedSetDurationMinutes));

    console.debug("Has concert status: ", concert.status);
    this.concertForm.controls.concertStatus.setValue(ConcertStatus.allValues.find(s => s.value == concert.status)?.value ?? null);
    this.concertForm.controls.customTitle.setValue(concert.customTitle ?? null);
    this.concertForm.controls.concertTypeId.setValue(concert.concertType?.id ?? null);
    this.concertForm.controls.tour.setValue(concert.tour ?? null);
    this.concertForm.controls.tourLegId.setValue(concert.tourLeg?.id ?? null);
    this.concertForm.controls.venue.setValue(concert.venue as VenueDto);

    this.concertForm.controls.postedStartTime.setValue(postedStartDateTime?.toJSDate() ?? null);
    this.concertForm.controls.lpStageTime.setValue(doorsDateTimeIsoStr?.substring(0, 5) ?? null);
    this.concertForm.controls.doorsTime.setValue(doorsDateTimeIsoStr?.substring(0, 5) ?? null);
    this.concertForm.controls.expectedSetDuration.setValue(setDurationStr ?? null);

    this.venueTimezone.set(timezones.find(t => t.tzCode == concert.venue?.timeZoneId) ?? null);
  }

  public reset() {
    this.concertForm.reset({
      concertStatus: ConcertStatusValueDto.Planned,
      customTitle: '',
      concertTypeId: null,
      tour: null,
      tourLegId: null,
      venue: null,
    });
  }

  /**
   * Sets the field expectedSetDuration based on minutes
   * @param minutes
   */
  setExpectedSetDuration(minutes: number) {
    let str = this.convertMinutesToString(minutes);
    this.concertForm.controls.expectedSetDuration.setValue(str ?? null);
  }

  private convertMinutesToString(minutes: number | undefined){
    if (minutes != undefined) {
      let setDurationMinutes = minutes % 60;
      let setDurationHours = (minutes - setDurationMinutes) / 60;
      return (setDurationHours < 10 ? "0" : "") + setDurationHours.toString() + ":" + (setDurationMinutes < 10 ? "0" : "") + setDurationMinutes.toString();
    }

    return undefined;
  }

  private convertH2M(timeInHour: string){
    let timeParts = timeInHour.split(":");
    return Number(timeParts[0]) * 60 + Number(timeParts[1]);
  }

  protected readonly timezones = timezones;
}

export class ConcertFormContent {
  status!: ConcertStatusValueDto;
  customTitle?: string | null;
  concertTypeId?: number | null;
  tourId?: string | null;
  tourLegId?: string | null;
  venueId?: string | null;
  timezone!: string;
  postedStartTime!: DateTime;
  doorsTime?: DateTime | null;
  mainStageTime?: DateTime | null;
  expectedSetDuration?: number | null;
}
