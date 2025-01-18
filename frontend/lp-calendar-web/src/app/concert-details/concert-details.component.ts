import {Component, OnInit} from '@angular/core';
import {ConcertsService} from '../services/concerts.service';
import {Concert} from '../data/concert';
import { ActivatedRoute } from '@angular/router';
import {DateTime} from 'luxon';
import {NgIf} from '@angular/common';
import {CountdownComponent} from '../countdown/countdown.component';

@Component({
  selector: 'app-concert-details',
  imports: [
    NgIf,
    CountdownComponent
  ],
  templateUrl: './concert-details.component.html',
  styleUrl: './concert-details.component.css'
})
export class ConcertDetailsComponent implements OnInit {
  concert$: Concert | null = null;
  concertId: string | undefined;

  constructor(private route: ActivatedRoute, private concertsService: ConcertsService) {
  }

  ngOnInit(): void {
    this.concertId = this.route.snapshot.paramMap.get('id')!;

    this.concertsService
      .getConcert(this.concertId)
      .subscribe(result => this.concert$ = result);
  }


  public getDateTime(inputDate: string) {
    return DateTime.fromISO(inputDate, {setZone: false});
  }


  public getDateTimeInTimezone(inputDate: string) {
    return DateTime.fromISO(inputDate, {setZone: true});
  }


  protected readonly DateTime = DateTime;
}
