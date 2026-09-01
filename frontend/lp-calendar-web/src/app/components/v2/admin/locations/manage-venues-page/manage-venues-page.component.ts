import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ConfirmationService, MessageService } from 'primeng/api';
import { LocationsService } from '../../../../../services/locations.service';
import { CountryDto, VenueDto, VenueWithCityDto } from '../../../../../modules/lpshows-api/v3';
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
    FormsModule
  ],
  templateUrl: './manage-venues-page.component.html',
  styleUrl: './manage-venues-page.component.css',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class ManageVenuesPageComponent {
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  private locationsService = inject(LocationsService);


  venues$: VenueDto[] = [];

  isDeletingVenue$ = false;

  // true while data is being loaded
  isLoading$ = false;

  globalSearchText$: string = "";


  ngOnInit() {
    this.reloadList(false);
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
    this.isDeletingVenue$ = true;

    if (venue) {
      this.locationsService.deleteVenue(Number(venue.id))
        .subscribe({
          next: () => {
            this.reloadList(false);
            this.isDeletingVenue$ = false;
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            this.messageService.add({
              severity: "danger",
              summary: "Could not load delete venue!",
              text: errorResponse.message,
            });
            this.isDeletingVenue$ = false;
          }
        });
    }
  }


  private reloadList(cache: boolean) {
    this.locationsService.getVenues().subscribe({
      next: venues => {
        this.venues$ = venues;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({
          severity: "error",
          summary: "Could not load venues!",
          text: errorResponse.message,
        });
      }
    })
  }
}
