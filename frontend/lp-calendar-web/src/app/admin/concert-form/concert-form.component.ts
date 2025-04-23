import {
  AfterViewInit,
  Component, ElementRef,
  EventEmitter,
  inject,
  Input,
  OnInit,
  Output,
  ViewChild
} from '@angular/core';
import timezones from 'timezones-list';
import {listOfTours, listOfShowTypes} from '../../app.config';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {NgClass, NgForOf, NgIf} from '@angular/common';
import {Concert} from '../../data/concert';
import {ConcertsService} from '../../services/concerts.service';
import {DateTime} from 'luxon';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import Map from 'ol/Map';
import View from 'ol/View';
import TileLayer from 'ol/layer/Tile';
import OSM from 'ol/source/OSM';
import {fromLonLat, toLonLat} from 'ol/proj';
import Feature from 'ol/Feature';
import Point from 'ol/geom/Point';
import VectorLayer from 'ol/layer/Vector';
import VectorSource from 'ol/source/Vector';
import Style from 'ol/style/Style';
import Icon from 'ol/style/Icon';
import { Translate } from 'ol/interaction';
import {Collection} from "ol";
import {Observable} from "rxjs";
import {environment} from "../../../environments/environment";
import {ToastrService} from 'ngx-toastr';

// This class represents a form for adding and editing concerts
@Component({
  selector: 'app-concert-form',
  imports: [
    FormsModule,
    NgForOf,
    NgIf,
    ReactiveFormsModule,
    NgClass
  ],
  templateUrl: './concert-form.component.html',
  styleUrl: './concert-form.component.css'
})
export class ConcertFormComponent implements OnInit, AfterViewInit {
  private formBuilder = inject(FormBuilder);
  private concertsService = inject(ConcertsService);
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
  saveClicked = new EventEmitter<Concert>();

  // Name of the form-tab that is open at the moment
  activeTabName$: string = "main";

  concert$ : Concert | null = null;

  // Map of the location of the concert
  private venueMap: Map | undefined;
  private marker: Feature | undefined;

  // Selected schedule-file
  protected selectedScheduleFile: File | undefined;

  // Service to check auth information
  private readonly oidcSecurityService = inject(OidcSecurityService);

  hasWriteAccess$ = false;
  scheduleIsUploading$ = false;

  constructor() {
    this.oidcSecurityService.checkAuth().subscribe(({ isAuthenticated }) => {
      this.hasWriteAccess$ = isAuthenticated;
    });
  }


  ngOnInit() {
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


  ngAfterViewInit() {
    this.initVenueMap();
  }


  private initVenueMap() {
    this.venueMap = new Map({
      layers: [
        new TileLayer({
          source: new OSM(),
        }),
      ],
      view: new View({
        center: [0, 0],
        zoom: 2, maxZoom: 18,
      }),
    });
  }


  private addOrMoveMarker(lon: number, lat: number) {
    const newCoords = fromLonLat([lon, lat]); // Convert to EPSG:3857
    console.log("Set marker at: " + newCoords.toString())

    if (this.marker == undefined) {
      // Create a marker feature
      this.marker = new Feature({
        geometry: new Point(newCoords), // Initial position
      });

      this.marker.setStyle(new Style({
        image: new Icon({
          anchor: [0.5, 1],
          src: './map/map-pin-50-black.png',
          scale: 0.59
        })
      }));

      // Add marker to vector layer
      const vectorSource = new VectorSource({
        features: [this.marker]
      });

      const vectorLayer = new VectorLayer({
        source: vectorSource
      });

      this.venueMap?.addLayer(vectorLayer);

      // Add drag interaction
      const translate = new Translate({
        features: new Collection([this.marker])
      });

      this.venueMap?.addInteraction(translate);

      // Listen for movement
      translate.on('translateend', (event) => {
        const point = (event.features.item(0) as Feature).getGeometry() as Point;
        const coords = point.getCoordinates();
        console.log('Marker moved to:', coords);

        if (coords) {
          const [lon, lat] = toLonLat(coords); // Convert to lat/lon
          console.log('Corrected Coordinates:', { latitude: lat, longitude: lon });

          this.concertForm.controls.venueLong.setValue(lon);
          this.concertForm.controls.venueLat.setValue(lat);
        }
      });
    } else {
      (this.marker.getGeometry() as Point).setCoordinates(newCoords);
    }

    this.zoomToCoordinates(lon, lat);
  }


  private zoomToCoordinates(lon: number, lat: number) {
    this.venueMap?.getView().setCenter(fromLonLat([lon, lat]));
    this.venueMap?.getView().setZoom(11);
  }


  private fillFormWithConcert(concert: Concert) {
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

    let setDurationStr;
    if (concert.expectedSetDuration != undefined) {
      let setDurationMinutes = concert.expectedSetDuration % 60;
      let setDurationHours = (concert.expectedSetDuration - setDurationMinutes) % 60;
      setDurationStr = (setDurationHours < 10 ? "0" : "") + setDurationHours.toString() + ":" + (setDurationMinutes < 10 ? "0" : "") + setDurationMinutes.toString();
    }

    this.concertForm.controls.showType.setValue(concert.showType ?? null);
    this.concertForm.controls.tourName.setValue(concert.tourName ?? null);
    this.concertForm.controls.customTitle.setValue(concert.customTitle ?? null);
    this.concertForm.controls.venue.setValue(concert.venue ?? null);
    this.concertForm.controls.city.setValue(concert.city ?? null);
    this.concertForm.controls.state.setValue(concert.state ?? null);
    this.concertForm.controls.country.setValue(concert.country ?? null);
    this.concertForm.controls.postedStartTime.setValue(postedStartDateTimeIsoStr?.substring(0, postedStartDateTimeIsoStr?.length - 6) ?? null);
    this.concertForm.controls.timezone.setValue(concert.timeZoneId ?? null);
    this.concertForm.controls.lpuEarlyEntryConfirmed.setValue(concert.lpuEarlyEntryConfirmed);
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


  @ViewChild('tabMap')
  set tabMapRendered(element: ElementRef | undefined) {
    // is called when tab rendered or destroyed
    if (element) {
      if (this.venueMap) {
        console.log("Set target");
        this.venueMap.setTarget("venueMap");
      }
    } else {
      console.log("Map tab destroyed");
      this.venueMap?.setTarget("");
    }
  }


  openTab(tabName: string) {
    this.activeTabName$ = tabName;
  }


  private readConcertFromForm() {
    const postedStartTime = this.concertForm.value.postedStartTime!;
    let newConcert = new Concert();
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


  protected readonly timezones = timezones;
  protected readonly listOfShowTypes = listOfShowTypes;
  protected readonly listOfTours = listOfTours;
  protected readonly environment = environment;
}
