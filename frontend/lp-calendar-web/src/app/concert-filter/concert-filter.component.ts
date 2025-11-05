import {Component, EventEmitter, inject, Input, OnInit, Output} from '@angular/core';
import {FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {NgForOf, NgIf} from '@angular/common';
import {listOfTours} from '../app.config';
import {ConcertFilter} from '../data/concert-filter';
import {DateTime} from 'luxon';
import {NgbTooltip} from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-concert-filter',
  imports: [
    ReactiveFormsModule,
    NgForOf,
    NgIf,
    NgbTooltip
  ],
  templateUrl: './concert-filter.component.html',
  styleUrl: './concert-filter.component.css'
})
export class ConcertFilterComponent implements OnInit {
  private formBuilder = inject(FormBuilder);

  filterForm = this.formBuilder.group({
    tourName: new FormControl('', []),
    showPastConcerts: new FormControl(false, []),
    dateFrom: new FormControl(''),
    dateTo: new FormControl('')
  });

  @Output('applyClicked')
  applyClicked = new EventEmitter<ConcertFilter>();


  @Input('defaultFilter')
  defaultFilter: ConcertFilter | undefined;

  // properties to control which fields were added
  availableFilters: string[] = ["tour", "dateRange"];
  visibleFilters$: string[] = [];
  filterLabels$: Map<string, string> = new Map<string, string>();

  constructor() {
    this.filterLabels$.set('tour', 'Tour name');
    this.filterLabels$.set('dateRange', 'Concert date');
  }


  ngOnInit() {
    this.setDefaultFilters();
  }


  onApplyClicked() {
    this.onFiltersChanged();
  }


  onClearClicked() {
    this.setDefaultFilters();
    this.applyClicked.emit(this.defaultFilter);
  }


  onFiltersChanged() {
    let filter = this.readFilterFromForm();
    this.applyClicked.emit(filter);
  }


  showOrHideFilter(filterName: string, show: boolean) {
    if (show) {
      // add filter to list of visible
      this.visibleFilters$.push(filterName);
    } else {
      // remove filter from list of visible
      this.visibleFilters$ = this.visibleFilters$.filter(e => e !== filterName);
      this.onApplyClicked();
    }
  }


  private setDefaultFilters() {
    console.debug("setDefaultFilters: ", this.defaultFilter);
    this.filterForm.controls.showPastConcerts.setValue(!this.defaultFilter?.onlyFuture);
    this.filterForm.controls.tourName.setValue(this.defaultFilter?.tour ?? null);
    this.visibleFilters$ = [];
  }


  private readFilterFromForm() {
    let concertFilter = new ConcertFilter();
    concertFilter.tour = this.visibleFilters$.includes("tour") ? this.filterForm.value.tourName?.valueOf() : null;

    let filterDateFrom = this.visibleFilters$.includes("dateRange") ? this.filterForm.value.dateFrom : null;
    if (filterDateFrom != null) {
      concertFilter.dateFrom = DateTime.fromISO(filterDateFrom);
    }

    let filterDateTo = this.visibleFilters$.includes("dateRange") ? this.filterForm.value.dateTo : null;
    if (filterDateTo != null) {
      let dateTo = DateTime.fromISO(filterDateTo);
      // make sure to search for the whole day
      concertFilter.dateTo = dateTo.set({hour: 23, minute: 59, second: 59});
    }

    // if dates are set, include past shows as well
    concertFilter.onlyFuture = !(concertFilter.dateFrom?.isValid || concertFilter.dateTo?.isValid);

    console.debug("Filter: ", concertFilter);

    return concertFilter;
  }


  protected readonly listOfTours = listOfTours;
}
