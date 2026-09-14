import { Component, EventEmitter, inject, Input, OnInit, Output, signal, ViewChild } from '@angular/core';
import { MessageService } from 'primeng/api';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  ConcertDetailsDto,
  ConcertStatusValueDto,
  ImportConcertPreviewDto,
  TourDto,
  VenueDto
} from '../../../../../modules/lpshows-api/v3';
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
import { DateTime, Zone } from 'luxon';
import { ConcertStatus } from '../../../../../data/concert-status';
import { InputGroup } from 'primeng/inputgroup';
import { InputGroupAddon } from 'primeng/inputgroupaddon';
import { ConcertsService } from '../../../../../services/concerts.service';
import { Dialog } from 'primeng/dialog';
import {
  ApplyClickedEvent,
  ImportConcertDialogContentComponent
} from '../import-concert-dialog-content/import-concert-dialog-content.component';
import { firstValueFrom } from 'rxjs';
import { ToggleSwitch } from 'primeng/toggleswitch';

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
    InputGroup,
    InputGroupAddon,
    Dialog,
    ImportConcertDialogContentComponent,
    ToggleSwitch,
  ],
  templateUrl: './concert-form.component.html',
  styleUrl: './concert-form.component.css',
})
export class ConcertFormComponent implements OnInit {
  private messageService = inject(MessageService);
  private formBuilder = inject(FormBuilder);
  private concertsService = inject(ConcertsService);

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

  @ViewChild(SelectTourComponent) selectTourComponent?: SelectTourComponent;
  @ViewChild(SelectVenueComponent) selectVenueComponent?: SelectVenueComponent;

  venueTimezone = signal<TimeZone | null>(null);

  selectedTour = signal<TourDto | null>(null);

  isShowingImportDialog = signal(false);
  isImporting = signal(false);
  importPlan = signal<ImportConcertPreviewDto | null>(null);

  concertForm = this.formBuilder.group({
    concertStatus: new FormControl<ConcertStatusValueDto>(ConcertStatusValueDto.Planned, [Validators.required]),
    customTitle: new FormControl<string>(''),
    concertTypeId: new FormControl<number | null>(null, [Validators.required]),
    tour: new FormControl<TourDto | null>(null, []),
    tourLegId: new FormControl<string | null>(null),
    venue: new FormControl<VenueDto | null>(null, [Validators.required]),
    postedStartTime: new FormControl<Date | null>(null, [Validators.required]),
    timeIsPlaceholder: new FormControl(false, []),
    lpuEarlyEntryConfirmed: new FormControl(false, []),
    lpuEarlyEntryTime: new FormControl('', []),
    doorsTime: new FormControl('', []),
    lpStageTime: new FormControl('', []),
    expectedSetDuration: new FormControl<string | null>(null, []),
    linkinpediaUrl: new FormControl<string | null>(null, []),
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
      timeIsPlaceholder: this.concertForm.value.timeIsPlaceholder ?? false,
      timezone: timezone,
      mainStageTime: lpStageDateTime,
      doorsTime: doorsDateTime,
      expectedSetDuration: expectedSetDuration,
      linkinpediaUrl: this.concertForm.value.linkinpediaUrl?.valueOf() ?? null,
    };
  }

  public fillFormWith(concert: ConcertDetailsDto) {
    console.debug("Filling form with concert: ", concert);
    let postedStartDateTimeVenueLocal = concert.postedStartTime == undefined ? null : DateTime.fromISO(concert.postedStartTime, {zone: concert.venue.timeZoneId});
    let postedStartDateTimeClientLocal = postedStartDateTimeVenueLocal?.setZone(DateTime.local().zone, {keepLocalTime: false})
    let postedStartDateTimeJs = postedStartDateTimeVenueLocal == null ? null : new Date(postedStartDateTimeVenueLocal!.year, postedStartDateTimeVenueLocal!.month - 1, postedStartDateTimeVenueLocal?.day, postedStartDateTimeVenueLocal?.hour, postedStartDateTimeVenueLocal?.minute, postedStartDateTimeVenueLocal?.second);
    console.debug("Posted start time raw string: ", concert.postedStartTime);
    console.debug("Posted start time JS Date: ", postedStartDateTimeJs);
    console.debug("Posted start time: ", postedStartDateTimeVenueLocal?.toISO(), postedStartDateTimeClientLocal?.toISO());
    console.debug("Venue time zone: ", concert.venue.timeZoneId);

    let lpuEarlyEntryDateTimeUtc = concert.lpuEarlyEntryTime == undefined ? null : DateTime.fromISO(concert.lpuEarlyEntryTime);
    let lpuEarlyEntryDateTime = lpuEarlyEntryDateTimeUtc?.setZone(concert.venue.timeZoneId!, {keepLocalTime: false})
    let lpuEarlyEntryDateTimeIsoStr = lpuEarlyEntryDateTime?.toISOTime();
    console.log("LPU EE: " + lpuEarlyEntryDateTimeIsoStr);

    console.debug("Doors time string: ", concert.doorsTime);
    let doorsDateTime = concert.doorsTime == undefined ? null : DateTime.fromISO(concert.doorsTime);
    console.debug("doorsDateTime:", doorsDateTime?.toString());
    let doorsDateTimeVenue = doorsDateTime?.setZone(concert.venue.timeZoneId!, {keepLocalTime: false})
    console.debug("doorsDateTimeVenue:", doorsDateTimeVenue?.toString());
    let doorsDateTimeIsoStr = doorsDateTimeVenue?.toISOTime();
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

    this.concertForm.controls.postedStartTime.setValue(postedStartDateTimeJs ?? null);
    this.concertForm.controls.lpStageTime.setValue(lpStageDateTimeIsoStr?.substring(0, 5) ?? null);
    this.concertForm.controls.doorsTime.setValue(doorsDateTimeIsoStr?.substring(0, 5) ?? null);
    this.concertForm.controls.expectedSetDuration.setValue(setDurationStr ?? null);

    this.concertForm.controls.linkinpediaUrl.setValue(concert.linkinpediaUrl ?? null);
    this.concertForm.controls.timeIsPlaceholder.setValue(concert.timeIsPlaceholder ?? false);

    this.venueTimezone.set(timezones.find(t => t.tzCode == concert.venue?.timeZoneId) ?? null);
  }

  public setWikiPageId(wikiPageId: string) {
    this.concertForm.controls.linkinpediaUrl.setValue(`https://linkinpedia.com/wiki/${wikiPageId}`);
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
    let minutes = Number(timeParts[0]) * 60 + Number(timeParts[1]);
    return isNaN(minutes) ? null : minutes;
  }

  openLinkinpediaUrlClicked() {
    let url = this.concertForm.value.linkinpediaUrl?.valueOf();
    if (url?.length == 0) {
      return;
    }

    window.open(url, "_blank");
  }

  async importFromLinkinpediaUrlClicked() {
    let url = this.concertForm.value.linkinpediaUrl?.valueOf() ?? null;
    if (url == null || url?.length == 0) {
      return;
    }

    let wikiPageId = url.split("/").pop();
    let importPlan = await this.concertsService.getImportConcertPlanForConcert(wikiPageId!);
    console.debug("Import plan: ", importPlan);
    this.isShowingImportDialog.set(true);
    this.importPlan.set(importPlan);
  }

  async onApplyImportClicked(evt: ApplyClickedEvent) {
    this.isImporting.set(true);
    console.debug('Applying import: ', evt);
    this.isShowingImportDialog.set(false);

    try {
      // 1. Reload tours and venues in parallel before assigning values
      const reloadTasks = [];
      if (this.selectTourComponent) {
        reloadTasks.push(firstValueFrom(this.selectTourComponent.loadTours()));
      } else {
        console.error("selectTourComponent could not be referenced");
      }
      if (this.selectVenueComponent) {
        reloadTasks.push(firstValueFrom(this.selectVenueComponent.reloadAvailableOptions()));
        console.error("selectVenueComponent could not be referenced");
      }
      await Promise.all(reloadTasks);

      const startTime = evt.postedStartTime;
      const concertType = evt.concertType;
      let importTour = evt.tour;
      const importTourLeg = evt.tourLeg;
      const importVenue = evt.venue;
      const importCustomTitle = evt.customTitle;
      const importConcertStatus = evt.concertStatus;

      if (startTime != null) {
        this.concertForm.controls.postedStartTime.setValue(startTime);
      }
      if (concertType != null) {
        this.concertForm.controls.concertTypeId.setValue(concertType.id ?? null);
      }

      let previousStartTime = this.concertForm.value.postedStartTime;
      if (previousStartTime) {
        this.concertForm.controls.timeIsPlaceholder.setValue(true);
      }

      // 2. Set tour with full updated legs list
      if (importTour != null) {
        // Find the freshly loaded tour so its `legs` array contains newly created tour legs
        const freshTour = this.selectTourComponent?.tours().find(t => t.id === importTour?.id) ?? importTour;
        this.concertForm.controls.tour.setValue(freshTour);
      }

      // 3. Set tour leg (SelectTourLegComponent updates automatically via [tour] binding)
      if (importTourLeg != null) {
        this.concertForm.controls.tourLegId.setValue(importTourLeg.id);
      }

      // 4. Set venue
      if (importVenue != null) {
        // Find the freshly loaded venue to match the options list
        const freshVenue = this.selectVenueComponent?.venues().find(v => v.id === importVenue?.id) ?? importVenue;
        this.concertForm.controls.venue.setValue(freshVenue);
      }

      // 5. Set custom title
      if (importCustomTitle) {
        this.concertForm.controls.customTitle.setValue(importCustomTitle);
      }

      // 6. set concert status
      if (importConcertStatus) {
        this.concertForm.controls.concertStatus.setValue(importConcertStatus);
      }
    } catch (error) {
      console.error('Failed to reload data before applying import:', error);
    } finally {
      this.isImporting.set(false);
    }
  }

  protected readonly timezones = timezones;
}

export class ConcertFormContent {
  status!: ConcertStatusValueDto;
  customTitle?: string | null;
  concertTypeId?: number | null;
  tourId?: string | null;
  tourLegId?: string | null;
  venueId?: number | null;
  timezone!: string;
  postedStartTime!: DateTime;
  timeIsPlaceholder!: boolean;
  doorsTime?: DateTime | null;
  mainStageTime?: DateTime | null;
  expectedSetDuration?: number | null;
  linkinpediaUrl?: string | null;
}
