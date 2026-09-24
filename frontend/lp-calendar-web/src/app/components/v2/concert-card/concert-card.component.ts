import {
  ChangeDetectionStrategy,
  Component,
  inject,
  Input,
  OnChanges,
  OnInit,
  signal,
  SimpleChanges
} from '@angular/core';
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
import { MessageSeverity } from 'primeng/types/message';
import { ConcertDetailsDto, VenueDto } from '../../../modules/lpshows-api/v3';

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
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class ConcertCardComponent implements OnInit, OnChanges {
  private authService = inject(AuthService);

  @Input("cardTitle")
  cardTitle: string = "Concert";

  @Input("concert")
  concert$: ConcertDto | null = null;

  @Input("concert-details")
  concert2$: ConcertDetailsDto | null = null;

  protected viewModel = signal<ConcertCardViewModel | null>(null);

  @Input("isLoading")
  isLoading$: boolean = false;

  @Input("notFoundAlertClass")
  notFoundAlertClass: MessageSeverity = "info"

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

  ngOnChanges(changes: SimpleChanges<ConcertCardComponent>) {
    let current = changes.concert2$?.currentValue ?? changes.concert$?.currentValue;
    if (current == null) {
      this.viewModel.set(null);
    } else if ("timeIsPlaceholder" in current) {
      let currentV3 = current as ConcertDetailsDto;
      console.debug("currentV3", currentV3);
      this.viewModel.set({
        id: currentV3.id!,
        isPast: false,
        venue: currentV3.venue.currentName,
        location: `${currentV3.venue.city.name}${(currentV3.venue.city.state?.name.length ?? 0) > 0 ? ", " + currentV3.venue.city.state?.name : ""}, ${currentV3.venue.city.country.name}`,
        postedStartTime: currentV3.postedStartTime,
        mainStageTime: currentV3.mainStageTime
      });
    } else if ("isPast" in current) {
      let currentV1 = current as ConcertDto;
      console.debug("currentV1", currentV1);
      this.viewModel.set({
        id: currentV1.id!,
        isPast: currentV1.isPast ?? false,
        venue: currentV1.venue!,
        location: `${currentV1.city}${(currentV1.state?.length ?? 0) > 0 ? ", " + currentV1.state : ""}, ${currentV1.country}`,
        postedStartTime: current.postedStartTime,
        mainStageTime: current.mainStageTime
      });
    }
  }

  protected readonly DateTime = DateTime;
}


export class ConcertCardViewModel {
  id!: string;
  location!: string;
  venue!: string;
  isPast!: boolean;
  postedStartTime?: string | undefined;
  mainStageTime?: string | undefined;
}
