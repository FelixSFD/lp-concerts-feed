import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { MessageService } from 'primeng/api';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { TourDto } from '../../../../../modules/lpshows-api/v3';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Divider } from 'primeng/divider';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { NgTemplateOutlet } from '@angular/common';

@Component({
  selector: 'app-tour-form',
  imports: [
    Button,
    Card,
    Divider,
    FloatLabel,
    InputText,
    NgTemplateOutlet,
    ReactiveFormsModule
  ],
  templateUrl: './tour-form.component.html',
  styleUrl: './tour-form.component.css',
})
export class TourFormComponent {
  private messageService = inject(MessageService);
  private formBuilder = inject(FormBuilder);

  @Input("is-saving")
  isSaving$: boolean = false;

  /**
   * true, if the form is "standalone", meaning it manages its own layout and has a save-button
   */
  @Input("standalone")
  standalone$: boolean = true;

  @Input("is-edit")
  isEdit$: boolean = false;

  @Output("saveClicked")
  saveClicked = new EventEmitter<TourFormContent>();

  tourForm = this.formBuilder.group({
    id: new FormControl<string>('', [Validators.required]),
    name: new FormControl<string>('', [Validators.required]),
  });

  onSaveClicked() {
    let content = this.readFromForm();
    if (content) {
      this.saveClicked.emit(content);
    }
  }

  public readFromForm(): TourFormContent | null {
    let id = this.tourForm.controls.id.value?.valueOf()?.trim();
    let name = this.tourForm.controls.name.value?.valueOf()?.trim();

    if (!id) {
      this.messageService.add({
        severity: "error",
        summary: "Tour ID is required",
      });
      return null;
    }

    if (!name) {
      this.messageService.add({
        severity: "error",
        summary: "Tour name is required",
      });
      return null;
    }

    return {
      id: id,
      name: name
    };
  }

  public fillFormWith(tour: TourDto) {
    this.tourForm.controls.id.setValue(tour.id ?? null);
    this.tourForm.controls.name.setValue(tour.name ?? null);
  }
}

export class TourFormContent {
  id!: string;
  name!: string;
}
