import { Component, inject, OnInit } from '@angular/core';
import { MessageService } from 'primeng/api';
import { LocationsService } from '../../../../../services/locations.service';
import { Router, RouterLink } from '@angular/router';
import { CountryFormComponent } from '../country-form/country-form.component';
import { CountryDto, CreateCityRequestDto } from '../../../../../modules/lpshows-api/v3';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { CityFormComponent, CityFormContent } from '../city-form/city-form.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';

@Component({
  selector: 'app-add-city-page',
  imports: [
    Button,
    Card,
    CountryFormComponent,
    RouterLink,
    CityFormComponent
  ],
  templateUrl: './add-city-page.component.html',
  styleUrl: './add-city-page.component.css',
})
export class AddCityPageComponent implements OnInit {
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
        console.error("Failed to load countries", err);
      }
    });
  }

  onSaveClicked(formContent: CityFormContent) {
    this.isAdding$ = true;

    let request: CreateCityRequestDto = {
      name: formContent.name,
      nativeName: formContent.nativeName,
    };

    this.locationsService.createCity(formContent.countryCode, request).subscribe({
      next: createdCity => {
        console.debug('Created new city', createdCity);
        this.isAdding$ = false;
        this.router.navigate(["/", "admin", "countries", createdCity.countryCode, "cities", createdCity.id]).catch(err => {
          this.messageService.add({severity: "error", summary: "Failed to navigate to the new city"});
        });
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({severity: "error", summary: "Could not create the city", text: errorResponse.message});
        this.isAdding$ = false;
      }
    });
  }
}
