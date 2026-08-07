import { Component, inject, viewChild } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { LocationsService } from '../../../../../services/locations.service';
import { CountryFormComponent, CountryFormContent } from '../country-form/country-form.component';
import { UpdateCountryRequestDto } from '../../../../../modules/lpshows-api/v3';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { SongFormComponent } from '../../setlists/song-form/song-form.component';

@Component({
  selector: 'app-edit-country-page',
  imports: [
    Button,
    Card,
    CountryFormComponent,
    RouterLink
  ],
  templateUrl: './edit-country-page.component.html',
  styleUrl: './edit-country-page.component.css',
})
export class EditCountryPageComponent {
  private activeRoute = inject(ActivatedRoute);
  private messageService = inject(MessageService);
  private locationsService = inject(LocationsService);

  private countryFormComponent = viewChild(CountryFormComponent);

  currentCountryId: string = "";

  isSaving$ = false;


  ngOnInit() {
    this.activeRoute.data.subscribe(data => {
      console.debug("Resolved country data:", data);
      console.debug("Resolved type:", data['country'].type);

      if (data['country'].type === 'ErrorResponseDto') {
        this.messageService.add({severity: "error", summary: "Failed to load country", detail: data['country'].message, sticky: true});
        return;
      }

      this.currentCountryId = data['country'].isoCode;
      this.countryFormComponent()?.fillFormWith(data['country']);
    });
  }


  onSaveClicked(formContent: CountryFormContent) {
    this.isSaving$ = true;

    let request: UpdateCountryRequestDto = {
      name: formContent.name,
      nativeName: formContent.nativeName,
    };

    this.locationsService.updateCountry(this.currentCountryId, request).subscribe({
      next: updatedCountry => {
        console.debug('Updated country', updatedCountry);
        this.messageService.add({severity: "success", summary: "Successfully saved this country"});
        this.isSaving$ = false;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({severity: "error", summary: "Failed to save country", detail: errorResponse.message});
        this.isSaving$ = false;
      }
    });
  }
}
