import {Component, Input, OnInit} from '@angular/core';
import {RouterLink} from '@angular/router';
import {environment} from '../../environments/environment';
import {ConcertCardComponent} from '../concert-card/concert-card.component';
import {ConcertsService} from '../services/concerts.service';
import {Concert} from '../data/concert';

@Component({
  selector: 'app-home',
  imports: [
    RouterLink,
    ConcertCardComponent
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {

  protected readonly environment = environment;

  nextConcert: Concert | null = null;


  constructor(private concertsService: ConcertsService) {
  }


  ngOnInit() {
    this.concertsService.getNextConcert().subscribe(result => this.nextConcert = result);
  }
}
