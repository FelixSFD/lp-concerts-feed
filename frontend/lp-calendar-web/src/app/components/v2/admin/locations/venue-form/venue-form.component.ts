import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  EventEmitter,
  inject,
  Input,
  Output,
  signal,
  ViewChild
} from '@angular/core';
import { MessageService } from 'primeng/api';
import { FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  AddVenueNameRequestDto,
  CityWithCountryDto,
  CountryDto,
  PreviousVenueNameDto,
  UpdateVenueNameRequestDto,
  VenueDto,
  VenueWithDetailsDto
} from '../../../../../modules/lpshows-api/v3';
import { LocationsService } from '../../../../../services/locations.service';
import { Button } from 'primeng/button';
import { ButtonGroup } from 'primeng/buttongroup';
import { Card } from 'primeng/card';
import { DatePicker } from 'primeng/datepicker';
import { Dialog } from 'primeng/dialog';
import { Divider } from 'primeng/divider';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { NgTemplateOutlet } from '@angular/common';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import timezones from 'timezones-list';
import { InputNumber } from 'primeng/inputnumber';
import { SelectTimezoneComponent } from '../select-timezone/select-timezone.component';
import { DateTime } from 'luxon';
import {
  load,
  Map as AppleMap, MapAnnotationDragEvent,
  MapKit,
  MarkerAnnotation
} from '@apple/mapkit-loader';
import { environment } from '../../../../../../environments/environment';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-venue-form',
  imports: [
    Button,
    ButtonGroup,
    Card,
    DatePicker,
    Dialog,
    Divider,
    FloatLabel,
    FormsModule,
    InputText,
    NgTemplateOutlet,
    ReactiveFormsModule,
    Select,
    InputNumber,
    SelectTimezoneComponent,
    TableModule,
  ],
  templateUrl: './venue-form.component.html',
  styleUrl: './venue-form.component.css',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class VenueFormComponent {
  private messageService = inject(MessageService);
  private formBuilder = inject(FormBuilder);
  private locationsService = inject(LocationsService);

  // Apple Maps
  private mapKit: MapKit | undefined;
  private appleMap: AppleMap | undefined;
  private locationMarker: MarkerAnnotation | null = null;

  private currentVenueId: number | null = null;

  @Input("is-saving")
  isSaving$: boolean = false;

  @Input("available-countries")
  countries$ = signal<CountryDto[]>([]);

  citiesInCountry$ = signal<CityWithCountryDto[]>([]);

  timeZoneIsLoading$ = signal<boolean>(false);

  /*
   * true, if the form is "standalone", meaning it manages its own layout and has a save-button
   */
  @Input("standalone")
  standalone$: boolean = true;

  @Output("saveClicked")
  saveClicked = new EventEmitter<VenueFormContent>();

  historicNames$ = signal<PreviousVenueNameDto[]>([]);
  isShowingHistoricNameDialog$ = signal<boolean>(false);
  isEditingHistoricName$ = signal<boolean>(false);
  editingHistoricNameIndex$ = signal<number | null>(null);

  historicNameForm = this.formBuilder.group({
    name: new FormControl<string>('', [Validators.required]),
    usedFrom: new FormControl<Date | string | null>(null, [Validators.required]),
    usedUntil: new FormControl<Date | string | null>(null, []),
  });

  venueForm = this.formBuilder.group({
    countryCode: new FormControl<string>('', [Validators.required]),
    cityId: new FormControl<number>(0, [Validators.required]),
    currentName: new FormControl<string>('', [Validators.required]),
    timezone: new FormControl('', [Validators.required]),
    latitude: new FormControl<number>(0, []),
    longitude: new FormControl<number>(0, []),
  });

  constructor() {
    this.venueForm.controls.countryCode.valueChanges.subscribe((countryCode) => {
      if (countryCode == null) {
        this.citiesInCountry$.set([]);
        console.debug("Country code is null, clearing cities in country");
        return;
      }

      this.locationsService.getCitiesIn(countryCode)
        .subscribe({
          next: (cities) => {
            this.citiesInCountry$.set(cities);
            console.debug("Cities in selected country:", cities);
          },
          error: (error) => {
            console.error(error);
          }
        });
    });
  }

  private async initAppleMaps() {
    this.mapKit = await load({
      token: environment.appleMapsToken,
      language: "en-US",
      libraries: ["map", "annotations"],
    });
  }

  private addOrMoveMarker(lon: number, lat: number) {
    if (!this.appleMap || !this.mapKit) {
      return;
    }

    if (this.locationMarker) {
      console.debug("Pin already exists. Will just move it.");
      this.locationMarker.coordinate = new this.mapKit!.Coordinate(lat, lon);
      this.venueForm.controls.latitude.setValue(lat);
      this.venueForm.controls.longitude.setValue(lon);
    } else {
      console.debug("Creating new pin on the map...");
      this.locationMarker = new this.mapKit!.MarkerAnnotation(new this.mapKit!.Coordinate(lat, lon), {
        color: "#c969e0",
        map: this.appleMap,
        draggable: true
      });
      this.appleMap.addEventListener("dragging", this.didDragPin);
      console.debug("Pin created.", this.locationMarker);
      this.appleMap?.showItems([this.locationMarker]);
    }

    this.zoomToCoordinates(lon, lat);
  }


  private didDragPin = (evt: Event) => {
    let dragEvent = evt as MapAnnotationDragEvent;
    this.venueForm.controls.latitude.setValue(dragEvent.coordinate?.latitude ?? null);
    this.venueForm.controls.longitude.setValue(dragEvent.coordinate?.longitude ?? null);
  };


  private zoomToCoordinates(lon: number, lat: number, zoomLevel: number = 11) {
    if (this.appleMap && this.mapKit) {
      this.appleMap.region = new this.mapKit.CoordinateRegion(
        new this.mapKit.Coordinate(lat, lon),
        new this.mapKit.CoordinateSpan(0.06, 0.2)
      );
    }
  }

  onSaveClicked() {
    const content = this.readFromForm();
    if (content) {
      this.saveClicked.emit(content);
    }
  }

  public readFromForm(): VenueFormContent | null {
    const countryCode = this.venueForm.controls.countryCode.value;
    const cityId = this.venueForm.value.cityId?.valueOf();
    const currentName = this.venueForm.value.currentName?.valueOf();
    const timezone = this.venueForm.value.timezone?.valueOf();

    if (countryCode == undefined || countryCode.length === 0) {
      this.messageService.add({
        severity: "error",
        summary: "Country is required",
      });
      return null;
    }

    if (cityId == undefined || cityId === 0) {
      this.messageService.add({
        severity: "error",
        summary: "City is required",
      });
      return null;
    }

    if (currentName == undefined || currentName.length === 0) {
      this.messageService.add({
        severity: "error",
        summary: "Name is required",
      });
      return null;
    }

    if (timezone == undefined || timezone.length === 0) {
      this.messageService.add({
        severity: "error",
        summary: "Timezone is required",
      });
      return null;
    }

    return {
      countryCode: countryCode,
      stateCode: null,
      cityId: cityId,
      currentName: currentName,
      timeZoneId: timezone,
      latitude: this.venueForm.value.latitude ?? null,
      longitude: this.venueForm.value.longitude ?? null,
      historicNames: this.historicNames$()
    };
  }

  public fillFormWith(venue: VenueWithDetailsDto) {
    console.debug("Fill form with data:", venue);
    this.currentVenueId = venue.id ?? null;

    this.venueForm.controls.cityId.setValue(Number(venue.cityId));
    this.venueForm.controls.countryCode.setValue(venue.countryCode ?? null);
    this.venueForm.controls.timezone.setValue(venue.timeZoneId ?? null);
    this.venueForm.controls.currentName.setValue(venue.currentName ?? null);
    this.venueForm.controls.latitude.setValue(venue.latitude ?? null);
    this.venueForm.controls.longitude.setValue(venue.longitude ?? null);

    this.historicNames$.set(venue.venueNames);
  }

  onAddHistoricNameClicked() {
    this.isEditingHistoricName$.set(false);
    this.editingHistoricNameIndex$.set(null);
    this.historicNameForm.reset();
    this.isShowingHistoricNameDialog$.set(true);
  }

  onEditHistoricNameClicked(nameItem: PreviousVenueNameDto, index: number) {
    this.isEditingHistoricName$.set(true);
    this.editingHistoricNameIndex$.set(index);

    let fromDate: Date | null = null;
    if (nameItem.usedFrom) {
      const dt = DateTime.fromISO(nameItem.usedFrom);
      fromDate = dt.isValid ? dt.toJSDate() : new Date(nameItem.usedFrom);
    }

    let untilDate: Date | null = null;
    if (nameItem.usedUntil) {
      const dt = DateTime.fromISO(nameItem.usedUntil);
      untilDate = dt.isValid ? dt.toJSDate() : new Date(nameItem.usedUntil);
    }

    this.historicNameForm.setValue({
      name: nameItem.name,
      usedFrom: fromDate,
      usedUntil: untilDate,
    });
    this.isShowingHistoricNameDialog$.set(true);
  }

  onDeleteHistoricNameClicked(nameId: number) {
    this.locationsService.deleteVenueName(this.currentVenueId!, nameId).subscribe({
      next: () => {
        this.messageService.add({
          severity: "success",
          summary: "Successfully deleted previous name"
        });
      },
      error: err => {
        console.error("Failed to delete name", err);
        this.messageService.add({
          severity: "error",
          summary: "Failed to delete previous name"
        });

        this.locationsService.getVenueDetails(this.currentVenueId!).subscribe({
          next: (updated) => {
            this.historicNames$.set(updated.venueNames);
          }
        })
      }
    })
  }

  onSaveHistoricNameDialog() {
    if (this.historicNameForm.invalid) {
      this.historicNameForm.markAllAsTouched();
      return;
    }

    const formValue = this.historicNameForm.value;
    let fromStr = '';
    if (formValue.usedFrom instanceof Date) {
      fromStr = DateTime.fromJSDate(formValue.usedFrom).toISODate() ?? '';
    } else if (typeof formValue.usedFrom === 'string') {
      fromStr = formValue.usedFrom;
    }

    let untilStr: string | undefined = undefined;
    if (formValue.usedUntil instanceof Date) {
      untilStr = DateTime.fromJSDate(formValue.usedUntil).toISODate() ?? undefined;
    } else if (typeof formValue.usedUntil === 'string' && formValue.usedUntil.trim() !== '') {
      untilStr = formValue.usedUntil.trim();
    }

    if (this.isEditingHistoricName$()) {
      const idx = this.editingHistoricNameIndex$()!;
      const updated = [...this.historicNames$()];
      const existing = updated[idx];
      updated[idx] = {
        ...existing,
        name: formValue.name ?? '',
        usedFrom: fromStr,
        usedUntil: untilStr,
      };
      let updateRequest: UpdateVenueNameRequestDto = {
        name: formValue.name ?? "",
        from: fromStr,
        to: untilStr
      };
      this.locationsService.updateVenueName(existing.venueId, existing.id, updateRequest)
        .subscribe({
          next: value => {
            this.historicNames$.set(updated);
          },
          error: err => {
            console.error(err);
          }
        });
    } else {
      const newNameRequest: AddVenueNameRequestDto = {
        name: formValue.name ?? "",
        from: fromStr,
        to: untilStr
      };
      this.locationsService.addNewVenueName(this.currentVenueId ?? 0, newNameRequest)
        .subscribe({
          next: updatedVenue => {
            console.debug("Added venue name", updatedVenue.venueNames);
            this.historicNames$.set(updatedVenue.venueNames);
          },
          error: err => {
            console.error(err);
          }
        });
    }

    this.isShowingHistoricNameDialog$.set(false);
  }

  onUpdateTimeZoneClicked() {
    this.timeZoneIsLoading$.set(true);

    let cityId = this.venueForm.value.cityId;
    let countryCode = this.venueForm.value.countryCode;

    if (cityId == null || countryCode == null) {
      this.timeZoneIsLoading$.set(false);
      return;
    }

    let cityName = this.citiesInCountry$().find(c => c.id == cityId)?.name;
    let countryName = this.countries$().find(c => c.isoCode == countryCode)?.name;

    this.locationsService.getTimeZoneForCity(cityName!, null, countryName!)
      .subscribe({
        next: (tzObj) => {
          if (tzObj == null) {
            console.warn("No timezone found for: City: ", cityName, " Country: ", countryName);
            this.timeZoneIsLoading$.set(false);
            return;
          }

          console.log("Found timezone: ", tzObj);
          let tz = tzObj.timeZoneId!;
          this.timeZoneIsLoading$.set(false);

          if (timezones.map(t => t.tzCode).indexOf(tz, 0) >= 0) {
            this.venueForm.controls.timezone.setValue(tz);
          } else {
            console.error("Invalid timezone returned: ", tz);
            this.messageService.add({
              severity: "error",
              summary: "Could not load timezone",
              text: `Timezone '${tz}' found, but it is invalid.`,
            });
          }
        },
        error: (err) => {
          console.error("Error loading timezone", err);
          this.timeZoneIsLoading$.set(false);
        }
      });
  }

  @ViewChild('appleMaps')
  set appleMaps(mapElement: ElementRef<HTMLDivElement> | undefined) {
    console.log('appleMaps will be displayed', mapElement);
    if (!mapElement) return;
    if (!this.appleMaps) {
      console.debug('MapKit not initialized yet!');
      this.initAppleMaps().then(() => {
        this.appleMap = this.makeMap(mapElement.nativeElement);
        this.addOrMoveMarker(this.venueForm.controls.longitude.value ?? 0, this.venueForm.controls.latitude.value ?? 0);
      });
      return;
    }

    console.log("Will set map element: ", mapElement);
    this.appleMap = this.makeMap(mapElement.nativeElement);
    this.addOrMoveMarker(this.venueForm.controls.longitude.value ?? 0, this.venueForm.controls.latitude.value ?? 0);
  }

  private makeMap(mapElement: HTMLDivElement) {
    let map = new this.mapKit!.Map(mapElement);
    map.colorScheme = "adaptive";
    return map;
  }

  private async getCityDetails(cityId: number) {
    let city = await firstValueFrom(this.locationsService.getCity(this.venueForm.value.countryCode!, cityId));
    if (city == null) {
      return null;
    }

    return city;
  }

  async onGoToCityClicked() {
    let cityId = this.venueForm.value.cityId;
    if (cityId == null) {
      this.messageService.add({ severity: 'warn', summary: 'No city selected', detail: 'Please select a city first' });
      return;
    }

    let city = await this.getCityDetails(cityId);
    if (city == null) {
      this.messageService.add({ severity: 'error', summary: 'City not found', detail: 'The list of cities was not loaded correctly' });
      return;
    }

    console.log("Will zoom to city: ", city);

    let state = city.state ?? null;
    let country = city.country;

    this.locationsService.getCoordinatesFor(city.name, state ? state.name : null, country.name)
      .subscribe(coordinates => {
        this.zoomToCoordinates(coordinates?.longitude ?? 0, coordinates?.latitude ?? 0);
      });
  }


  onSetPinClicked() {
    let center = this.appleMap?.center;
    this.addOrMoveMarker(center?.longitude ?? 0, center?.latitude ?? 0);
  }


  async tryAutoSetVenuePin() {
    let venueName = this.venueForm.value.currentName;

    let cityId = this.venueForm.value.cityId;
    if (cityId == null) {
      this.messageService.add({ severity: 'warn', summary: 'No city selected', detail: 'Please select a city first' });
      return;
    }

    let city = await this.getCityDetails(cityId);
    if (city == null) {
      this.messageService.add({ severity: 'error', summary: 'City not found', detail: 'The list of cities was not loaded correctly' });
      return;
    }

    let state = city.state ?? null;
    let country = city.country;

    if (country == null || venueName == null) {
      return;
    }

    this.locationsService.getCoordinatesForVenue(venueName, city.name, state ? state.name : null, country.name)
      .subscribe(coordinates => {
        let lat = coordinates?.latitude ?? 0;
        let lon = coordinates?.longitude ?? 0;

        this.addOrMoveMarker(lon, lat);
        this.venueForm.controls.longitude.setValue(lon);
        this.venueForm.controls.latitude.setValue(lat);
      });
  }
}


export class VenueFormContent {
  countryCode!: string;
  stateCode: string | null = null;
  cityId!: number;
  currentName!: string;
  timeZoneId!: string;
  latitude: number | null = null;
  longitude: number | null = null;
  historicNames: PreviousVenueNameDto[] = [];
}
