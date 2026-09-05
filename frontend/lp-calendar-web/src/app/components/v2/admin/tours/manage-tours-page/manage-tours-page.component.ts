import { Component, inject, signal } from '@angular/core';
import { ConfirmationService, MessageService } from 'primeng/api';
import { RouterLink } from '@angular/router';
import { ToursService } from '../../../../../services/tours.service';
import { TourDto } from '../../../../../modules/lpshows-api/v3';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { Button } from 'primeng/button';
import { ButtonGroup } from 'primeng/buttongroup';
import { Card } from 'primeng/card';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { InputText } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';

@Component({
  selector: 'app-manage-tours-page',
  imports: [
    Button,
    ButtonGroup,
    Card,
    ConfirmDialog,
    FormsModule,
    IconField,
    InputIcon,
    InputText,
    RouterLink,
    TableModule
  ],
  templateUrl: './manage-tours-page.component.html',
  styleUrl: './manage-tours-page.component.css',
})
export class ManageToursPageComponent {
  private toursService = inject(ToursService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);

  tours$ = signal<TourDto[]>([]);
  isLoading$ = signal<boolean>(true);
  isDeletingTour$ = signal<boolean>(false);
  globalSearchText$ = signal<string>('');

  ngOnInit() {
    this.loadTours();
  }

  loadTours() {
    this.isLoading$.set(true);
    this.toursService.getTours().subscribe({
      next: (tours) => {
        this.tours$.set(tours);
        this.isLoading$.set(false);
      },
      error: (err) => {
        console.error('Failed to load tours', err);
        this.messageService.add({
          severity: 'error',
          summary: 'Failed to load tours',
          detail: err?.error?.message ?? err?.message,
        });
        this.isLoading$.set(false);
      },
    });
  }

  onDeleteTourClicked(event: MouseEvent, tour: TourDto) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `Do you really want to delete the tour "${tour.name}" (${tour.id})?`,
      header: 'Delete tour',
      icon: 'pi pi-info-circle',
      rejectLabel: 'Cancel',
      rejectButtonProps: {
        label: 'Cancel',
        severity: 'secondary',
        outlined: true,
      },
      acceptButtonProps: {
        label: 'Delete',
        severity: 'danger',
      },
      accept: () => {
        this.isDeletingTour$.set(true);
        this.toursService.deleteTour(tour.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Successfully deleted tour',
            });
            this.isDeletingTour$.set(false);
            this.loadTours();
          },
          error: (err) => {
            let errorResponse: ErrorResponseDto = err.error;
            this.messageService.add({
              severity: 'error',
              summary: 'Failed to delete tour',
              detail: errorResponse?.message ?? err?.message,
            });
            this.isDeletingTour$.set(false);
          },
        });
      },
      reject: () => {},
    });
  }
}
