import {Component, inject} from '@angular/core';
import {ConcertFormComponent} from '../concert-form/concert-form.component';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {ConcertsService} from '../../../../services/concerts.service';
import {HttpErrorResponse} from "@angular/common/http";
import {AdjacentConcertsResponseDto, ConcertDto, ErrorResponseDto} from '../../../../modules/lpshows-api';
import {Card} from 'primeng/card';
import {Button} from 'primeng/button';
import {ButtonGroup} from 'primeng/buttongroup';
import {MessageService} from 'primeng/api';

@Component({
  selector: 'app-edit-concert-page',
  imports: [
    ConcertFormComponent,
    RouterLink,
    Card,
    Button,
    ButtonGroup
  ],
  templateUrl: './edit-concert-page.component.html',
  styleUrl: './edit-concert-page.component.css'
})
export class EditConcertPageComponent {
  private messageService = inject(MessageService);

  concertId: string | null;
  adjacentConcertData$: AdjacentConcertsResponseDto | null = null;

  isSaving$: boolean = false;

  constructor(private route: ActivatedRoute, private concertsService: ConcertsService) {
    this.concertId = this.route.snapshot.paramMap.get('id');
    this.route.params.subscribe(params => {
      this.loadDataForId(params['id']);
    })
  }


  loadDataForId(id: string) {
    this.concertId = id;

    console.log('loadDataForId', id);

    this.concertsService.getAdjacentConcerts(this.concertId)
      .subscribe(adjacentConcerts => {
        if (adjacentConcerts != undefined) {
          this.adjacentConcertData$ = adjacentConcerts;
        }
      });
  }


  onFormSaved(concert: ConcertDto) {
    this.isSaving$ = true;

    concert.id = this.concertId!;

    // TODO: extra route needed?
    this.concertsService.addConcert(concert)
        .subscribe({
          next: (result) => {
            console.log("Update concert request finished");
            console.log(result);

            this.messageService.add({
              severity: "success",
              summary: "Saved concert!",
            });
            this.isSaving$ = false;
          },
          error: (err: HttpErrorResponse) => {
            this.isSaving$ = false;

            console.log(err);

            let errorResponse: ErrorResponseDto = err.error;
            this.messageService.add({
              severity: "danger",
              summary: "Failed to save concert!",
              text: errorResponse.message,
            });
          }
        });
  }
}
