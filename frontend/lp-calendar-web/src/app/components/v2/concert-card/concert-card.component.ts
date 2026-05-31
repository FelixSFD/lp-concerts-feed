import { Component } from '@angular/core';
import {Button} from 'primeng/button';
import {Card} from 'primeng/card';

@Component({
  selector: 'app-concert-card',
  imports: [
    Button,
    Card
  ],
  templateUrl: './concert-card.component.html',
  styleUrl: './concert-card.component.css',
})
export class ConcertCardComponent {

}
