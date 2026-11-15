import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ConfirmationService, MessageService } from 'primeng/api';
import { LocationsService } from '../../../../../services/locations.service';
import { CountryDto, ProblemDetailsDto, VenueDto, VenueWithCityDto } from '../../../../../modules/lpshows-api/v3';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { Button } from 'primeng/button';
import { ButtonGroup } from 'primeng/buttongroup';
import { Card } from 'primeng/card';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { InputText } from 'primeng/inputtext';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { RouterLink } from '@angular/router';
import { NgTemplateOutlet } from '@angular/common';
import { Tooltip } from 'primeng/tooltip';

@Component({
  selector: 'app-manage-venues-page',
  imports: [
    Button,
    ButtonGroup,
    Card,
    ConfirmDialog,
    IconField,
    InputIcon,
    InputText,
    ReactiveFormsModule,
    TableModule,
    RouterLink,
    FormsModule,
    NgTemplateOutlet,
    Tooltip
  ],
  templateUrl: './manage-venues-page.component.html',
  styleUrl: './manage-venues-page.component.css',
})
export class ManageVenuesPageComponent {
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  private locationsService = inject(LocationsService);


  venues$ = signal<VenueDto[]>([]);

  isDeletingVenue$ = signal(false);

  // true while data is being loaded
  isLoading$ = signal(false);

  globalSearchText$ = signal("");

  ngOnInit() {
    this.reloadList(false).then();
  }


  onDeleteVenueClicked(event: Event, venue: VenueDto) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `Do you want to delete the venue "${venue.currentName}"?`,
      header: 'Delete venue',
      icon: 'pi pi-info-circle',
      rejectLabel: 'Cancel',
      rejectButtonProps: {
        label: 'Cancel',
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: 'Delete',
        severity: 'danger'
      },

      accept: () => {
        this.onDeleteVenueConfirm(venue);
      }
    });
  }


  onDeleteVenueConfirm(venue: VenueDto) {
    this.isDeletingVenue$.set(true);

    if (venue) {
      this.locationsService.deleteVenue(Number(venue.id))
        .subscribe({
          next: async () => {
            await this.reloadList(false);
            this.isDeletingVenue$.set(false);
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            this.messageService.add({
              severity: "danger",
              summary: "Could not delete venue!",
              text: errorResponse.message,
            });
            this.isDeletingVenue$.set(false);
          }
        });
    }
  }


  private async reloadList(cache: boolean) {
    try {
      let venues = await this.locationsService.getVenues();
      this.venues$.set(venues);
    } catch (e) {
      let problemDetails: ProblemDetailsDto = e as ProblemDetailsDto;
      this.messageService.add({
        severity: "error",
        summary: "Failed to load the venues",
        detail: problemDetails.detail,
      });
    }
  }
}
