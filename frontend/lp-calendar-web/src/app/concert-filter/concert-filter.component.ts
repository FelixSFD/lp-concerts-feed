import {Component, EventEmitter, inject, Output} from '@angular/core';
import {FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {NgForOf} from '@angular/common';
import {listOfTours} from '../app.config';
import {Concert} from '../data/concert';
import {ConcertFilter} from '../data/concert-filter';
import {DateTime} from 'luxon';

@Component({
  selector: 'app-concert-filter',
  imports: [
    ReactiveFormsModule,
    NgForOf
  ],
  templateUrl: './concert-filter.component.html',
  styleUrl: './concert-filter.component.css'
})
export class ConcertFilterComponent {
  private formBuilder = inject(FormBuilder);

  filterForm = this.formBuilder.group({
    tourName: new FormControl('', [])
  });

  @Output('applyClicked')
  applyClicked = new EventEmitter<ConcertFilter>();


  onApplyClicked() {
    let filter = this.readFilterFromForm();
    this.applyClicked.emit(filter);
  }


  private readFilterFromForm() {
    let concertFilter = new ConcertFilter();
    concertFilter.tour = this.filterForm.value.tourName?.valueOf();

    console.debug("Filter: ", concertFilter);

    return concertFilter;
  }


  protected readonly listOfTours = listOfTours;
}
