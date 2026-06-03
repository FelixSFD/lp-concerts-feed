import {Component, EventEmitter, inject, Input, OnInit, Output} from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule} from "@angular/forms";

import {ConcertFilter} from '../../../data/concert-filter';
import {DateTime} from 'luxon';
import {NgbTooltip} from '@ng-bootstrap/ng-bootstrap';
import {listOfTours, tourConfigs} from '../../../app.config';
import {Button} from 'primeng/button';
import {Drawer} from 'primeng/drawer';
import {Select} from 'primeng/select';
import {DatePicker} from 'primeng/datepicker';
import {TourConfig} from '../../../data/tour-config';
import {Card} from 'primeng/card';

@Component({
  selector: 'app-concert-filter',
  imports: [
    ReactiveFormsModule,
    NgbTooltip,
    Button,
    Drawer,
    Select,
    FormsModule,
    DatePicker,
    Card
  ],
  templateUrl: './concert-filter.component.html',
  styleUrl: './concert-filter.component.css'
})
export class ConcertFilterComponent implements OnInit {
  private formBuilder = inject(FormBuilder);

  filterForm = this.formBuilder.group({
    tourName: new FormControl<TourConfig | null>(null, []),
    showPastConcerts: new FormControl(false, []),
    dateFrom: new FormControl<Date | undefined>(undefined),
    dateTo: new FormControl<Date | undefined>(undefined)
  });

  @Output('applyClicked')
  applyClicked = new EventEmitter<ConcertFilter>();


  @Input('defaultFilter')
  defaultFilter: ConcertFilter | undefined;

  // properties to control which fields were added
  availableFilters: string[] = ["tour", "dateRange"];
  filterLabels$: Map<string, string> = new Map<string, string>();

  visible$: boolean = false;

  availableTours$: TourConfig[] = tourConfigs;

  constructor() {
    this.filterLabels$.set('tour', 'Tour name');
    this.filterLabels$.set('dateRange', 'Concert date');

    this.availableTours$.push({ label: "Not part of a tour", value: ""})
  }


  ngOnInit() {
    this.setDefaultFilters();

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


  onClearClicked() {
    this.setDefaultFilters();
    this.applyClicked.emit(this.defaultFilter);
  }


  onFiltersChanged() {
    console.debug("Filters changed");
    let filter = this.readFilterFromForm();
    this.applyClicked.emit(filter);
  }


  showOrHideFilter(filterName: string, show: boolean) {
    this.onApplyClicked();
  }


  private setDefaultFilters() {
    console.debug("setDefaultFilters: ", this.defaultFilter);
    this.filterForm.controls.showPastConcerts.setValue(!this.defaultFilter?.onlyFuture);
    this.filterForm.controls.tourName.setValue(this.availableTours$.find(t => t.value == this.defaultFilter?.tour) ?? null);
  }


  private readFilterFromForm() {
    let concertFilter = new ConcertFilter();
    concertFilter.tour = (this.filterForm.value.tourName?.valueOf() as TourConfig)?.value ?? null;

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
