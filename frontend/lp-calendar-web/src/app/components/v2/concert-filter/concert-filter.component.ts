import {Component, EventEmitter, inject, Input, OnInit, Output} from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule} from "@angular/forms";

import {ConcertFilter} from '../../../data/concert-filter';
import {DateTime} from 'luxon';
import {listOfTours, tourConfigs} from '../../../app.config';
import {Button} from 'primeng/button';
import {Drawer} from 'primeng/drawer';
import {DatePicker} from 'primeng/datepicker';
import {TourConfig} from '../../../data/tour-config';
import {Card} from 'primeng/card';
import {ActivatedRoute, NavigationExtras, Router} from '@angular/router';
import {isValidDate} from 'rxjs/internal/util/isDate';
import { SelectTourComponent } from '../admin/concerts/select-tour/select-tour.component';
import { TourDto } from '../../../modules/lpshows-api/v3';

@Component({
  selector: 'app-concert-filter',
  imports: [
    ReactiveFormsModule,
    Button,
    Drawer,
    FormsModule,
    DatePicker,
    Card,
    SelectTourComponent
  ],
  templateUrl: './concert-filter.component.html',
  styleUrl: './concert-filter.component.css'
})
export class ConcertFilterComponent implements OnInit {
  private formBuilder = inject(FormBuilder);
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);

  filterForm = this.formBuilder.group({
    tour: new FormControl<TourDto | null>(null, []),
    showPastConcerts: new FormControl(false, []),
    dateFrom: new FormControl<Date | undefined>(undefined),
    dateTo: new FormControl<Date | undefined>(undefined)
  });

  @Output('applyClicked')
  applyClicked = new EventEmitter<ConcertFilter>();


  @Input('defaultFilter')
  defaultFilter: ConcertFilter | undefined;

  visible$: boolean = false;

  activeFilterCount$ = 0;

  constructor() {
  }


  ngOnInit() {
    this.setDefaultFilters();
    const queryParams = this.activatedRoute.snapshot.queryParams;
    console.debug("Query params: ", queryParams);
    if (queryParams["tourId"] !== undefined) {
      console.debug("Query includes tour ID: ", queryParams["tourId"]);
      this.filterForm.controls.tour.setValue(queryParams["tourId"] ?? null);
    }

    let dateFrom = DateTime.fromISO(queryParams["dateFrom"]).toJSDate();
    if (isValidDate(dateFrom)) {
      console.debug("Query includes dateFrom: ", dateFrom);
      this.filterForm.controls.dateFrom.setValue(dateFrom ?? null);
    }

    let dateTo = DateTime.fromISO(queryParams["dateTo"]).toJSDate();
    if (isValidDate(dateTo)) {
      console.debug("Query includes dateTo: ", dateTo);
      this.filterForm.controls.dateTo.setValue(dateTo ?? null);
    }

    // send first filter event
    this.onFiltersChanged();

    this.filterForm.controls.dateFrom.valueChanges.subscribe({
      next: value => {
        this.onFiltersChanged();
      }
    });

    this.filterForm.controls.dateTo.valueChanges.subscribe({
      next: value => {
        this.onFiltersChanged();
      }
    });
  }


  onApplyClicked() {
    this.onFiltersChanged();
    this.visible$ = false;
  }


  onFiltersChanged() {
    console.debug("Filters changed");
    let filter = this.readFilterFromForm();
    this.applyClicked.emit(filter);
    this.calculateActiveFilterCount();
    this.setFiltersToUrl(filter);
  }

  onClearButtonClicked() {
    this.setDefaultFilters();
    this.onFiltersChanged();
  }


  private setFiltersToUrl(filter: ConcertFilter) {
    const extras: NavigationExtras = {
      relativeTo: this.activatedRoute,
      queryParams: {
        dateFrom: filter.dateFrom?.toISODate() ?? null,
        dateTo: filter.dateTo?.toISODate() ?? null,
        tourId: filter.tour?.id ?? null,
      },
      queryParamsHandling: 'merge'
    };

    this.router.navigate([], extras).then(() => { /* continue here */ });
  }


  private calculateActiveFilterCount() {
    let count = 0;
    if (this.filterForm.value.tour != this.defaultFilter?.tour) {
      console.debug("Tour filter is active");
      count++;
    }

    if (this.filterForm.value.dateFrom != this.defaultFilter?.dateFrom) {
      console.debug("dateFrom filter is active");
      count++;
    }

    if (this.filterForm.value.dateTo != this.defaultFilter?.dateTo) {
      console.debug("dateTo filter is active");
      count++;
    }

    this.activeFilterCount$ = count;
  }


  private setDefaultFilters() {
    console.debug("setDefaultFilters: ", this.defaultFilter);
    this.filterForm.controls.showPastConcerts.setValue(!this.defaultFilter?.onlyFuture);
    this.filterForm.controls.tour.setValue(this.defaultFilter?.tour ?? null);
    this.filterForm.controls.dateFrom.setValue(this.defaultFilter?.dateFrom?.toJSDate() ?? null);
    this.filterForm.controls.dateTo.setValue(this.defaultFilter?.dateTo?.toJSDate() ?? null);
  }


  private readFilterFromForm() {
    let concertFilter = new ConcertFilter();
    concertFilter.orderBy = this.defaultFilter?.orderBy;
    concertFilter.tour = this.filterForm.value.tour ?? null;

    let filterDateFrom = this.filterForm.value.dateFrom;
    if (filterDateFrom != null) {
      concertFilter.dateFrom = DateTime.fromJSDate(filterDateFrom);
    }

    let filterDateTo = this.filterForm.value.dateTo;
    if (filterDateTo != null) {
      let dateTo = DateTime.fromJSDate(filterDateTo);
      // make sure to search for the whole day
      concertFilter.dateTo = dateTo.set({hour: 23, minute: 59, second: 59});
    }

    // if dates are set, include past shows as well
    concertFilter.onlyFuture = !(concertFilter.dateFrom?.isValid || concertFilter.dateTo?.isValid);

    console.debug("Filter: ", concertFilter);

    return concertFilter;
  }

  protected readonly listOfTours = listOfTours;
  protected readonly String = String;
}
