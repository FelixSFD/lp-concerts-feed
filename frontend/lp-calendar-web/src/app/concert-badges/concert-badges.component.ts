import {Component, Input} from '@angular/core';
import {NgIf} from "@angular/common";
import {Concert} from '../data/concert';
import {defaultShowType} from '../app.config';
import {NgbTooltip} from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-concert-badges',
  imports: [
    NgIf,
    NgbTooltip
  ],
  templateUrl: './concert-badges.component.html',
  styleUrl: './concert-badges.component.css'
})
export class ConcertBadgesComponent {
  @Input("concert")
  concert$: Concert | null = null;
  protected readonly defaultShowType = defaultShowType;
}
