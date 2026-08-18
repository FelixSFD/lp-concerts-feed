import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ToursService } from '../../../../../services/tours.service';
import { CreateTourRequestDto } from '../../../../../modules/lpshows-api/v3';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { TourFormComponent, TourFormContent } from '../tour-form/tour-form.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';

@Component({
  selector: 'app-add-tour-page',
  imports: [
    Button,
    Card,
    RouterLink,
    TourFormComponent
  ],
  templateUrl: './add-tour-page.component.html',
  styleUrl: './add-tour-page.component.css',
})
export class AddTourPageComponent {
  private router = inject(Router);
  private messageService = inject(MessageService);
  private toursService = inject(ToursService);

  isSaving$ = false;

  onSaveClicked(formContent: TourFormContent) {
    this.isSaving$ = true;

    const request: CreateTourRequestDto = {
      id: formContent.id,
      name: formContent.name,
    };

    this.toursService.createTour(request).subscribe({
      next: (createdTour) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Successfully created tour',
        });
        this.router.navigate(['/', 'admin', 'tours', createdTour.id ?? formContent.id]);
      },
      error: (err) => {
        const errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({
          severity: 'error',
          summary: 'Failed to create tour',
          detail: errorResponse?.message ?? err?.message,
        });
        this.isSaving$ = false;
      },
    });
  }
}
