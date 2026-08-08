import { Component, inject, viewChild } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { LocationsService } from '../../../../../services/locations.service';
import { CountryFormComponent, CountryFormContent } from '../country-form/country-form.component';
import {
  CountryDto,
  CreateStateRequestDto, StateDto,
  StateWithCountryDto,
  UpdateCountryRequestDto, UpdateStateRequestDto
} from '../../../../../modules/lpshows-api/v3';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { Dialog } from 'primeng/dialog';
import { Divider } from 'primeng/divider';
import { StateFormComponent } from '../state-form/state-form.component';
import { ButtonGroup } from 'primeng/buttongroup';
import { ConfirmDialog } from 'primeng/confirmdialog';

@Component({
  selector: 'app-edit-country-page',
  imports: [
    Button,
    Card,
    CountryFormComponent,
    RouterLink,
    FormsModule,
    TableModule,
    Dialog,
    Divider,
    StateFormComponent,
    ButtonGroup,
    ConfirmDialog
  ],
  templateUrl: './edit-country-page.component.html',
  styleUrl: './edit-country-page.component.css',
})
export class EditCountryPageComponent {
  private activeRoute = inject(ActivatedRoute);
  private messageService = inject(MessageService);
  private locationsService = inject(LocationsService);
  private confirmationService = inject(ConfirmationService);

  private countryFormComponent = viewChild(CountryFormComponent);
  private addStateFormComponent = viewChild(StateFormComponent);

  currentCountryId: string = "";

  isSaving$ = false;

  statesInCountry$: StateWithCountryDto[] = [];
  isLoadingStates$ = false;

  isShowingAddStateDialog$ = false;
  isAddingState$ = false;

  isShowingEditStateDialog$ = false;
  isEditingState$ = false;

  isDeletingState$ = false;


  ngOnInit() {
    this.activeRoute.data.subscribe(data => {
      console.debug("Resolved country data:", data);
      console.debug("Resolved type:", data['country'].type);

      if (data['country'].type === 'ErrorResponseDto') {
        this.messageService.add({severity: "error", summary: "Failed to load country", detail: data['country'].message, sticky: true});
        return;
      }

      this.currentCountryId = data['country'].isoCode;
      this.loadStatesInCountry();
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

  private loadStatesInCountry() {
    this.isLoadingStates$ = true;
    this.locationsService.getStatesIn(this.currentCountryId).subscribe({
      next: states => {
        this.statesInCountry$ = states;
        this.isLoadingStates$ = false;
      },
      error: err => {
        this.isLoadingStates$ = false;
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({severity: "error", summary: "Failed to load states in country", detail: errorResponse.message});
      }
    });
  }

  dismissAddStateModal() {
    this.isShowingAddStateDialog$ = false;
  }

  onAddStateClicked() {
    this.isShowingAddStateDialog$ = true;
  }

  onAddStateConfirm() {
    this.isAddingState$ = true;

    let formContent = this.addStateFormComponent()?.readFromForm();

    if (formContent?.code == null || formContent?.name == null || formContent?.nativeName == null) {
      this.messageService.add({severity: "error", summary: "Failed to create state", detail: "Please fill in all fields"});
      this.isAddingState$ = false;
      return;
    }

    let createRequest: CreateStateRequestDto = {
      code: formContent?.code,
      name: formContent?.name,
      nativeName: formContent?.nativeName
    }
    this.locationsService.createState(this.currentCountryId, createRequest)
      .subscribe({
        next: state => {
          this.isAddingState$ = false;
          this.loadStatesInCountry();
        },
        error: err => {
          this.isAddingState$ = false;
          let errorResponse: ErrorResponseDto = err.error;
          this.messageService.add({severity: "error", summary: "Failed to create state", detail: errorResponse.message});
        }
      })
  }

  dismissEditStateModal() {
    this.isShowingEditStateDialog$ = false;
  }

  onEditStateClicked(content: StateFormComponent, state: StateDto) {
    content.fillFormWith(state);
    this.isShowingEditStateDialog$ = true;
  }

  onEditStateConfirm(content: StateFormComponent) {
    this.isEditingState$ = true;

    let formContent = content.readFromForm();
    console.debug("Form content:", formContent);

    if (formContent?.code == null || formContent?.name == null || formContent?.nativeName == null) {
      this.messageService.add({severity: "error", summary: "Failed to save state", detail: "Please fill in all fields"});
      this.isEditingState$ = false;
      return;
    }

    let updateRequest: UpdateStateRequestDto = {
      name: formContent?.name,
      nativeName: formContent?.nativeName
    }
    this.locationsService.updateState(this.currentCountryId, formContent.code, updateRequest)
      .subscribe({
        next: state => {
          this.isEditingState$ = false;
          this.loadStatesInCountry();
        },
        error: err => {
          this.isEditingState$ = false;
          let errorResponse: ErrorResponseDto = err.error;
          this.messageService.add({severity: "error", summary: "Failed to save state", detail: errorResponse.message});
        }
      })
  }

  onDeleteStateClicked(event: Event, state: StateDto) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `Do you want to delete the state "${state.name}"?`,
      header: 'Delete state',
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
        this.onDeleteStateConfirm(state);
      }
    });
  }


  onDeleteStateConfirm(state: StateDto) {
    this.isDeletingState$ = true;

    if (state) {
      this.locationsService.deleteState(state.countryCode, state.code)
        .subscribe({
          next: () => {
            this.loadStatesInCountry();
            this.isDeletingState$ = false;
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            this.messageService.add({
              severity: "error",
              summary: "Could not load delete state!",
              text: errorResponse.message,
            });
            this.isDeletingState$ = false;
          }
        });
    }
  }
}
