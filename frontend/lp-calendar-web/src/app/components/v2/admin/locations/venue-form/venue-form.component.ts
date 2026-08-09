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
    Select
  ],
  templateUrl: './venue-form.component.html',
  styleUrl: './venue-form.component.css',
})
export class VenueFormComponent implements OnInit {
  private messageService = inject(MessageService);
  private formBuilder = inject(FormBuilder);
  private locationsService = inject(LocationsService);

  @Input("is-saving")
  isSaving$: boolean = false;

  @Input("available-countries")
  countries$: CountryDto[] = [];

  citiesInCountry$: CityWithCountryDto[] = [];

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
  });

  ngOnInit() {
    this.venueForm.controls.countryCode.valueChanges.subscribe((countryCode) => {
      if (countryCode == null) {
        this.citiesInCountry$ = [];
        return;
      }

      this.locationsService.getCitiesIn(countryCode)
        .subscribe({
          next: (cities) => {
            this.citiesInCountry$ = cities;
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

    return {
      countryCode: countryCode,
      stateCode: null,
      cityId: cityId,
      currentName: currentName,
    };
  }

  public fillFormWith(venue: VenueDto) {
    console.debug("Fill form with data:", venue);
    this.venueForm.controls.currentName.setValue(venue.currentName ?? null);
  }
}


export class VenueFormContent {
  countryCode!: string;
  stateCode: string | null = null;
  cityId!: number;
  currentName!: string;
}
