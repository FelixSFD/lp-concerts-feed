import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ConcertsApi, CreateConcertRequestDto } from '../../../../../modules/lpshows-api/v3';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { ConcertFormComponent, ConcertFormContent } from '../concert-form/concert-form.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';

@Component({
  selector: 'app-add-concert-page',
  imports: [
    Button,
    Card,
    RouterLink,
    ConcertFormComponent
  ],
  templateUrl: './add-concert-page.component.html',
  styleUrl: './add-concert-page.component.css',
})
export class AddConcertPageComponent {
  private router = inject(Router);
  private messageService = inject(MessageService);
  private concertsApi = inject(ConcertsApi);

  isSaving$ = false;

  onSaveClicked(formContent: ConcertFormContent) {
    this.isSaving$ = true;

    const request: CreateConcertRequestDto = {
      customTitle: formContent.customTitle ?? undefined,
      concertTypeId: formContent.concertTypeId != null ? String(formContent.concertTypeId) : undefined,
      tourId: formContent.tourId ?? undefined,
    };

    this.concertsApi.createConcert(request).subscribe({
      next: (createdConcert: any) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Successfully created concert',
        });
        this.router.navigate(['/', 'admin', 'concerts', createdConcert?.id ?? '']);
      },
      error: (err) => {
        const errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({
          severity: 'error',
          summary: 'Failed to create concert',
          detail: errorResponse?.message ?? err?.message,
        });
        this.isSaving$ = false;
      },
    });
  }
}
