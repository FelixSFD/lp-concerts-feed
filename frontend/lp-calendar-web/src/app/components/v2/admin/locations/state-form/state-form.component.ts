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

@Component({
  selector: 'app-state-form',
  imports: [
    Button,
    Card,
    Divider,
    FloatLabel,
    InputText,
    NgTemplateOutlet,
    ReactiveFormsModule
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
    code: new FormControl<string>('', [Validators.required]),
    name: new FormControl<string>('', [Validators.required]),
    nativeName: new FormControl<string>('', [Validators.required]),
  });

  onSaveClicked() {
    const content = this.readFromForm();
    if (content) {
      this.saveClicked.emit(content);
    }
  }

  public readFromForm(): StateFormContent | null {
    const code = this.stateForm.controls.code.value;
    const name = this.stateForm.value.name?.valueOf();
    const nativeName = this.stateForm.value.nativeName?.valueOf();

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

    if (nativeName == undefined || nativeName.length === 0) {
      this.messageService.add({
        severity: "error",
        summary: "Native name is required",
      });
      return null;
    }

    return {
      code,
      name,
      nativeName
    };
  }

  public fillFormWith(state: StateDto) {
    console.debug("Fill form with data:", state);
    this.stateForm.controls.code.disable();
    this.stateForm.controls.code.setValue(state.code ?? null);
    this.stateForm.controls.name.setValue(state.name ?? null);
    this.stateForm.controls.nativeName.setValue(state.nativeName ?? null);
  }
}

export class StateFormContent {
  code!: string;
  name!: string;
  nativeName!: string;
}
