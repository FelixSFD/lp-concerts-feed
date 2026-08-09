import { Component, inject } from '@angular/core';
import {Button} from "primeng/button";
import {Card} from "primeng/card";
import { MessageService } from 'primeng/api';
import { Router, RouterLink } from '@angular/router';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { CountryFormComponent, CountryFormContent } from '../country-form/country-form.component';
import { CreateCountryRequestDto } from '../../../../../modules/lpshows-api/v3';
import { LocationsService } from '../../../../../services/locations.service';

@Component({
  selector: 'app-add-country-page',
  imports: [
    Button,
    Card,
    CountryFormComponent,
    RouterLink
  ],
  templateUrl: './add-country-page.component.html',
  styleUrl: './add-country-page.component.css',
})
export class AddCountryPageComponent {
  private messageService = inject(MessageService);
  private locationsService = inject(LocationsService);
  private router = inject(Router);

  isAdding$ = false;

  onSaveClicked(formContent: CountryFormContent) {
    this.isAdding$ = true;

    let request: CreateCountryRequestDto = {
      isoCode: formContent.isoCode,
      name: formContent.name,
      nativeName: formContent.nativeName,
    };

    this.locationsService.createCountry(request).subscribe({
      next: createdCountry => {
        console.debug('Created new country', createdCountry);
        this.isAdding$ = false;
        this.router.navigate(["/", "admin", "countries", createdCountry.isoCode]).catch(err => {
          this.messageService.add({severity: "error", summary: "Failed to navigate to the new country"});
        });
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({severity: "error", summary: "Could not create the country", text: errorResponse.message});
        this.isAdding$ = false;
      }
    });
  }
}
