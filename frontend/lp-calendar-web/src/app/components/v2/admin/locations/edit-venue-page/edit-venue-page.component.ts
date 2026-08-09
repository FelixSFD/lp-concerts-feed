import { Component, inject, viewChild } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { LocationsService } from '../../../../../services/locations.service';
import {
  CountryDto,
  UpdateVenueRequestDto
} from '../../../../../modules/lpshows-api/v3';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { VenueFormComponent, VenueFormContent } from '../venue-form/venue-form.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { TableModule } from 'primeng/table';

@Component({
  selector: 'app-edit-venue-page',
  imports: [
    Button,
    Card,
    TableModule,
    VenueFormComponent,
    RouterLink
  ],
  templateUrl: './edit-venue-page.component.html',
  styleUrl: './edit-venue-page.component.css',
})
export class EditVenuePageComponent {
  private activeRoute = inject(ActivatedRoute);
  private messageService = inject(MessageService);
  private locationsService = inject(LocationsService);

  private venueFormComponent = viewChild(VenueFormComponent);

  currentVenueId: number = 0;

  isSaving$ = false;

  availableCountries$: CountryDto[] = [];

  ngOnInit() {
    this.locationsService.getCountries().subscribe({
      next: countries => {
        this.availableCountries$ = countries;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({
          severity: "error",
          summary: "Could not load countries!",
          text: errorResponse.message,
        });
      }
    });

    this.activeRoute.data.subscribe(data => {
      console.debug("Resolved venue data:", data);
      console.debug("Resolved type:", data['venue'].type);

      if (data['venue'].type === 'ErrorResponseDto') {
        this.messageService.add({severity: "error", summary: "Failed to load venue", detail: data['venue'].message, sticky: true});
        return;
      }

      this.currentVenueId = data['venue'].id;
      this.venueFormComponent()?.fillFormWith(data['venue']);
    });
  }


  onSaveClicked(formContent: VenueFormContent) {
    this.isSaving$ = true;

    let request: UpdateVenueRequestDto = {
      countryCode: formContent.countryCode,
      cityId: formContent.cityId.toString(),
      timeZone: formContent.timeZone
    };

    this.locationsService.updateVenue(this.currentVenueId, request).subscribe({
      next: updatedVenue => {
        console.debug('Updated venue', updatedVenue);
        this.messageService.add({severity: "success", summary: "Successfully saved this venue"});
        this.isSaving$ = false;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({severity: "error", summary: "Failed to save venue", detail: errorResponse.message});
        this.isSaving$ = false;
      }
    });
  }
}
