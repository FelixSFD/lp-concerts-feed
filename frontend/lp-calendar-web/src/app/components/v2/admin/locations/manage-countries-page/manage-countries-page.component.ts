import { Component, inject } from '@angular/core';
import { ConfirmationService, MessageService } from 'primeng/api';
import { SongsService } from '../../../../../services/songs.service';
import { ErrorResponseDto, SongDto } from '../../../../../modules/lpshows-api';
import { CountryDto } from '../../../../../modules/lpshows-api/v3';
import { LocationsService } from '../../../../../services/locations.service';

@Component({
  selector: 'app-manage-countries-page',
  imports: [],
  templateUrl: './manage-countries-page.component.html',
  styleUrl: './manage-countries-page.component.css',
})
export class ManageCountriesPageComponent {
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  private locationsService = inject(LocationsService);


  countries$: CountryDto[] = [];

  isDeletingCountry$ = false;

  // true while data is being loaded
  isLoading$ = false;

  globalSearchText$: string = "";


  ngOnInit() {
    this.reloadList(false);
  }


  private reloadList(cache: boolean) {
    this.locationsService.getCountries().subscribe({
      next: countries => {
        this.countries$ = countries;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({
          severity: "danger",
          summary: "Could not load countries!",
          text: errorResponse.message,
        });
      }
    })
  }
}
