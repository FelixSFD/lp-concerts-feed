import {Component, inject, Input, OnInit} from '@angular/core';
import {CountdownComponent} from '../countdown/countdown.component';
import {ConcertsService} from '../services/concerts.service';

import {RouterLink} from '@angular/router';
import {ConcertTitleGenerator} from '../data/concert-title-generator';
import {ConcertDto} from '../modules/lpshows-api';
import {DateTime} from 'luxon';
import {AuthService} from '../auth/auth.service';

@Component({
  selector: 'app-concert-card',
  imports: [
    CountdownComponent,
    RouterLink
],
  templateUrl: './concert-card.component.html',
  styleUrl: './concert-card.component.css'
})
export class ConcertCardComponent implements OnInit{
  private authService = inject(AuthService);

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

  canUpdateConcerts$ = false;

  constructor() {
  }


  ngOnInit(): void {
    this.authService.canUpdateConcerts.subscribe(hasPermission => {
      this.canUpdateConcerts$ = hasPermission;
    });
  }

  protected readonly ConcertTitleGenerator = ConcertTitleGenerator;
  protected readonly DateTime = DateTime;
}
