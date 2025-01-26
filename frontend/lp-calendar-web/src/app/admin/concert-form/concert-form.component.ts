import {Component, Inject, inject, Input, OnInit} from '@angular/core';
import timezones from 'timezones-list';
import {listOfTours} from '../../app.config';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {NgClass, NgForOf, NgIf} from '@angular/common';
import {Concert} from '../../data/concert';
import {ConcertsService} from '../../services/concerts.service';

// This class represents a form for adding and editing concerts
@Component({
  selector: 'app-concert-form',
  imports: [
    FormsModule,
    NgForOf,
    NgIf,
    ReactiveFormsModule,
    NgClass
  ],
  templateUrl: './concert-form.component.html',
  styleUrl: './concert-form.component.css'
})
export class ConcertFormComponent implements OnInit {
  private formBuilder = inject(FormBuilder);
  private concertsService = inject(ConcertsService);

  concertForm = this.formBuilder.group({
    tourName: new FormControl('', []),
    venue: new FormControl('', [Validators.min(3), Validators.required]),
    timezone: new FormControl('', [Validators.required]),
    city: new FormControl('', [Validators.required]),
    state: new FormControl('', []),
    country: new FormControl('', [Validators.required]),
    postedStartTime: new FormControl('', [Validators.required])
  });

  @Input({ alias: "concert-id" })
  concertId: string | null = null;

  concert$ : Concert | null = null;

  ngOnInit() {
    // Fetch concert data from API to prefill the form
    if (this.concertId != null) {
      this.concertsService.getConcert(this.concertId).subscribe(c => {
        this.concert$ = c;

        this.fillFormWithConcert(c);
      });
    }
  }


  private fillFormWithConcert(concert: Concert) {
    this.concertForm.controls.tourName.setValue(concert.tourName ?? null);
    this.concertForm.controls.venue.setValue(concert.venue ?? null);
    this.concertForm.controls.city.setValue(concert.city ?? null);
    this.concertForm.controls.state.setValue(concert.state ?? null);
    this.concertForm.controls.country.setValue(concert.country ?? null);
    this.concertForm.controls.postedStartTime.setValue(concert.postedStartTime ?? null);
    this.concertForm.controls.timezone.setValue(concert.timeZoneId ?? null);
  }


  protected readonly timezones = timezones;
  protected readonly listOfTours = listOfTours;
}
