import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { MessageService } from 'primeng/api';
import { FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CityWithCountryDto, CountryDto } from '../../../../../modules/lpshows-api/v3';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Divider } from 'primeng/divider';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { NgTemplateOutlet } from '@angular/common';
import { Select } from 'primeng/select';

@Component({
  selector: 'app-city-form',
  imports: [
    Button,
    Card,
    Divider,
    FloatLabel,
    FormsModule,
    InputText,
    NgTemplateOutlet,
    ReactiveFormsModule,
    Select
  ],
  templateUrl: './city-form.component.html',
  styleUrl: './city-form.component.css',
})
export class CityFormComponent {
  private messageService = inject(MessageService);
  private formBuilder = inject(FormBuilder);

  @Input("is-saving")
  isSaving$: boolean = false;

  /*
   * true, if the form is "standalone", meaning it manages its own layout and has a save-button
   */
  @Input("standalone")
  standalone$: boolean = true;

  @Input("available-countries")
  availableCountries$: CountryDto[] = [];

  @Output("saveClicked")
  saveClicked = new EventEmitter<CityFormContent>();

  cityForm = this.formBuilder.group({
    countryCode: new FormControl<string>('', [Validators.required]),
    name: new FormControl<string>('', [Validators.required]),
    nativeName: new FormControl<string>('', [Validators.required]),
  });

  onSaveClicked() {
    let content = this.readFromForm();
    if (content) {
      this.saveClicked.emit(content!);
    }
  }


  public readFromForm(): CityFormContent | null {
    let countryCode = this.cityForm.value.countryCode?.valueOf();
    let name = this.cityForm.value.name?.valueOf();
    let nativeName = this.cityForm.value.nativeName?.valueOf();

    if (countryCode == undefined) {
      this.messageService.add({
        severity: "danger",
        summary: "Country is required",
      });
      return null;
    }

    if (name == undefined) {
      this.messageService.add({
        severity: "danger",
        summary: "Name is required",
      });
      return null;
    }

    if (nativeName == undefined) {
      this.messageService.add({
        severity: "danger",
        summary: "Native Name is required",
      });
      return null;
    }

    return {
      countryCode: countryCode,
      stateCode: null,
      name: name,
      nativeName: nativeName
    };
  }


  public fillFormWith(city: CityWithCountryDto) {
    console.debug("Fill form with data:", city);
    this.cityForm.controls.name.setValue(city.name ?? null);
    this.cityForm.controls.nativeName.setValue(city.nativeName ?? null);
  }
}

export class CityFormContent {
  countryCode!: string;
  stateCode: string | null = null;
  name!: string;
  nativeName!: string;
}
