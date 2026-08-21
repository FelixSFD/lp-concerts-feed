import { Component, EventEmitter, inject, Input, OnInit, Output, signal } from '@angular/core';
import { MessageService } from 'primeng/api';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConcertDetailsDto, TourDto } from '../../../../../modules/lpshows-api/v3';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Divider } from 'primeng/divider';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { NgTemplateOutlet } from '@angular/common';
import { SelectConcertTypeComponent } from '../select-concert-type/select-concert-type.component';
import { SelectTourComponent } from '../select-tour/select-tour.component';
import { SelectTourLegComponent } from '../select-tour-leg/select-tour-leg.component';
import { DatePicker } from 'primeng/datepicker';
import { InputGroup } from 'primeng/inputgroup';
import { InputGroupAddon } from 'primeng/inputgroupaddon';
import { Select } from 'primeng/select';
import timezones from 'timezones-list';
import { DateTime } from 'luxon';

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
    SelectTourComponent,
    SelectTourLegComponent,
    DatePicker,
    InputGroup,
    InputGroupAddon,
    Select,
  ],
  templateUrl: './concert-form.component.html',
  styleUrl: './concert-form.component.css',
})
export class ConcertFormComponent implements OnInit {
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

  selectedTour = signal<TourDto | null>(null);

  concertForm = this.formBuilder.group({
    customTitle: new FormControl<string>(''),
    concertTypeId: new FormControl<number | null>(null, [Validators.required]),
    tour: new FormControl<TourDto | null>(null, [Validators.required]),
    tourLegId: new FormControl<string | null>(null),
    postedStartTime: new FormControl<Date | null>(null, [Validators.required]),
    timezone: new FormControl('', [Validators.required]),
  });

  ngOnInit() {
    this.concertForm.controls.tour.valueChanges.subscribe((tour) => {
      console.debug('ConcertFormComponent tour changed', tour);
      this.selectedTour.set(tour);
    });
  }

  onSaveClicked() {
    let content = this.readFromForm();
    if (content) {
      this.saveClicked.emit(content);
    }
  }

  public readFromForm(): ConcertFormContent | null {
    let customTitle = this.concertForm.controls.customTitle.value?.valueOf()?.trim();
    let concertTypeId = this.concertForm.controls.concertTypeId.value;
    let tourId = this.concertForm.controls.tour.value?.id;
    let tourLegId = this.concertForm.controls.tourLegId.value;
    let timezone = this.concertForm.controls.timezone.value;

    if (concertTypeId == null) {
      this.messageService.add({
        severity: 'error',
        summary: 'Concert type is required',
      });
      return null;
    }

    if (timezone == null) {
      this.messageService.add({
        severity: 'error',
        summary: 'Timezone is required',
      });
      return null;
    }

    return {
      customTitle: customTitle,
      concertTypeId: concertTypeId,
      tourId: tourId ?? null,
      tourLegId: tourLegId ?? null,
      postedStartTime: DateTime.now(), // TODO: use actual value
      timezone: this.concertForm.controls.timezone.value!,
    };
  }

  public fillFormWith(concert: ConcertDetailsDto) {
    this.concertForm.controls.customTitle.setValue(concert.customTitle ?? null);
    this.concertForm.controls.concertTypeId.setValue(concert.concertType?.id ?? null);
    this.concertForm.controls.tour.setValue(concert.tour ?? null);
    this.concertForm.controls.tourLegId.setValue(concert.tourLeg?.id ?? null);
  }

  public reset() {
    this.concertForm.reset({
      customTitle: '',
      concertTypeId: null,
      tour: null,
      tourLegId: null,
    });
  }

  protected readonly timezones = timezones;
}

export class ConcertFormContent {
  customTitle?: string | null;
  concertTypeId?: number | null;
  tourId?: string | null;
  tourLegId?: string | null;
  timezone!: string;
  postedStartTime!: DateTime;
}
