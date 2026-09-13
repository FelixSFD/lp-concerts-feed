import { Component, effect, EventEmitter, inject, Input, OnInit, Output, signal } from '@angular/core';
import {
  AddTourLegRequestDto,
  ConcertTypeDto, CreateCityRequestDto, CreateCountryRequestDto, CreateTourRequestDto, CreateVenueRequestDto,
  ImportConcertPreviewDto,
  TourDto, VenueDto
} from '../../../../../modules/lpshows-api/v3';
import { FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { FloatLabel } from 'primeng/floatlabel';
import { Button } from 'primeng/button';
import { Divider } from 'primeng/divider';
import { Accordion, AccordionContent, AccordionHeader, AccordionPanel } from 'primeng/accordion';
import { InputText } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { LocationsService } from '../../../../../services/locations.service';
import { ToursService } from '../../../../../services/tours.service';
import { Tooltip } from 'primeng/tooltip';
import { DatePipe } from '@angular/common';
import { DateTime } from 'luxon';
import { InputGroup } from 'primeng/inputgroup';
import { InputGroupAddon } from 'primeng/inputgroupaddon';
import { SelectTimezoneComponent } from '../../locations/select-timezone/select-timezone.component';
import timezones from 'timezones-list';

@Component({
  imports: [
    FormsModule,
    ReactiveFormsModule,
    FloatLabel,
    Button,
    Divider,
    Accordion,
    AccordionPanel,
    AccordionHeader,
    AccordionContent,
    InputText,
    Tooltip,
    DatePipe,
    InputGroup,
    InputGroupAddon,
    SelectTimezoneComponent
  ],
  selector: 'app-import-concert-dialog-content',
  styleUrl: './import-concert-dialog-content.component.css',
  templateUrl: './import-concert-dialog-content.component.html',
})
export class ImportConcertDialogContentComponent implements OnInit {
  private messageService = inject(MessageService);
  private formBuilder = inject(FormBuilder);
  private toursService = inject(ToursService);
  private locationsService = inject(LocationsService);

  @Input("import-plan")
  importPlan = signal<ImportConcertPreviewDto | null>(null);

  @Output("applyClicked")
  applyClickedEvent: EventEmitter<ApplyClickedEvent> = new EventEmitter<ApplyClickedEvent>();

  timeZoneIsLoading$ = signal(false);

  private importPlanChangedEffect = effect(() => {
    let plan = this.importPlan();
    console.debug("Import plan changed:", plan);
    if (plan) {
      //this.importPlanForm.controls.concertTypeId.setValue(plan.concertType?.id ?? null);
      this.createTourForm.controls.tourName.setValue(plan.tourName ?? null);
      this.createTourLegForm.controls.legName.setValue(plan.tourLegName ?? null);
      this.createCountryForm.controls.name.setValue(plan.countryName ?? null);
      this.createCityForm.controls.name.setValue(plan.cityName ?? null);
      this.createCityForm.controls.nativeName.setValue(plan.cityNativeName ?? null);
      this.createVenueForm.controls.currentName.setValue(plan.venueName ?? null);
    }
  });

  createTourForm = this.formBuilder.group({
    tourId: new FormControl<string | null>(null, [Validators.required]),
    tourName: new FormControl<string | null>(null, [Validators.required]),
  });

  createTourLegForm = this.formBuilder.group({
    legId: new FormControl<string | null>(null, [Validators.required]),
    legName: new FormControl<string | null>(null, [Validators.required]),
  });

  createCountryForm = this.formBuilder.group({
    isoCode: new FormControl<string | null>(null, [Validators.required, Validators.pattern('^[A-Z]{3}$')]),
    name: new FormControl<string | null>(null, [Validators.required]),
    nativeName: new FormControl<string | null>(null, [Validators.required]),
  });

  createCityForm = this.formBuilder.group({
    name: new FormControl<string | null>(null, [Validators.required]),
    nativeName: new FormControl<string | null>(null, [Validators.required]),
  });

  createVenueForm = this.formBuilder.group({
    currentName: new FormControl<string | null>(null, [Validators.required]),
    timezone: new FormControl('', [Validators.required]),
  });


  ngOnInit() {
    // make sure to always convert the ISO code to uppercase
    this.createCountryForm.controls.isoCode.valueChanges.subscribe(value => {
      if (value?.match(/.*[a-z].*/)) {
        console.debug("Converting ISO code to uppercase:", value);
        this.createCountryForm.controls.isoCode.setValue(value.toUpperCase(), {emitEvent: false});
      }
    });
  }


  onCreateTourClicked() {
    console.debug("Create tour clicked");

    let tourName = this.createTourForm.controls.tourName.value;
    if (!tourName) {
      console.error("Tour name is required");
      this.messageService.add({severity: "error", summary: "Tour name is required", detail: "Please enter a tour name"});
      return;
    }

    let createTourRequest: CreateTourRequestDto = {
      id: this.createTourForm.controls.tourId.value ?? tourName.toLowerCase().replaceAll(" ", "-"),
      name: tourName
    };
    this.toursService.createTour(createTourRequest).subscribe({
      next: (createdTour) => {
        console.debug("Created tour:", createdTour);
        this.importPlan.update(prev => {
          if (prev) {
            prev.foundTours = [createdTour];
            console.debug("Updated tours:", prev.foundTours);
          }

          console.debug("importPlan.update() will return:", prev);
          return {
            ...prev,
            foundTours: [createdTour]
          };
        });
        console.debug("Updated import plan:", this.importPlan());
      },
      error: (err) => {
        console.error("Could not create tour:", err);
      }
    });
  }


  onCreateTourLegClicked() {
    console.debug("Create tour leg clicked");

    let tourLegName = this.createTourLegForm.controls.legName.value;
    if (!tourLegName) {
      console.error("Tour leg name is required");
      this.messageService.add({severity: "error", summary: "Tour leg name is required", detail: "Please enter a tour leg name"});
      return;
    }

    let createTourLegRequest: AddTourLegRequestDto = {
      id: this.createTourLegForm.controls.legId.value ?? tourLegName.toLowerCase().replaceAll(" ", "-"),
      name: tourLegName
    };
    this.toursService.createTourLeg(this.importPlan()?.foundTours?.at(0)?.id ?? "null", createTourLegRequest).subscribe({
      next: (createdTourLeg) => {
        console.debug("Created tour leg:", createdTourLeg);
        this.importPlan.update(prev => {
          if (prev) {
            prev.foundTourLegs = [createdTourLeg];
            console.debug("Updated tour legs:", prev.foundTourLegs);
          }

          console.debug("importPlan.update() will return:", prev);
          return {
            ...prev,
            foundTourLegs: [createdTourLeg]
          };
        });
        console.debug("Updated import plan:", this.importPlan());
      },
      error: (err) => {
        console.error("Could not create tour leg:", err);
      }
    });
  }

  onCreateCountryClicked() {
    console.debug("Create country clicked");

    let isoCode = this.createCountryForm.controls.isoCode.value;
    let name = this.createCountryForm.controls.name.value;
    let nativeName = this.createCountryForm.controls.nativeName.value;

    if (!isoCode || isoCode.length != 3) {
      console.error("ISO-code is required");
      this.messageService.add({severity: "error", summary: "ISO-code is required", detail: "Please enter a valid ISO-code"});
      return;
    }

    if (!name) {
      console.error("Name is required");
      this.messageService.add({severity: "error", summary: "Name is required", detail: "Please enter a valid name"});
      return;
    }

    if (!nativeName) {
      console.error("Native name is required");
      this.messageService.add({severity: "error", summary: "Native name is required", detail: "Please enter a valid name"});
      return;
    }

    let createCountryRequest: CreateCountryRequestDto = {
      isoCode: isoCode,
      name: name,
      nativeName: nativeName,
    };
    this.locationsService.createCountry(createCountryRequest).subscribe({
      next: (createdCountry) => {
        console.debug("Created country:", createdCountry);
        this.importPlan.update(prev => {
          if (prev) {
            prev.foundCountries = [createdCountry];
            console.debug("Updated countries:", prev.foundCountries);
          }

          console.debug("importPlan.update() will return:", prev);
          return {
            ...prev,
            foundCountries: [createdCountry]
          };
        });
        console.debug("Updated import plan:", this.importPlan());
      },
      error: (err) => {
        console.error("Could not create country:", err);
      }
    });
  }


  onCreateCityClicked() {
    console.debug("Create city clicked");

    let countryCode = this.importPlan()?.foundCountries?.at(0)?.isoCode ?? null;
    let name = this.createCityForm.controls.name.value;
    let nativeName = this.createCityForm.controls.nativeName.value;

    if (!countryCode || countryCode.length != 3) {
      console.error("Country is required");
      this.messageService.add({severity: "error", summary: "Country is required", detail: "Please create a country in the previous steps"});
      return;
    }

    if (!name) {
      console.error("Name is required");
      this.messageService.add({severity: "error", summary: "Name is required", detail: "Please enter a valid name"});
      return;
    }

    if (!nativeName) {
      console.error("Native name is required");
      this.messageService.add({severity: "error", summary: "Native name is required", detail: "Please enter a valid name"});
      return;
    }

    let createCityRequest: CreateCityRequestDto = {
      name: name,
      nativeName: nativeName,
    };
    this.locationsService.createCity(countryCode, createCityRequest).subscribe({
      next: (createdCity) => {
        console.debug("Created city:", createdCity);
        this.importPlan.update(prev => {
          if (prev) {
            prev.foundCities = [createdCity];
            console.debug("Updated cities:", prev.foundCities);
          }

          console.debug("importPlan.update() will return:", prev);
          return {
            ...prev,
            foundCities: [createdCity]
          };
        });
        console.debug("Updated import plan:", this.importPlan());
      },
      error: (err) => {
        console.error("Could not create city:", err);
      }
    });
  }


  onCreateVenueClicked() {
    console.debug("Create venue clicked");

    let countryCode = this.importPlan()?.foundCountries?.at(0)?.isoCode ?? null;
    let cityId = this.importPlan()?.foundCities?.at(0)?.id ?? null;
    let currentName = this.createVenueForm.controls.currentName.value;
    let timezone = this.createVenueForm.controls.timezone.value;

    if (!countryCode || countryCode.length != 3) {
      console.error("Country is required");
      this.messageService.add({severity: "error", summary: "Country is required", detail: "Please create a country in the previous steps"});
      return;
    }

    if (!currentName) {
      console.error("Name is required");
      this.messageService.add({severity: "error", summary: "Name is required", detail: "Please enter a valid name"});
      return;
    }

    if (!timezone) {
      console.error("Timezone is required");
      this.messageService.add({severity: "error", summary: "Timezone is required", detail: "Please select a timezone"});
      return;
    }

    let createVenueRequest: CreateVenueRequestDto = {
      countryCode: countryCode,
      cityId: cityId ?? 0,
      currentName: currentName,
      timeZoneId: timezone,
    };
    this.locationsService.createVenue(createVenueRequest).subscribe({
      next: (createdVenue) => {
        console.debug("Created venue:", createdVenue);
        this.importPlan.update(prev => {
          if (prev) {
            prev.foundVenues = [createdVenue];
            console.debug("Updated venues:", prev.foundCities);
          }

          console.debug("importPlan.update() will return:", prev);
          return {
            ...prev,
            foundVenues: [createdVenue]
          };
        });
        console.debug("Updated import plan:", this.importPlan());
      },
      error: (err) => {
        console.error("Could not create venue:", err);
      }
    });
  }


  protected generateId(nameControl: FormControl<string | null>, idControl: FormControl<string | null>) {
    let name = nameControl.getRawValue();
    let id = name?.toLowerCase().replaceAll(" ", "-") ?? null;
    if (id) {
      console.debug("Generated id:", id);
      idControl.setValue(id);
    }
  }

  protected onApplyClicked() {
    let plan = this.importPlan();
    if (!plan) {
      console.error("No import plan available");
      return;
    }

    let applyEvent: ApplyClickedEvent = {
      concertType: plan.concertType ?? null,
      tour: plan.foundTours?.at(0) ?? null,
      tourLeg: plan.foundTourLegs?.at(0) ?? null,
      postedStartTime: DateTime.fromISO(plan.postedStartTime ?? "").toJSDate() ?? null,
      venue: plan.foundVenues?.at(0) ?? null,
      customTitle: plan.proposedCustomTitle ?? null,
    };

    console.debug("prepared ApplyClickedEvent:", applyEvent);
    this.applyClickedEvent.emit(applyEvent);
  }

  protected onGetVenueTimezoneClicked() {
    this.timeZoneIsLoading$.set(true);

    let cityName = this.importPlan()?.foundCities?.at(0)?.name;
    let countryName = this.importPlan()?.foundCountries?.at(0)?.name;

    this.locationsService.getTimeZoneForCity(cityName!, null, countryName!)
      .subscribe(tzObj => {
        if (tzObj == null) {
          console.warn("No timezone found for: City: ", cityName, " Country: ", countryName);
          return;
        }

        console.log("Found timezone: ", tzObj);
        let tz = tzObj.timeZoneId!;
        this.timeZoneIsLoading$.set(false);

        if (timezones.map(t => t.tzCode).indexOf(tz, 0) >= 0) {
          this.createVenueForm.controls.timezone.setValue(tz);
        } else {
          console.error("Invalid timezone returned: ", tz);
          this.timeZoneIsLoading$.set(false);
          this.messageService.add({
            severity: "error",
            summary: "Could not load timezone",
            text: `Timezone '${tz}' found, but it is invalid.`,
          });
        }
      });
  }

  protected readonly DatePipe = DatePipe;
}

export class ApplyClickedEvent {
  concertType: ConcertTypeDto | null = null;
  tour: TourDto | null = null;
  tourLeg: TourDto | null = null;
  postedStartTime: Date | null = null;
  venue: VenueDto | null = null;
  customTitle: string | null = null;
}
