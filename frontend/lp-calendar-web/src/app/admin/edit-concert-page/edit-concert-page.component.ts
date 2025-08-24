import {Component} from '@angular/core';
import {ConcertFormComponent} from '../concert-form/concert-form.component';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {ConcertsService} from '../../services/concerts.service';
import {ToastrService} from 'ngx-toastr';
import {ErrorResponse} from "../../data/error-response";
import {HttpErrorResponse} from "@angular/common/http";
import {AdjacentConcertsResponseDto, ConcertDto} from '../../modules/lpshows-api';

@Component({
  selector: 'app-edit-concert-page',
  imports: [
    ConcertFormComponent,
    RouterLink
  ],
  templateUrl: './edit-concert-page.component.html',
  styleUrl: './edit-concert-page.component.css'
})
export class EditConcertPageComponent {
  concertId: string | null;
  adjacentConcertData$: AdjacentConcertsResponseDto | null = null;

  isSaving$: boolean = false;

  constructor(private route: ActivatedRoute, private concertsService: ConcertsService, private toastr: ToastrService) {
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

            this.toastr.success("Saved concert!");
            this.isSaving$ = false;
          },
          error: (err: HttpErrorResponse) => {
            this.isSaving$ = false;

            console.log(err);

            let errorResponse: ErrorResponse = err.error;

            this.toastr.error(errorResponse.message, "Failed to save concert!");
          }
        });
  }
}
