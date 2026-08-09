import { Component, inject, viewChild } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { LocationsService } from '../../../../../services/locations.service';
import {
  CityWithCountryDto,
  CountryDto,
  UpdateCityRequestDto,
} from '../../../../../modules/lpshows-api/v3';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { CityFormComponent, CityFormContent } from '../city-form/city-form.component';
import { Button } from 'primeng/button';
import { ButtonGroup } from 'primeng/buttongroup';
import { Card } from 'primeng/card';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { CountryFormComponent } from '../country-form/country-form.component';
import { Dialog } from 'primeng/dialog';
import { Divider } from 'primeng/divider';
import { StateFormComponent } from '../state-form/state-form.component';
import { TableModule } from 'primeng/table';

@Component({
  selector: 'app-edit-city-page',
  imports: [
    Button,
    ButtonGroup,
    Card,
    ConfirmDialog,
    CountryFormComponent,
    Dialog,
    Divider,
    StateFormComponent,
    TableModule,
    RouterLink,
    CityFormComponent
  ],
  templateUrl: './edit-city-page.component.html',
  styleUrl: './edit-city-page.component.css',
})
export class EditCityPageComponent {
  private activeRoute = inject(ActivatedRoute);
  private messageService = inject(MessageService);
  private locationsService = inject(LocationsService);
  private confirmationService = inject(ConfirmationService);

  private cityFormComponent = viewChild(CityFormComponent);

  currentCityId: number = 0;
  currentCountryCode: string = "";

  isSaving$ = false;

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

    this.activeRoute.data.subscribe(data => {
      console.debug("Resolved city data:", data);
      console.debug("Resolved type:", data['city'].type);

      if (data['city'].type === 'ErrorResponseDto') {
        this.messageService.add({severity: "error", summary: "Failed to load city", detail: data['city'].message, sticky: true});
        return;
      }

      this.currentCityId = data['city'].id;
      this.currentCountryCode = data['city'].countryCode;
      this.cityFormComponent()?.fillFormWith(data['city']);
    });
  }


  onSaveClicked(formContent: CityFormContent) {
    this.isSaving$ = true;

    let request: UpdateCityRequestDto = {
      name: formContent.name,
      nativeName: formContent.nativeName,
    };

    this.locationsService.updateCity(this.currentCountryCode, this.currentCityId, request).subscribe({
      next: updatedCity => {
        console.debug('Updated city', updatedCity);
        this.messageService.add({severity: "success", summary: "Successfully saved this city"});
        this.isSaving$ = false;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({severity: "error", summary: "Failed to save city", detail: errorResponse.message});
        this.isSaving$ = false;
      }
    });
  }
}
