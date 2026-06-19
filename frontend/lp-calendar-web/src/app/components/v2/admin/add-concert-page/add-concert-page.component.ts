import {Component, inject} from '@angular/core';
import {ConcertFormComponent} from '../concert-form/concert-form.component';
import {ConcertsService} from '../../../../services/concerts.service';
import {ConcertDto} from '../../../../modules/lpshows-api';
import {Button} from 'primeng/button';
import {ButtonGroup} from 'primeng/buttongroup';
import {Card} from 'primeng/card';
import {MessageService} from 'primeng/api';

@Component({
  selector: 'app-add-concert-page',
  imports: [
    ConcertFormComponent,
    Card
  ],
  templateUrl: './add-concert-page.component.html',
  styleUrl: './add-concert-page.component.css'
})
export class AddConcertPageComponent {
  private messageService = inject(MessageService);

  isSaving$: boolean = false;

  constructor(private concertsService: ConcertsService) {
  }


  onFormSaved(concert: ConcertDto) {
    this.isSaving$ = true;

    this.concertsService.addConcert(concert).subscribe({
      next: result => {
        console.log("Add concert request finished");
        console.log(result);

        this.messageService.add({
          severity: "success",
          summary: "Saved concert",
        });
        this.isSaving$ = false;
      },
      error: err => {
        this.messageService.add({
          severity: "danger",
          summary: "Failed to add concert",
          text: err.error?.message
        });
        this.isSaving$ = false;
      }
    });
  }
}
