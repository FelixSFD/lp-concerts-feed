import { Component, inject } from '@angular/core';
import { ConfirmationService, MessageService } from 'primeng/api';
import { LocationsService } from '../../../../../services/locations.service';
import { CityWithCountryDto, CountryDto } from '../../../../../modules/lpshows-api/v3';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { Button } from 'primeng/button';
import { ButtonGroup } from 'primeng/buttongroup';
import { Card } from 'primeng/card';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { FormsModule } from '@angular/forms';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { InputText } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-manage-cities-page',
  imports: [
    Button,
    ButtonGroup,
    Card,
    ConfirmDialog,
    FormsModule,
    IconField,
    InputIcon,
    InputText,
    TableModule,
    RouterLink
  ],
  templateUrl: './manage-cities-page.component.html',
  styleUrl: './manage-cities-page.component.css',
})
export class ManageCitiesPageComponent {
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  private locationsService = inject(LocationsService);


  cities$: CityWithCountryDto[] = [];

  isDeletingCity$ = false;

  // true while data is being loaded
  isLoading$ = false;

  globalSearchText$: string = "";


  ngOnInit() {
    this.reloadList(false);
  }


  onDeleteCityClicked(event: Event, city: CityWithCountryDto) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `Do you want to delete the city "${city.name}"?`,
      header: 'Delete city',
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
        this.onDeleteCityConfirm(city);
      }
    });
  }


  onDeleteCityConfirm(city: CityWithCountryDto) {
    this.isDeletingCity$ = true;

    if (city) {
      this.locationsService.deleteCity(city.countryCode, Number(city.id))
        .subscribe({
          next: () => {
            this.reloadList(false);
            this.isDeletingCity$ = false;
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            this.messageService.add({
              severity: "danger",
              summary: "Could not load delete city!",
              text: errorResponse.message,
            });
            this.isDeletingCity$ = false;
          }
        });
    }
  }


  private reloadList(cache: boolean) {
    this.locationsService.getCities().subscribe({
      next: cities => {
        this.cities$ = cities;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({
          severity: "error",
          summary: "Could not load cities!",
          text: errorResponse.message,
        });
      }
    })
  }
}
