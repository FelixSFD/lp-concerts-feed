import { Component } from '@angular/core';
import {ConcertFormComponent} from '../concert-form/concert-form.component';
import {ActivatedRoute} from '@angular/router';

@Component({
  selector: 'app-edit-concert-page',
  imports: [
    ConcertFormComponent
  ],
  templateUrl: './edit-concert-page.component.html',
  styleUrl: './edit-concert-page.component.css'
})
export class EditConcertPageComponent {
  concertId: string | null;

  constructor(private route: ActivatedRoute) {
    this.concertId = this.route.snapshot.paramMap.get('id');
  }
}
