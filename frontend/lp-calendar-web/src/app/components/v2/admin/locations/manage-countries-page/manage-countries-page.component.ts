import { Component, inject, signal } from '@angular/core';
import { ConfirmationService, MessageService } from 'primeng/api';
import { SongsService } from '../../../../../services/songs.service';
import { ErrorResponseDto, SongDto } from '../../../../../modules/lpshows-api';
import { CountryDto } from '../../../../../modules/lpshows-api/v3';
import { LocationsService } from '../../../../../services/locations.service';
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
  selector: 'app-manage-countries-page',
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
  templateUrl: './manage-countries-page.component.html',
  styleUrl: './manage-countries-page.component.css',
})
export class ManageCountriesPageComponent {
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  private locationsService = inject(LocationsService);


  countries$ = signal<CountryDto[]>([]);

  isDeletingCountry$ = signal(false);

  // true while data is being loaded
  isLoading$ = signal(false);

  globalSearchText$ = signal('');


  ngOnInit() {
    this.reloadList(false);
  }


  onDeleteCountryClicked(event: Event, country: CountryDto) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `Do you want to delete the country "${country.name}"?`,
      header: 'Delete country',
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
        this.onDeleteCountryConfirm(country);
      }
    });
  }


  onDeleteCountryConfirm(country: CountryDto) {
    this.isDeletingCountry$.set(true);

    if (country) {
      this.locationsService.deleteCountry(country.isoCode!)
        .subscribe({
          next: () => {
            this.reloadList(false);
            this.isDeletingCountry$.set(false);
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            this.messageService.add({
              severity: "danger",
              summary: "Could not load delete country!",
              text: errorResponse.message,
            });
            this.isDeletingCountry$.set(false);
          }
        });
    }
  }


  private reloadList(cache: boolean) {
    this.locationsService.getCountries().subscribe({
      next: countries => {
        this.countries$.set(countries);
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({
          severity: "error",
          summary: "Could not load countries!",
          text: errorResponse.message,
        });
      }
    })
  }
}
