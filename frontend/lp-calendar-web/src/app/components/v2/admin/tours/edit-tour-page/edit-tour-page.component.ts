import { Component, inject, viewChild } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ToursService } from '../../../../../services/tours.service';
import { TourDto, TourLegDto } from '../../../../../modules/lpshows-api/v3';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { TourFormComponent, TourFormContent } from '../tour-form/tour-form.component';
import { TourLegFormComponent, TourLegFormContent } from '../tour-leg-form/tour-leg-form.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { Dialog } from 'primeng/dialog';
import { TableModule } from 'primeng/table';

@Component({
  selector: 'app-edit-tour-page',
  imports: [
    Button,
    Card,
    ConfirmDialog,
    Dialog,
    RouterLink,
    TableModule,
    TourFormComponent,
    TourLegFormComponent
  ],
  templateUrl: './edit-tour-page.component.html',
  styleUrl: './edit-tour-page.component.css',
})
export class EditTourPageComponent {
  private activeRoute = inject(ActivatedRoute);
  private messageService = inject(MessageService);
  private toursService = inject(ToursService);
  private confirmationService = inject(ConfirmationService);

  tourFormComponent = viewChild(TourFormComponent);
  tourLegFormComponent = viewChild(TourLegFormComponent);

  currentTourId: string = '';
  currentTourName: string = '';
  legsInTour$: TourLegDto[] = [];

  isSaving$ = false;
  isShowingAddLegDialog$ = false;
  isAddingLeg$ = false;

  ngOnInit() {
    this.activeRoute.data.subscribe((data) => {
      const tour: TourDto | ErrorResponseDto = data['tour'];
      if (!tour) {
        return;
      }

      if ('type' in tour && tour.type === 'ErrorResponseDto') {
        this.messageService.add({
          severity: 'error',
          summary: 'Failed to load tour',
          detail: (tour as ErrorResponseDto).message,
          sticky: true,
        });
        return;
      }

      const tourDto = tour as TourDto;
      this.currentTourId = tourDto.id ?? '';
      this.currentTourName = tourDto.name ?? '';
      this.legsInTour$ = tourDto.legs ?? [];
      this.tourFormComponent()?.fillFormWith(tourDto);
    });
  }

  reloadTour() {
    if (!this.currentTourId) {
      return;
    }
    this.toursService.getTour(this.currentTourId).subscribe({
      next: (tour) => {
        this.currentTourName = tour.name ?? '';
        this.legsInTour$ = tour.legs ?? [];
        this.tourFormComponent()?.fillFormWith(tour);
      },
      error: (err) => {
        console.error('Failed to reload tour', err);
      },
    });
  }

  onOpenAddLegDialog() {
    this.tourLegFormComponent()?.reset();
    this.isShowingAddLegDialog$ = true;
  }

  onSaveLegClicked(formContent: TourLegFormContent) {
    this.isAddingLeg$ = true;

    this.toursService
      .createTourLeg(this.currentTourId, {
        id: formContent.id,
        name: formContent.name,
      })
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Successfully added tour leg',
          });
          this.isShowingAddLegDialog$ = false;
          this.isAddingLeg$ = false;
          this.reloadTour();
        },
        error: (err) => {
          const errorResponse: ErrorResponseDto = err.error;
          this.messageService.add({
            severity: 'error',
            summary: 'Failed to add tour leg',
            detail: errorResponse?.message ?? err?.message,
          });
          this.isAddingLeg$ = false;
        },
      });
  }

  onDeleteLegClicked(event: MouseEvent, leg: TourLegDto) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `Do you really want to delete the leg "${leg.name}" (${leg.id})?`,
      header: 'Delete tour leg',
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
        this.toursService.deleteTourLeg(this.currentTourId, leg.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Successfully deleted tour leg',
            });
            this.reloadTour();
          },
          error: (err) => {
            const errorResponse: ErrorResponseDto = err.error;
            this.messageService.add({
              severity: 'error',
              summary: 'Failed to delete tour leg',
              detail: errorResponse?.message ?? err?.message,
            });
          },
        });
      },
      reject: () => {},
    });
  }
}
