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
  // how the concert is displayed. "countdown" is default
  displayType: string = "countdown";

  @Input("concert")
  concert$: ConcertDto | null = null;

  // if no concert is set, use distant past as placeholder for countdown
  pastPlaceholderDate = new Date(2024, 9, 5, 15, 0).toISOString();


  constructor(private concertsService: ConcertsService) {
  }


  ngOnInit(): void {
  }

  protected readonly ConcertTitleGenerator = ConcertTitleGenerator;
}
