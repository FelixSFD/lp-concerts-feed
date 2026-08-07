import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { MessageService } from 'primeng/api';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { CountryDto } from '../../../../../modules/lpshows-api/v3';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Divider } from 'primeng/divider';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { NgTemplateOutlet } from '@angular/common';
import { InputMaskDirective } from 'primeng/inputmask';

@Component({
  selector: 'app-country-form',
  imports: [
    Button,
    Card,
    Divider,
    FloatLabel,
    InputText,
    NgTemplateOutlet,
    ReactiveFormsModule,
    InputMaskDirective
  ],
  templateUrl: './country-form.component.html',
  styleUrl: './country-form.component.css',
})
export class CountryFormComponent {
  private messageService = inject(MessageService);
  private formBuilder = inject(FormBuilder);

  @Input("is-saving")
  isSaving$: boolean = false;

  /*
   * true, if the for is "standalone", meaning it manages its own layout and has a save-button
   */
  @Input("standalone")
  standalone$: boolean = true;

  @Output("saveClicked")
  saveClicked = new EventEmitter<CountryFormContent>();

  countryForm = this.formBuilder.group({
    isoCode: new FormControl<string>('', [Validators.required, Validators.pattern('^[A-Za-z]{3}$')]),
    name: new FormControl<string>('', [Validators.required]),
    nativeName: new FormControl<string>('', [Validators.required]),
  });

  onSaveClicked() {
    let content = this.readFromForm();
    if (content) {
      this.saveClicked.emit(content!);
    }
  }


  public readFromForm(): CountryFormContent | null {
    let isoCode = this.countryForm.value.isoCode?.valueOf();
    let name = this.countryForm.value.name?.valueOf();
    let nativeName = this.countryForm.value.nativeName?.valueOf();

    if (isoCode == undefined) {
      this.messageService.add({
        severity: "danger",
        summary: "ISO-code is required",
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
      isoCode: isoCode,
      name: name,
      nativeName: name
    };
  }


  public fillFormWith(country: CountryDto) {
    console.debug("Fill form with data:", country);
    this.countryForm.controls.isoCode.setValue(country.isoCode ?? null);
    this.countryForm.controls.name.setValue(country.name ?? null);
    this.countryForm.controls.nativeName.setValue(country.nativeName ?? null);
  }
}


export class CountryFormContent {
  isoCode!: string;
  name!: string;
  nativeName!: string;
}
