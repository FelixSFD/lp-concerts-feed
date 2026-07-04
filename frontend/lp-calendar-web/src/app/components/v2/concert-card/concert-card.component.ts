import {Component, inject, Input, OnInit} from '@angular/core';
import {Button} from 'primeng/button';
import {Card} from 'primeng/card';
import {AuthService} from '../../../auth/auth.service';
import {ConcertDto} from '../../../modules/lpshows-api';
import {Skeleton} from 'primeng/skeleton';
import {CountdownComponent} from '../countdown/countdown.component';
import {RouterLink} from '@angular/router';
import {DatePipe} from '@angular/common';
import {DateTime} from 'luxon';
import {Tooltip} from 'primeng/tooltip';
import {Message} from 'primeng/message';

@Component({
  selector: 'app-concert-card',
  imports: [
    Button,
    Card,
    Skeleton,
    CountdownComponent,
    RouterLink,
    CountdownComponent,
    DatePipe,
    Tooltip,
    Message
  ],
  templateUrl: './concert-card.component.html',
  styleUrl: './concert-card.component.css',
})
export class ConcertCardComponent implements OnInit {
  private authService = inject(AuthService);

  @Input("cardTitle")
  cardTitle: string = "Concert";

  @Input("concert")
  concert$: ConcertDto | null = null;

  @Input("isLoading")
  isLoading$: boolean = true;

  @Input("notFoundAlertClass")
  notFoundAlertClass: "info" | "error" | "success" | "warn" | "secondary" | "contrast" | null | undefined = "info"

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

  protected readonly DateTime = DateTime;
}
