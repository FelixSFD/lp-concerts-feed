import { Component, effect, inject, Input, signal } from '@angular/core';
import {
  ConcertStatusValueDto,
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
    Card
  ],
  selector: 'app-import-concert-dialog-content',
  styleUrl: './import-concert-dialog-content.component.css',
  templateUrl: './import-concert-dialog-content.component.html',
})
export class ImportConcertDialogContentComponent {
  private formBuilder = inject(FormBuilder);

  @Input("import-plan")
  importPlan = signal<ImportConcertPreviewDto | null>(null);

  private importPlanChangedEffect = effect(() => {
    let plan = this.importPlan();
    console.debug("Import plan changed:", plan);
    if (plan) {
      //this.importPlanForm.controls.concertTypeId.setValue(plan.concertType?.id ?? null);
    }
  });

  importPlanForm = this.formBuilder.group({
    tour: new FormControl<TourDto | null>(null, []),
    tourLegId: new FormControl<string | null>(null),
  });
}
