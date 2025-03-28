import { Component } from '@angular/core';
import {ConcertFormComponent} from '../concert-form/concert-form.component';
import {ActivatedRoute} from '@angular/router';
import {Concert} from '../../data/concert';
import {ConcertsService} from '../../services/concerts.service';
import {ToastrService} from 'ngx-toastr';

@Component({
  selector: 'app-add-concert-page',
  imports: [
    ConcertFormComponent
  ],
  templateUrl: './add-concert-page.component.html',
  styleUrl: './add-concert-page.component.css'
})
export class AddConcertPageComponent {
  isSaving$: boolean = false;

  constructor(private concertsService: ConcertsService, private toastr: ToastrService) {
  }


  onFormSaved(concert: Concert) {
    this.isSaving$ = true;

    this.concertsService.addConcert(concert).subscribe(result => {
      console.log("Add concert request finished");
      console.log(result);

      this.toastr.success("Saved concert!");
      this.isSaving$ = false;
    });
  }
}
