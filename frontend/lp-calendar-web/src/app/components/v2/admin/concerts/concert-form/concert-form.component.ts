import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { MessageService } from 'primeng/api';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConcertDetailsDto } from '../../../../../modules/lpshows-api/v3';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Divider } from 'primeng/divider';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { NgTemplateOutlet } from '@angular/common';
import { SelectConcertTypeComponent } from '../select-concert-type/select-concert-type.component';

@Component({
  selector: 'app-concert-form',
  imports: [
    Button,
    Card,
    Divider,
    FloatLabel,
    InputText,
    NgTemplateOutlet,
    ReactiveFormsModule,
    SelectConcertTypeComponent,
  ],
  templateUrl: './concert-form.component.html',
  styleUrl: './concert-form.component.css',
})
export class ConcertFormComponent {
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
  saveClicked = new EventEmitter<ConcertFormContent>();

  concertForm = this.formBuilder.group({
    customTitle: new FormControl<string>(''),
    concertTypeId: new FormControl<number | null>(null, [Validators.required]),
  });

  onSaveClicked() {
    let content = this.readFromForm();
    if (content) {
      this.saveClicked.emit(content);
    }
  }

  public readFromForm(): ConcertFormContent | null {
    let customTitle = this.concertForm.controls.customTitle.value?.valueOf()?.trim();
    let concertTypeId = this.concertForm.controls.concertTypeId.value;

    if (concertTypeId == null) {
      this.messageService.add({
        severity: 'error',
        summary: 'Concert type is required',
      });
      return null;
    }

    return {
      customTitle: customTitle,
      concertTypeId: concertTypeId,
    };
  }

  public fillFormWith(concert: ConcertDetailsDto) {
    this.concertForm.controls.customTitle.setValue(concert.customTitle ?? null);
    this.concertForm.controls.concertTypeId.setValue(concert.concertType?.id ?? null);
  }

  public reset() {
    this.concertForm.reset({
      customTitle: '',
      concertTypeId: null,
    });
  }
}

export class ConcertFormContent {
  customTitle?: string | null;
  concertTypeId?: number | null;
}
