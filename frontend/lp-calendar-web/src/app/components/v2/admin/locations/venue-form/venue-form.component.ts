import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { MessageService } from 'primeng/api';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { CityWithCountryDto, CountryDto, VenueDto } from '../../../../../modules/lpshows-api/v3';
import { LocationsService } from '../../../../../services/locations.service';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Divider } from 'primeng/divider';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { NgTemplateOutlet } from '@angular/common';
import { Select } from 'primeng/select';
import { InputGroup } from 'primeng/inputgroup';
import { InputGroupAddon } from 'primeng/inputgroupaddon';
import timezones from 'timezones-list';

@Component({
  selector: 'app-venue-form',
  imports: [
    Button,
    Card,
    Divider,
    FloatLabel,
    InputText,
    NgTemplateOutlet,
    ReactiveFormsModule,
    Select,
    InputGroup,
    InputGroupAddon
  ],
  templateUrl: './venue-form.component.html',
  styleUrl: './venue-form.component.css',
})
export class VenueFormComponent {
  private messageService = inject(MessageService);
  private formBuilder = inject(FormBuilder);
  private locationsService = inject(LocationsService);

  @Input("is-saving")
  isSaving$: boolean = false;

  @Input("available-countries")
  countries$: CountryDto[] = [];

  citiesInCountry$: CityWithCountryDto[] = [];

  timeZoneIsLoading$ = false;

  /*
   * true, if the form is "standalone", meaning it manages its own layout and has a save-button
   */
  @Input("standalone")
  standalone$: boolean = true;

  @Output("saveClicked")
  saveClicked = new EventEmitter<VenueFormContent>();

  venueForm = this.formBuilder.group({
    countryCode: new FormControl<string>('', [Validators.required]),
    cityId: new FormControl<number>(0, [Validators.required]),
    currentName: new FormControl<string>('', [Validators.required]),
    timezone: new FormControl('', [Validators.required]),
  });

  constructor() {
    this.venueForm.controls.countryCode.valueChanges.subscribe((countryCode) => {
      if (countryCode == null) {
        this.citiesInCountry$ = [];
        console.debug("Country code is null, clearing cities in country");
        return;
      }

      this.locationsService.getCitiesIn(countryCode)
        .subscribe({
          next: (cities) => {
            this.citiesInCountry$ = cities;
            console.debug("Cities in selected country:", cities);
          },
          error: (error) => {
            console.error(error);
          }
        });
    });
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
      timeZone: timezone,
    };
  }

  public fillFormWith(venue: VenueDto) {
    console.debug("Fill form with data:", venue);
    this.venueForm.controls.cityId.setValue(Number(venue.cityId));
    this.venueForm.controls.countryCode.setValue(venue.countryCode ?? null);
    this.venueForm.controls.timezone.setValue(venue.timeZone ?? null);
    this.venueForm.controls.currentName.setValue(venue.currentName ?? null);
  }


  onUpdateTimeZoneClicked() {
    this.timeZoneIsLoading$ = true;

    let cityId = this.venueForm.value.cityId;
    let countryCode = this.venueForm.value.countryCode;

    if (cityId == null || countryCode == null) {
      return;
    }

    let cityName = this.citiesInCountry$.find(c => c.id == cityId.toString())?.name;
    let countryName = this.countries$.find(c => c.isoCode == countryCode)?.name;

    this.locationsService.getCoordinatesFor(cityName!, null, countryName!)
      .subscribe(coordinates => {
        console.log("Found coordinates: ", coordinates);
        this.locationsService.getTimeZoneForCoordinates(coordinates?.latitude ?? 0, coordinates?.longitude ?? 0)
          .subscribe(tzObj => {
            console.log("Found timezone: ", tzObj);
            let tz = tzObj.timeZoneId!;
            this.timeZoneIsLoading$ = false;

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
          });
      });
  }


  protected readonly timezones = timezones;
}


export class VenueFormContent {
  countryCode!: string;
  stateCode: string | null = null;
  cityId!: number;
  currentName!: string;
  timeZone!: string;
}
