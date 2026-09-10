import { Component, effect, EventEmitter, inject, Input, Output, signal } from '@angular/core';
import {
  AddTourLegRequestDto,
  ConcertStatusValueDto, ConcertTypeDto, CreateTourRequestDto,
  ImportConcertPreviewDto,
  TourDto
} from '../../../../../modules/lpshows-api/v3';
import { SelectConcertTypeComponent } from '../select-concert-type/select-concert-type.component';
import { FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { FloatLabel } from 'primeng/floatlabel';
import { Button } from 'primeng/button';
import { Divider } from 'primeng/divider';
import { SelectTourComponent } from '../select-tour/select-tour.component';
import { SelectTourLegComponent } from '../select-tour-leg/select-tour-leg.component';
import { Message } from 'primeng/message';
import { Accordion, AccordionContent, AccordionHeader, AccordionPanel } from 'primeng/accordion';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { Toast } from 'primeng/toast';
import { LocationsService } from '../../../../../services/locations.service';
import { ToursService } from '../../../../../services/tours.service';
import { Tooltip } from 'primeng/tooltip';
import { DatePipe } from '@angular/common';
import { DateTime } from 'luxon';
import { InputGroup } from 'primeng/inputgroup';
import { InputGroupAddon } from 'primeng/inputgroupaddon';

@Component({
  imports: [
    SelectConcertTypeComponent,
    FormsModule,
    ReactiveFormsModule,
    FloatLabel,
    Button,
    Divider,
    SelectTourComponent,
    SelectTourLegComponent,
    Message,
    Accordion,
    AccordionPanel,
    AccordionHeader,
    AccordionContent,
    Card,
    InputText,
    Toast,
    Tooltip,
    DatePipe,
    InputGroup,
    InputGroupAddon
  ],
  selector: 'app-import-concert-dialog-content',
  styleUrl: './import-concert-dialog-content.component.css',
  templateUrl: './import-concert-dialog-content.component.html',
})
export class ImportConcertDialogContentComponent {
  private messageService = inject(MessageService);
  private formBuilder = inject(FormBuilder);
  private toursService = inject(ToursService);
  private locationsService = inject(LocationsService);

  @Input("import-plan")
  importPlan = signal<ImportConcertPreviewDto | null>(null);

  @Output("applyClicked")
  applyClickedEvent: EventEmitter<ApplyClickedEvent> = new EventEmitter<ApplyClickedEvent>();

  private importPlanChangedEffect = effect(() => {
    let plan = this.importPlan();
    console.debug("Import plan changed:", plan);
    if (plan) {
      //this.importPlanForm.controls.concertTypeId.setValue(plan.concertType?.id ?? null);
      this.createTourForm.controls.tourName.setValue(plan.tourName ?? null);
      this.createTourLegForm.controls.legName.setValue(plan.tourLegName ?? null);
    }
  });

  createTourForm = this.formBuilder.group({
    tourId: new FormControl<string | null>(null, [Validators.required]),
    tourName: new FormControl<string | null>(null, [Validators.required]),
  });

  createTourLegForm = this.formBuilder.group({
    legId: new FormControl<string | null>(null, [Validators.required]),
    legName: new FormControl<string | null>(null, [Validators.required]),
  });


  onCreateTourClicked() {
    console.debug("Create tour clicked");

    let tourName = this.createTourForm.controls.tourName.value;
    if (!tourName) {
      console.error("Tour name is required");
      this.messageService.add({severity: "error", summary: "Tour name is required", detail: "Please enter a tour name"});
      return;
    }

    let createTourRequest: CreateTourRequestDto = {
      id: this.createTourForm.controls.tourId.value ?? tourName.toLowerCase().replaceAll(" ", "-"),
      name: tourName
    };
    this.toursService.createTour(createTourRequest).subscribe({
      next: (createdTour) => {
        console.debug("Created tour:", createdTour);
        this.importPlan.update(prev => {
          if (prev) {
            prev.foundTours = [createdTour];
            console.debug("Updated tours:", prev.foundTours);
          }

          console.debug("importPlan.update() will return:", prev);
          return {
            ...prev,
            foundTours: [createdTour]
          };
        });
        console.debug("Updated import plan:", this.importPlan());
      },
      error: (err) => {
        console.error("Could not create tour:", err);
      }
    });
  }


  onCreateTourLegClicked() {
    console.debug("Create tour leg clicked");

    let tourLegName = this.createTourLegForm.controls.legName.value;
    if (!tourLegName) {
      console.error("Tour leg name is required");
      this.messageService.add({severity: "error", summary: "Tour leg name is required", detail: "Please enter a tour leg name"});
      return;
    }

    let createTourLegRequest: AddTourLegRequestDto = {
      id: this.createTourLegForm.controls.legId.value ?? tourLegName.toLowerCase().replaceAll(" ", "-"),
      name: tourLegName
    };
    this.toursService.createTourLeg(this.importPlan()?.foundTours?.at(0)?.id ?? "null", createTourLegRequest).subscribe({
      next: (createdTourLeg) => {
        console.debug("Created tour leg:", createdTourLeg);
        this.importPlan.update(prev => {
          if (prev) {
            prev.foundTourLegs = [createdTourLeg];
            console.debug("Updated tour legs:", prev.foundTourLegs);
          }

          console.debug("importPlan.update() will return:", prev);
          return {
            ...prev,
            foundTourLegs: [createdTourLeg]
          };
        });
        console.debug("Updated import plan:", this.importPlan());
      },
      error: (err) => {
        console.error("Could not create tour leg:", err);
      }
    });
  }


  protected generateId(nameControl: FormControl<string | null>, idControl: FormControl<string | null>) {
    let name = nameControl.getRawValue();
    let id = name?.toLowerCase().replaceAll(" ", "-") ?? null;
    if (id) {
      console.debug("Generated id:", id);
      idControl.setValue(id);
    }
  }

  protected onApplyClicked() {
    let plan = this.importPlan();
    if (!plan) {
      console.error("No import plan available");
      return;
    }

    let applyEvent: ApplyClickedEvent = {
      concertType: plan.concertType ?? null,
      tour: plan.foundTours?.at(0) ?? null,
      tourLeg: plan.foundTourLegs?.at(0) ?? null,
      postedStartTime: DateTime.fromISO(plan.postedStartTime ?? "").toJSDate() ?? null,
    };

    console.debug("prepared ApplyClickedEvent:", applyEvent);
    this.applyClickedEvent.emit(applyEvent);
  }

  protected readonly DatePipe = DatePipe;
}

export class ApplyClickedEvent {
  concertType: ConcertTypeDto | null = null;
  tour: TourDto | null = null;
  tourLeg: TourDto | null = null;
  postedStartTime: Date | null = null;
}
