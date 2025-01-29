import {Component, Input} from '@angular/core';
import {NgIf} from "@angular/common";
import {Concert} from '../data/concert';

@Component({
  selector: 'app-concert-badges',
    imports: [
        NgIf
    ],
  templateUrl: './concert-badges.component.html',
  styleUrl: './concert-badges.component.css'
})
export class ConcertBadgesComponent {
  @Input("concert")
  concert$: Concert | null = null;
}
