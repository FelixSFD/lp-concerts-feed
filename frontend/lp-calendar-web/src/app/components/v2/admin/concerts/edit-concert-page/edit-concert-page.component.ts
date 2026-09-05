import { ChangeDetectionStrategy, Component, inject, OnInit, signal, viewChild } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import {
  ConcertsApi,
  ConcertDetailsDto,
  CreateConcertRequestDto,
  UpdateConcertRequestDto
} from '../../../../../modules/lpshows-api/v3';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { ConcertFormComponent, ConcertFormContent } from '../concert-form/concert-form.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { TableModule } from 'primeng/table';
import { Divider } from 'primeng/divider';

@Component({
  selector: 'app-edit-concert-page',
  imports: [
    Button,
    Card,
    ConfirmDialog,
    RouterLink,
    TableModule,
    ConcertFormComponent,
    Divider
  ],
  templateUrl: './edit-concert-page.component.html',
  styleUrl: './edit-concert-page.component.css',
})
export class EditConcertPageComponent implements OnInit {
  private activeRoute = inject(ActivatedRoute);
  private messageService = inject(MessageService);
  private concertsApi = inject(ConcertsApi);

  concertFormComponent = viewChild(ConcertFormComponent);

  currentConcertId: string = '';
  currentConcertTitle: string = '';

  isSaving = signal(false);

  ngOnInit() {
    this.activeRoute.data.subscribe((data) => {
      const concert = data['concert'] as ConcertDetailsDto | null;
      if (!concert || concert.id == null) {
        this.messageService.add({
          severity: 'error',
          summary: 'Failed to load concert',
          detail: (concert as ErrorResponseDto).message,
          sticky: true,
        });

        this.concertFormComponent()?.concertForm.disable();
        return;
      }

      this.currentConcertId = concert.id ?? null;
      this.concertFormComponent()?.fillFormWith(concert);
    });
  }

  onSaveClicked(formContent: ConcertFormContent) {
    this.isSaving.set(true);

    const request: UpdateConcertRequestDto = {
      customTitle: formContent.customTitle ?? undefined,
      concertTypeId: formContent.concertTypeId != null ? String(formContent.concertTypeId) : undefined,
      tourId: formContent.tourId ?? undefined,
      tourLegId: formContent.tourLegId ?? undefined,
      venueId: formContent.venueId ?? undefined,
      postedStartTime: formContent.postedStartTime.toISO()!,
      doorsTime: formContent.doorsTime?.toISO() ?? undefined,
      mainStageTime: formContent.mainStageTime?.toISO() ?? undefined,
      expectedSetDurationMinutes: String(formContent.expectedSetDuration) ?? undefined,
    };

    this.concertsApi.updateConcert(this.currentConcertId, request).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Successfully saved concert',
        });
        this.isSaving.set(false);
      },
      error: (err) => {
        const errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({
          severity: 'error',
          summary: 'Failed to create concert',
          detail: errorResponse?.message ?? err?.message,
        });
        this.isSaving.set(false);
      },
    });
  }
}
