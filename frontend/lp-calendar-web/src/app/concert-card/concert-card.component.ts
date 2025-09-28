import {Component, Input, OnInit} from '@angular/core';
import {CountdownComponent} from '../countdown/countdown.component';
import {ConcertsService} from '../services/concerts.service';
import {NgIf} from '@angular/common';
import {RouterLink} from '@angular/router';
import {ConcertTitleGenerator} from '../data/concert-title-generator';
import {ConcertDto} from '../modules/lpshows-api';

@Component({
  selector: 'app-concert-card',
  imports: [
    CountdownComponent,
    NgIf,
    RouterLink
  ],
  templateUrl: './concert-card.component.html',
  styleUrl: './concert-card.component.css'
})
export class ConcertCardComponent implements OnInit{
  @Input("cardTitle")
  cardTitle: string = "Concert";

  @Input("concert")
  concert$: ConcertDto | null = null;

  @Input("isLoading")
  isLoading$: boolean = true;

  @Input("notFoundAlertClass")
  notFoundAlertClass: string = "info"

  @Input("notFoundAlertText")
  notFoundAlertText: string = "Concert was not found";

  constructor() {
  }


  ngOnInit(): void {
  }

  protected readonly ConcertTitleGenerator = ConcertTitleGenerator;
}
