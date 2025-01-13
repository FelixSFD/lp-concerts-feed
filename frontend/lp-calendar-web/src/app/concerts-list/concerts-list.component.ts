import { Component } from '@angular/core';
import {ConcertsService} from '../services/concerts.service';
import {AsyncPipe, DatePipe, JsonPipe, NgForOf} from '@angular/common';
import {Concert} from '../data/concert';

@Component({
  selector: 'app-concerts-list',
  imports: [
    AsyncPipe,
    JsonPipe,
    DatePipe,
    NgForOf
  ],
  templateUrl: './concerts-list.component.html',
  styleUrl: './concerts-list.component.css'
})
export class ConcertsListComponent {
  concerts$: Concert[] = [];

  constructor(private concertsService: ConcertsService) {
    concertsService.getConcerts().subscribe(result => {
      this.concerts$ = result;
    })
  }

}
