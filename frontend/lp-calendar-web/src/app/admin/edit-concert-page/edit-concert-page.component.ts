import {Component} from '@angular/core';
import {ConcertFormComponent} from '../concert-form/concert-form.component';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {Concert} from '../../data/concert';
import {ConcertsService} from '../../services/concerts.service';
import {ToastrService} from 'ngx-toastr';

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

  isSaving$: boolean = false;

  constructor(private route: ActivatedRoute, private concertsService: ConcertsService, private toastr: ToastrService) {
    this.concertId = this.route.snapshot.paramMap.get('id');
  }


  onFormSaved(concert: Concert) {
    this.isSaving$ = true;

    concert.id = this.concertId!;

    // TODO: extra route needed?
    /*this.concertsService.addConcert(concert).subscribe(result => {
      console.log("Update concert request finished");
      console.log(result);

      this.toastr.success("Saved concert!");
      this.isSaving$ = false;
    });*/

    this.concertsService.addConcert(concert)
        .subscribe({
          next: (result) => {
            console.log("Update concert request finished");
            console.log(result);

            this.toastr.success("Saved concert!");
            this.isSaving$ = false;
          },
          error: (err) => {
            console.log(err);

            console.log(this.concertId)

            this.toastr.error("Invalid fields", "Failed to save concert!");
            this.isSaving$ = false;
          }
        });
  }
}
