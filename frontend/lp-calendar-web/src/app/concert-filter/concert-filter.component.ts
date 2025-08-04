import {Component, EventEmitter, inject, Input, OnInit, Output} from '@angular/core';
import {FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {NgForOf, NgIf} from '@angular/common';
import {listOfTours} from '../app.config';
import {Concert} from '../data/concert';
import {ConcertFilter} from '../data/concert-filter';
import {DateTime} from 'luxon';

@Component({
  selector: 'app-concert-filter',
  imports: [
    ReactiveFormsModule,
    NgForOf,
    NgIf
  ],
  templateUrl: './concert-filter.component.html',
  styleUrl: './concert-filter.component.css'
})
export class ConcertFilterComponent implements OnInit {
  private formBuilder = inject(FormBuilder);

  filterForm = this.formBuilder.group({
    tourName: new FormControl('', []),
    showPastConcerts: new FormControl(false, []),
  });

  @Output('applyClicked')
  applyClicked = new EventEmitter<ConcertFilter>();


  @Input('defaultFilter')
  defaultFilter: ConcertFilter | undefined;

  // properties to control which fields were added
  showFilterTour$ = true;
  showFilterPastConcerts$ = false;
  showFilterDateRange$ = false;
  availableFilters: string[] = ["tour", "pastConcerts", "dateRange"];
  numberOfActivatedFilters$ = 0;


  ngOnInit() {
    this.setDefaultFilters();
  }


  onApplyClicked() {
    let filter = this.readFilterFromForm();
    this.applyClicked.emit(filter);
  }


  onClearClicked() {
    this.setDefaultFilters();
    this.applyClicked.emit(this.defaultFilter);
  }


  showOrHideFilter(filterName: string, show: boolean) {
    switch (filterName) {
      case 'tour':
        this.showFilterTour$ = show;
        this.numberOfActivatedFilters$ += show ? 1 : -1;
        break;
      case 'pastConcerts':
        this.showFilterPastConcerts$ = show;
        this.numberOfActivatedFilters$ += show ? 1 : -1;
        break;
      case 'dateRange':
        this.showFilterDateRange$ = show;
        this.numberOfActivatedFilters$ += show ? 1 : -1;
        break;
      default:
          console.error("Could not find filter: ", filterName);
    }
  }


  private setDefaultFilters() {
    console.debug("setDefaultFilters: ", this.defaultFilter);
    this.filterForm.controls.showPastConcerts.setValue(!this.defaultFilter?.onlyFuture);
    this.filterForm.controls.tourName.setValue(this.defaultFilter?.tour ?? null);
  }


  private readFilterFromForm() {
    let concertFilter = new ConcertFilter();
    concertFilter.tour = this.filterForm.value.tourName?.valueOf();
    concertFilter.onlyFuture = !this.filterForm.value.showPastConcerts?.valueOf()

    console.debug("Filter: ", concertFilter);

    return concertFilter;
  }


  protected readonly listOfTours = listOfTours;
}
