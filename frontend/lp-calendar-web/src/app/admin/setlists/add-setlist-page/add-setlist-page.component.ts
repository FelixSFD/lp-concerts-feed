import {Component, inject, Input, OnInit} from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {NgClass} from '@angular/common';
import {ActivatedRoute, Router} from '@angular/router';
import {CreateSetlistRequestDto, ErrorResponseDto} from '../../../modules/lpshows-api';
import {SetlistsService} from '../../../services/setlists.service';
import {ToastrService} from 'ngx-toastr';
import {ConcertsService} from '../../../services/concerts.service';
import {DateTime} from 'luxon';

@Component({
  selector: 'app-add-setlist-page',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgClass
  ],
  templateUrl: './add-setlist-page.component.html',
  styleUrl: './add-setlist-page.component.css',
})
export class AddSetlistPageComponent implements OnInit {
  private formBuilder = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private concertsService = inject(ConcertsService);

  setlistForm = this.formBuilder.group({
    concertId: new FormControl('', [Validators.required]),
    concertTitle: new FormControl('', [Validators.min(5)]),
    setName: new FormControl('', []),
    linkinpediaUrl: new FormControl('', [Validators.pattern(/^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)/)]),
  });

  isSaving$: boolean = false;

  constructor(private setlistService: SetlistsService, private toastr: ToastrService) {
  }

  ngOnInit() {
    this.setlistForm.controls.concertId.valueChanges.subscribe(id => {
      console.debug("Changed concertId", id);
      if (id != null) {
        this.concertsService.getConcert(id).subscribe(concert => {
          let startDate = DateTime.fromISO(concert.postedStartTime!);
          let year = startDate.year.toString();
          let month = startDate.month < 10 ? '0' + startDate.month : startDate.month;
          let day = startDate.day < 10 ? '0' + startDate.day : startDate.day;
          this.setlistForm.controls.concertTitle.setValue(`${concert.locationShort} - ${year}-${month}-${day}`);
          this.setlistForm.controls.linkinpediaUrl.setValue(`https://linkinpedia.com/wiki/Live:${year}${month}${day}`);
        });
      }
    });

    this.route.params.subscribe(params => {
      let concertId = params['concertId'];
      if (concertId != null && concertId.length > 0) {
        this.setlistForm.controls.concertId.setValue(params['concertId'], { emitEvent: true });
        this.setlistForm.controls.concertId.disable();
      }
    });
  }

  private makeRequestDtoFromFormData(): CreateSetlistRequestDto {
    let concertId = this.setlistForm.getRawValue().concertId?.valueOf();
    let concertTitle = this.setlistForm.value.concertTitle?.valueOf();
    let setName = this.setlistForm.value.setName?.valueOf();
    let linkinpediaUrl = this.setlistForm.value.linkinpediaUrl?.valueOf();

    console.log('concertId', concertId);
    console.log('linkinpediaUrl', linkinpediaUrl);

    let request: CreateSetlistRequestDto = {
      concertId: concertId,
      concertTitle: concertTitle,
      setName: setName,
      linkinpediaUrl: linkinpediaUrl,
    };

    console.debug("Request: ", request);

    return request;
  }

  onSaveClicked() {
    this.isSaving$ = true;
    let request = this.makeRequestDtoFromFormData();
    this.setlistService.createSetlist(request)
      .subscribe({
        next: (response) => {
          let toast = this.toastr.success("Setlist was created successfully");
          toast.onTap.subscribe(toast => {
            this.navigateToSetlist(response.id);
          });

          this.navigateToSetlist(response.id);

          this.isSaving$ = false;
          this.setlistForm.reset();
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not create setlist");
          this.isSaving$ = false;
        }
      });
  }

  private navigateToSetlist(id: string | undefined) {
    this.router.navigate(['/admin/setlists/' + id]).catch(err => this.toastr.error(err));
  }

  openConcertDetailsClicked() {
    let concertId = this.setlistForm.getRawValue().concertId?.valueOf();
    if (concertId?.length == 0) {
      return;
    }

    window.open("/concerts/" + concertId, "_blank");
  }


  openLinkinpediaUrlClicked() {
    let url = this.setlistForm.value.linkinpediaUrl?.valueOf();
    if (url?.length == 0) {
      return;
    }

    window.open(url, "_blank");
  }
}
