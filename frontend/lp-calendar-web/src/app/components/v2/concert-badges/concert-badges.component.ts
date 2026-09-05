import {Component, Input} from '@angular/core';

import {ConcertDto, ConcertStatusValueDto} from '../../../modules/lpshows-api';
import {defaultShowType} from '../../../app.config';
import {Tag} from 'primeng/tag';
import {Tooltip} from 'primeng/tooltip';
import {ConcertBadgesData} from '../concert-details/concert-details.view-model';

@Component({
  selector: 'app-concert-badges',
  imports: [
    Tag,
    Tooltip
  ],
  templateUrl: './concert-badges.component.html',
  styleUrl: './concert-badges.component.css'
})
export class ConcertBadgesComponent {
  @Input("concert")
  concert$: ConcertBadgesData | ConcertDto | null = null;

  protected readonly ConcertStatusValueDto = ConcertStatusValueDto;
  protected readonly defaultShowType = defaultShowType;
}
