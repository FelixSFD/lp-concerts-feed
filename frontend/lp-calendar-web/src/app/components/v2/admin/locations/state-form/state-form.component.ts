import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { MessageService } from 'primeng/api';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { CountryDto, StateDto } from '../../../../../modules/lpshows-api/v3';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Divider } from 'primeng/divider';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { NgTemplateOutlet } from '@angular/common';
import { Select } from 'primeng/select';

@Component({
  selector: 'app-state-form',
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
  templateUrl: './state-form.component.html',
  styleUrl: './state-form.component.css',
})
export class StateFormComponent {
  private messageService = inject(MessageService);
  private formBuilder = inject(FormBuilder);

  @Input("is-saving")
  isSaving$: boolean = false;

  @Input("countries")
  countries$: CountryDto[] = [];

  /*
   * true, if the form is "standalone", meaning it manages its own layout and has a save-button
   */
  @Input("standalone")
  standalone$: boolean = true;

  @Output("saveClicked")
  saveClicked = new EventEmitter<StateFormContent>();

  stateForm = this.formBuilder.group({
    countryCode: new FormControl<string>('', [Validators.required]),
    code: new FormControl<string>('', [Validators.required]),
    name: new FormControl<string>('', [Validators.required]),
  });

  onSaveClicked() {
    const content = this.readFromForm();
    if (content) {
      this.saveClicked.emit(content);
    }
  }

  public readFromForm(): StateFormContent | null {
    const countryCode = this.stateForm.value.countryCode?.valueOf();
    const code = this.stateForm.value.code?.valueOf();
    const name = this.stateForm.value.name?.valueOf();

    if (countryCode == undefined || countryCode.length === 0) {
      this.messageService.add({
        severity: "error",
        summary: "Country is required",
      });
      return null;
    }

    if (code == undefined || code.length === 0) {
      this.messageService.add({
        severity: "error",
        summary: "State code is required",
      });
      return null;
    }

    if (name == undefined || name.length === 0) {
      this.messageService.add({
        severity: "error",
        summary: "Name is required",
      });
      return null;
    }

    return {
      countryCode,
      code,
      name
    };
  }

  public fillFormWith(state: StateDto) {
    console.debug("Fill form with data:", state);

    this.stateForm.controls.countryCode.setValue(state.countryCode ?? null);
    this.stateForm.controls.code.setValue(state.code ?? null);
    this.stateForm.controls.name.setValue(state.name ?? null);
  }

  public disableIdentityFields() {
    this.stateForm.controls.countryCode.disable();
    this.stateForm.controls.code.disable();
  }
}

export class StateFormContent {
  countryCode!: string;
  code!: string;
  name!: string;
}
