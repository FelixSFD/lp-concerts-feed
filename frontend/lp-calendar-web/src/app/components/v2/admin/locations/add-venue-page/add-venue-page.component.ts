import { Component, inject, OnInit } from '@angular/core';
import { MessageService } from 'primeng/api';
import { LocationsService } from '../../../../../services/locations.service';
import { Router, RouterLink } from '@angular/router';
import { CountryDto, CreateVenueRequestDto } from '../../../../../modules/lpshows-api/v3';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { VenueFormComponent, VenueFormContent } from '../venue-form/venue-form.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';

@Component({
  selector: 'app-add-venue-page',
  imports: [
    VenueFormComponent,
    Button,
    Card,
    RouterLink
  ],
  templateUrl: './add-venue-page.component.html',
  styleUrl: './add-venue-page.component.css',
})
export class AddVenuePageComponent implements OnInit {
  private messageService = inject(MessageService);
  private locationsService = inject(LocationsService);
  private router = inject(Router);

  isAdding$ = false;

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
  }

  onSaveClicked(formContent: VenueFormContent) {
    this.isAdding$ = true;

    let request: CreateVenueRequestDto = {
      countryCode: formContent.countryCode,
      stateCode: formContent.stateCode ?? undefined,
      cityId: formContent.cityId,
      currentName: formContent.currentName,
      timeZoneId: formContent.timeZoneId,
      latitude: formContent.latitude ?? null,
      longitude: formContent.longitude ?? null,
    };

    this.locationsService.createVenue(request).subscribe({
      next: createdVenue => {
        console.debug('Created new venue', createdVenue);
        this.isAdding$ = false;
        this.router.navigate(["/", "admin", "venues", createdVenue.id]).catch(err => {
          this.messageService.add({severity: "error", summary: "Failed to navigate to the new venue"});
        });
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({severity: "error", summary: "Could not create the venue", text: errorResponse.message});
        this.isAdding$ = false;
      }
    });
  }
}
