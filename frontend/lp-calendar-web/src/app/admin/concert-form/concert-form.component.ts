import {
  AfterViewInit,
  Component, ElementRef,
  EventEmitter,
  inject,
  Input, OnChanges,
  OnInit,
  Output, SimpleChanges,
  ViewChild
} from '@angular/core';
import timezones from 'timezones-list';
import {listOfTours, listOfShowTypes} from '../../app.config';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import { NgClass } from '@angular/common';
import {ConcertsService} from '../../services/concerts.service';
import {DateTime} from 'luxon';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {environment} from "../../../environments/environment";
import {ToastrService} from 'ngx-toastr';
import {LocationsService} from '../../services/locations.service';
import {NgbTooltip} from '@ng-bootstrap/ng-bootstrap';
import {ConcertDto} from '../../modules/lpshows-api';
import {load, MapKit, Map as AppleMap, MapKitEvent, AnnotationDragEvent} from '@apple/mapkit-loader';

// This class represents a form for adding and editing concerts
@Component({
  selector: 'app-concert-form',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgClass,
    NgbTooltip
],
  templateUrl: './concert-form.component.html',
  styleUrl: './concert-form.component.css'
})
export class ConcertFormComponent implements OnInit, AfterViewInit, OnChanges {
  private formBuilder = inject(FormBuilder);
  private concertsService = inject(ConcertsService);
  private locationsService = inject(LocationsService);
  private toastrService = inject(ToastrService);

  concertForm = this.formBuilder.group({
    showType: new FormControl('', [Validators.required]),
    tourName: new FormControl('', []),
    customTitle: new FormControl('', []),
    venue: new FormControl('', [Validators.min(3), Validators.required]),
    timezone: new FormControl('', [Validators.required]),
    city: new FormControl('', [Validators.required]),
    state: new FormControl('', []),
    country: new FormControl('', [Validators.required]),
    postedStartTime: new FormControl('', [Validators.required]),
    lpuEarlyEntryConfirmed: new FormControl(false, []),
    lpuEarlyEntryTime: new FormControl('', []),
    doorsTime: new FormControl('', []),
    lpStageTime: new FormControl('', []),
    expectedSetDuration: new FormControl('', []),
    venueLat: new FormControl(0, []),
    venueLong: new FormControl(0, []),
    scheduleImg: new FormControl('', [])
  });

  @Input({ alias: "concert-id" })
  concertId: string | null = null;

  @Input({ alias: "show-clear-button" })
  showClearButton$: boolean = false;

  @Input({ alias: "is-saving" })
  isSaving$: boolean = false;

  @Output('saveClicked')
  saveClicked = new EventEmitter<ConcertDto>();

  // Name of the form-tab that is open at the moment
  activeTabName$: string = "main";

  concert$ : ConcertDto | null = null;

  // Apple Maps
  private mapKit: MapKit | undefined;
  private appleMap: AppleMap | undefined;

  // Selected schedule-file
  protected selectedScheduleFile: File | undefined;

  // Service to check auth information
  private readonly oidcSecurityService = inject(OidcSecurityService);

  hasWriteAccess$ = false;
  scheduleIsUploading$ = false;
  timeZoneIsLoading$ = false;

  constructor() {
    this.oidcSecurityService.checkAuth().subscribe(({ isAuthenticated }) => {
      this.hasWriteAccess$ = isAuthenticated;
    });
  }


  ngOnInit() {
    this.loadConcertForId(this.concertId);
  }


  ngAfterViewInit() {
    //this.initVenueMap();
    this.initAppleMaps();
  }


  ngOnChanges(changes: SimpleChanges) {
    // only update if ID changed
    if (changes.hasOwnProperty("concertId")) {
      this.loadConcertForId(this.concertId);
    }
  }


  private loadConcertForId(id: string | null) {
    // Fetch concert data from API to prefill the form
    if (this.concertId != null) {
      this.concertForm.disable();

      this.concertsService.getConcert(this.concertId, false).subscribe(c => {
        this.concert$ = c;

        this.fillFormWithConcert(c);
        this.concertForm.enable();
      });
    }
  }


  private async initAppleMaps() {
    this.mapKit = await load({
      token: environment.appleMapsToken,
      language: "en-US",
      libraries: ["map", "annotations"],
    });
  }


  @ViewChild('appleMaps')
  set appleMaps(mapElement: ElementRef<HTMLDivElement> | undefined) {
    console.log('appleMaps will be displayed', mapElement);
    if (!mapElement) return;
    if (!this.appleMaps) {
      console.debug('MapKit not initialized yet!');
      this.initAppleMaps().then(() => {
        this.appleMap = new this.mapKit!.Map(mapElement.nativeElement);
        this.addOrMoveMarker(this.concertForm.controls.venueLong.value ?? 0, this.concertForm.controls.venueLat.value ?? 0);
      });
      return;
    }

    console.log("Will set map element: ", mapElement);
    this.appleMap = new this.mapKit!.Map(mapElement.nativeElement);
    this.addOrMoveMarker(this.concertForm.controls.venueLong.value ?? 0, this.concertForm.controls.venueLat.value ?? 0);
  }


  private addOrMoveMarker(lon: number, lat: number) {
    if (!this.appleMap || !this.mapKit) {
      return;
    }
    const annotation = new this.mapKit!.MarkerAnnotation(new this.mapKit!.Coordinate(lat, lon), {
      color: "#c969e0",
      map: this.appleMap,
      draggable: true
    });

    annotation.addEventListener("dragging", this.didDragPin, this);
    this.appleMap?.showItems([annotation]);

    this.zoomToCoordinates(lon, lat);
  }


  private didDragPin(evt: MapKitEvent) {
    let dragEvent = evt as AnnotationDragEvent;
    this.concertForm.controls.venueLat.setValue(dragEvent.coordinate.latitude)
    this.concertForm.controls.venueLong.setValue(dragEvent.coordinate.longitude);
  }


  private zoomToCoordinates(lon: number, lat: number, zoomLevel: number = 11) {
    if (this.appleMap && this.mapKit) {
      this.appleMap.region = new this.mapKit.CoordinateRegion(
        new this.mapKit.Coordinate(lat, lon),
        new this.mapKit.CoordinateSpan(0.06, 0.2)
      );
    }
  }


  private fillFormWithConcert(concert: ConcertDto) {
    let postedStartDateTimeUtc = concert.postedStartTime == undefined ? null : DateTime.fromISO(concert.postedStartTime);
    let postedStartDateTime = postedStartDateTimeUtc?.setZone(concert.timeZoneId!, {keepLocalTime: false})
    let postedStartDateTimeIsoStr = postedStartDateTime?.toISO();

    let lpuEarlyEntryDateTimeUtc = concert.lpuEarlyEntryTime == undefined ? null : DateTime.fromISO(concert.lpuEarlyEntryTime);
    let lpuEarlyEntryDateTime = lpuEarlyEntryDateTimeUtc?.setZone(concert.timeZoneId!, {keepLocalTime: false})
    let lpuEarlyEntryDateTimeIsoStr = lpuEarlyEntryDateTime?.toISOTime();
    console.log("LPU EE: " + lpuEarlyEntryDateTimeIsoStr);

    let doorsDateTimeUtc = concert.doorsTime == undefined ? null : DateTime.fromISO(concert.doorsTime);
    let doorsDateTime = doorsDateTimeUtc?.setZone(concert.timeZoneId!, {keepLocalTime: false})
    let doorsDateTimeIsoStr = doorsDateTime?.toISOTime();
    console.log("Doors at: " + doorsDateTimeIsoStr);

    let lpStageDateTimeUtc = concert.mainStageTime == undefined ? null : DateTime.fromISO(concert.mainStageTime);
    let lpStageDateTime = lpStageDateTimeUtc?.setZone(concert.timeZoneId!, {keepLocalTime: false})
    let lpStageDateTimeIsoStr = lpStageDateTime?.toISOTime();
    console.log("LP on stage at: " + lpStageDateTimeIsoStr);

    let setDurationStr = this.convertMinutesToString(concert.expectedSetDuration);

    this.concertForm.controls.showType.setValue(concert.showType ?? null);
    this.concertForm.controls.tourName.setValue(concert.tourName ?? null);
    this.concertForm.controls.customTitle.setValue(concert.customTitle ?? null);
    this.concertForm.controls.venue.setValue(concert.venue ?? null);
    this.concertForm.controls.city.setValue(concert.city ?? null);
    this.concertForm.controls.state.setValue(concert.state ?? null);
    this.concertForm.controls.country.setValue(concert.country ?? null);
    this.concertForm.controls.postedStartTime.setValue(postedStartDateTimeIsoStr?.substring(0, postedStartDateTimeIsoStr?.length - 6) ?? null);
    this.concertForm.controls.timezone.setValue(concert.timeZoneId ?? null);
    this.concertForm.controls.lpuEarlyEntryConfirmed.setValue(concert.lpuEarlyEntryConfirmed ?? false);
    this.concertForm.controls.lpuEarlyEntryTime.setValue(lpuEarlyEntryDateTimeIsoStr?.substring(0, 5) ?? null);
    this.concertForm.controls.doorsTime.setValue(doorsDateTimeIsoStr?.substring(0, 5) ?? null);
    this.concertForm.controls.lpStageTime.setValue(lpStageDateTimeIsoStr?.substring(0, 5) ?? null);
    this.concertForm.controls.expectedSetDuration.setValue(setDurationStr ?? null);
    this.concertForm.controls.venueLat.setValue(concert.venueLatitude ?? 0)
    this.concertForm.controls.venueLong.setValue(concert.venueLongitude ?? 0)

    this.addOrMoveMarker(concert.venueLongitude ?? 0, concert.venueLatitude ?? 0);
  }


  onSaveClicked() {
    let createdConcert = this.readConcertFromForm();
    console.log("Emitting event for concert");
    this.saveClicked.emit(createdConcert);
  }


  onClearClicked() {
    this.concertForm.reset();
  }


  onGoToCityClicked() {
    let city = this.concertForm.value.city;
    let state = this.concertForm.value.state;
    let country = this.concertForm.value.country;

    if (city == null || country == null) {
      return;
    }

    this.locationsService.getCoordinatesFor(city, state ? state : null, country)
      .subscribe(coordinates => {
        this.zoomToCoordinates(coordinates?.longitude ?? 0, coordinates?.latitude ?? 0);
      });
  }


  onSetPinClicked() {
    let center = this.appleMap?.center;
    this.addOrMoveMarker(center?.longitude ?? 0, center?.latitude ?? 0);
  }


  tryAutoSetVenuePin() {
    let venue = this.concertForm.value.venue;
    let city = this.concertForm.value.city;
    let state = this.concertForm.value.state;
    let country = this.concertForm.value.country;

    if (city == null || country == null || venue == null) {
      return;
    }

    this.locationsService.getCoordinatesForVenue(venue, city, state ? state : null, country)
      .subscribe(coordinates => {
        let lat = coordinates?.latitude ?? 0;
        let lon = coordinates?.longitude ?? 0;

        //this.zoomToCoordinates(lon, lat, 8);
        this.addOrMoveMarker(lon, lat);
        this.concertForm.controls.venueLong.setValue(lon);
        this.concertForm.controls.venueLat.setValue(lat);
      });
  }


  onUpdateTimeZoneClicked() {
    this.timeZoneIsLoading$ = true;

    let city = this.concertForm.value.city;
    let state = this.concertForm.value.state;
    let country = this.concertForm.value.country;

    if (city == null || country == null) {
      return;
    }

    this.locationsService.getCoordinatesFor(city, state ? state : null, country)
      .subscribe(coordinates => {
        console.log("Found coordinates: ", coordinates);
        this.locationsService.getTimeZoneForCoordinates(coordinates?.latitude ?? 0, coordinates?.longitude ?? 0)
          .subscribe(tzObj => {
            console.log("Found timezone: ", tzObj);
            let tz = tzObj.timeZoneId!;
            this.timeZoneIsLoading$ = false;

            if (timezones.map(t => t.tzCode).indexOf(tz, 0) >= 0) {
              this.concertForm.controls.timezone.setValue(tz);
            } else {
              console.error("Invalid timezone returned: ", tz);
              this.toastrService.error(`Timezone '${tz}' found, but it is invalid.`, "Could not load timezone");
            }
          });
      });
  }


  openTab(tabName: string) {
    this.activeTabName$ = tabName;
  }


  private readConcertFromForm() {
    const postedStartTime = this.concertForm.value.postedStartTime!;
    let newConcert: ConcertDto = {};
    newConcert.showType = this.concertForm.value.showType?.valueOf();
    newConcert.tourName = this.concertForm.value.tourName?.valueOf();
    newConcert.customTitle = this.concertForm.value.customTitle?.valueOf();
    newConcert.venue = this.concertForm.value.venue?.valueOf();
    newConcert.city = this.concertForm.value.city?.valueOf();
    newConcert.state = this.concertForm.value.state?.valueOf();
    newConcert.country = this.concertForm.value.country?.valueOf()

    // Read the datetime-local value and selected timezone
    const selectedTimezone = this.concertForm.value.timezone?.valueOf(); // e.g., "America/Los_Angeles"

    // Convert to the selected timezone
    const localDateTime = DateTime.fromISO(postedStartTime); // Interpret as local datetime
    const zonedDateTime = localDateTime.setZone(selectedTimezone, {keepLocalTime: true});

    console.log('Original datetime-local value:', postedStartTime);
    console.log('Converted datetime in selected timezone:', zonedDateTime.toString());

    newConcert.timeZoneId = selectedTimezone;
    newConcert.postedStartTime = zonedDateTime.toISO()!;

    // LPU data
    newConcert.lpuEarlyEntryConfirmed = this.concertForm.value.lpuEarlyEntryConfirmed?.valueOf() ?? false;
    let lpuEarlyEntryTime = this.concertForm.value.lpuEarlyEntryTime?.valueOf();
    if (lpuEarlyEntryTime != null && lpuEarlyEntryTime.length > 0) {
      let lpuEarlyEntryDateTime = zonedDateTime.set(DateTime.fromFormat(lpuEarlyEntryTime, 'hh:mm').toObject());
      // weird timezone issues can cause the LPU time to be on the next day. That's why we need to fix the date just to be sure
      lpuEarlyEntryDateTime = lpuEarlyEntryDateTime.set({day: localDateTime.day, month: localDateTime.month, year: localDateTime.year});
      newConcert.lpuEarlyEntryTime = lpuEarlyEntryDateTime.toISO()!;
    }

    // Normal Doors Time
    let doorsTime = this.concertForm.value.doorsTime?.valueOf();
    if (doorsTime != null && doorsTime.length > 0) {
      let doorsDateTime = zonedDateTime.set(DateTime.fromFormat(doorsTime, 'hh:mm').toObject());
      // weird timezone issues can cause the LPU time to be on the next day. That's why we need to fix the date just to be sure
      doorsDateTime = doorsDateTime.set({day: localDateTime.day, month: localDateTime.month, year: localDateTime.year});
      newConcert.doorsTime = doorsDateTime.toISO()!;
    }

    // LP stage time
    let lpStageTime = this.concertForm.value.lpStageTime?.valueOf();
    if (lpStageTime != null && lpStageTime.length > 0) {
      let lpStageDateTime = zonedDateTime.set(DateTime.fromFormat(lpStageTime, 'hh:mm').toObject());
      // weird timezone issues can cause the LPU time to be on the next day. That's why we need to fix the date just to be sure
      lpStageDateTime = lpStageDateTime.set({day: localDateTime.day, month: localDateTime.month, year: localDateTime.year});
      newConcert.mainStageTime = lpStageDateTime.toISO()!;
    }

    // Expected set duration
    newConcert.expectedSetDuration = this.convertH2M(this.concertForm.value.expectedSetDuration?.valueOf() ?? "00:00");

    // Venue coordinates
    newConcert.venueLatitude = this.concertForm.value.venueLat ?? 0;
    newConcert.venueLongitude = this.concertForm.value.venueLong ?? 0;

    console.log(newConcert);

    return newConcert;
  }


  private convertH2M(timeInHour: string){
    let timeParts = timeInHour.split(":");
    return Number(timeParts[0]) * 60 + Number(timeParts[1]);
  }


  private convertMinutesToString(minutes: number | undefined){
    if (minutes != undefined) {
      let setDurationMinutes = minutes % 60;
      let setDurationHours = (minutes - setDurationMinutes) / 60;
      return (setDurationHours < 10 ? "0" : "") + setDurationHours.toString() + ":" + (setDurationMinutes < 10 ? "0" : "") + setDurationMinutes.toString();
    }

    return undefined;
  }


  onScheduleFileSelected(event: any){
    this.selectedScheduleFile = <File> event.target.files[0];
  }


  uploadFileClicked() {
    if (this.selectedScheduleFile == undefined) {
      this.toastrService.error("No file was selected.", "File upload failed!");
      return;
    }

    this.scheduleIsUploading$ = true;
    this.concertsService.uploadConcertSchedule(this.concertId!, this.selectedScheduleFile)
        .subscribe(() => {
          this.scheduleIsUploading$ = false;
          this.toastrService.success("Schedule successfully uploaded!");
        })
  }


  /**
   * Sets the field expectedSetDuration based on minutes
   * @param minutes
   */
  setExpectedSetDuration(minutes: number) {
    let str = this.convertMinutesToString(minutes);
    this.concertForm.controls.expectedSetDuration.setValue(str ?? null);
  }


  protected readonly timezones = timezones;
  protected readonly listOfShowTypes = listOfShowTypes;
  protected readonly listOfTours = listOfTours;
  protected readonly environment = environment;
}
