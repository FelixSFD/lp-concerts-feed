import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { MessageService } from 'primeng/api';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { TourLegDto } from '../../../../../modules/lpshows-api/v3';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Divider } from 'primeng/divider';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { NgTemplateOutlet } from '@angular/common';

@Component({
  selector: 'app-tour-leg-form',
  imports: [
    Button,
    Card,
    Divider,
    FloatLabel,
    InputText,
    NgTemplateOutlet,
    ReactiveFormsModule
  ],
  templateUrl: './tour-leg-form.component.html',
  styleUrl: './tour-leg-form.component.css',
})
export class TourLegFormComponent {
  private messageService = inject(MessageService);
  private formBuilder = inject(FormBuilder);

  @Input("is-saving")
  isSaving$: boolean = false;

  /**
   * true, if the form is "standalone", meaning it manages its own layout and has a save-button
   */
  @Input("standalone")
  standalone$: boolean = true;

  @Output("saveClicked")
  saveClicked = new EventEmitter<TourLegFormContent>();

  legForm = this.formBuilder.group({
    id: new FormControl<string>('', [Validators.required]),
    name: new FormControl<string>('', [Validators.required]),
  });

  onSaveClicked() {
    let content = this.readFromForm();
    if (content) {
      this.saveClicked.emit(content);
    }
  }

  public readFromForm(): TourLegFormContent | null {
    let id = this.legForm.controls.id.value?.valueOf()?.trim();
    let name = this.legForm.controls.name.value?.valueOf()?.trim();

    if (!id) {
      this.messageService.add({
        severity: "error",
        summary: "Leg ID is required",
      });
      return null;
    }

    if (!name) {
      this.messageService.add({
        severity: "error",
        summary: "Leg name is required",
      });
      return null;
    }

    return {
      id: id,
      name: name
    };
  }

  public fillFormWith(leg: TourLegDto) {
    this.legForm.controls.id.setValue(leg.id ?? null);
    this.legForm.controls.name.setValue(leg.name ?? null);
  }

  public reset() {
    this.legForm.reset({
      id: '',
      name: ''
    });
  }
}

export class TourLegFormContent {
  id!: string;
  name!: string;
}
