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
