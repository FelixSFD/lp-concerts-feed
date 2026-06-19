import { Component } from '@angular/core';
import {ConcertFormComponent} from '../concert-form/concert-form.component';
import {ConcertsService} from '../../../../services/concerts.service';
import {ToastrService} from 'ngx-toastr';
import {ConcertDto} from '../../../../modules/lpshows-api';
import {Button} from 'primeng/button';
import {ButtonGroup} from 'primeng/buttongroup';
import {Card} from 'primeng/card';

@Component({
  selector: 'app-add-concert-page',
  imports: [
    ConcertFormComponent,
    Button,
    ButtonGroup,
    Card
  ],
  templateUrl: './add-concert-page.component.html',
  styleUrl: './add-concert-page.component.css'
})
export class AddConcertPageComponent {
  isSaving$: boolean = false;

  constructor(private concertsService: ConcertsService, private toastr: ToastrService) {
  }


  onFormSaved(concert: ConcertDto) {
    this.isSaving$ = true;

    this.concertsService.addConcert(concert).subscribe({
      next: result => {
        console.log("Add concert request finished");
        console.log(result);

        this.toastr.success("Saved concert!");
        this.isSaving$ = false;
      },
      error: err => {
        this.toastr.error(err.error?.message, "Failed to add concert!");
        this.isSaving$ = false;
      }
    });
  }
}
