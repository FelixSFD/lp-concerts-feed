import {Component, inject} from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {CreateSetlistRequestDto, ErrorResponseDto} from '../../../modules/lpshows-api';
import {Router} from '@angular/router';
import {SetlistsService} from '../../../services/setlists.service';
import {ToastrService} from 'ngx-toastr';

@Component({
  selector: 'app-edit-setlist-page',
  imports: [
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: './edit-setlist-page.component.html',
  styleUrl: './edit-setlist-page.component.css',
})
export class EditSetlistPageComponent {
  private formBuilder = inject(FormBuilder);
  private router = inject(Router);
  private setlistService = inject(SetlistsService);
  private toastr = inject(ToastrService);

  setlistForm = this.formBuilder.group({
    concertId: new FormControl('', [Validators.required]),
    concertTitle: new FormControl('', [Validators.required]),
    setName: new FormControl('', []),
    linkinpediaUrl: new FormControl('', [Validators.pattern(/^https?:\/\/[a-z0-9]+(?:[-.][a-z0-9]+)*(?::[0-9]{1,5})?(?:\/[^\/\r\n]+)*\.[a-z]{2,5}(?:[?#]\S*)?$/)]),
  });

  isSaving$: boolean = false;


  private makeRequestDtoFromFormData(): CreateSetlistRequestDto {
    let concertId = this.setlistForm.getRawValue().concertId?.valueOf();
    let linkinpediaUrl = this.setlistForm.value.linkinpediaUrl?.valueOf();

    console.log('concertId', concertId);
    console.log('linkinpediaUrl', linkinpediaUrl);

    let request: CreateSetlistRequestDto = {
      concertId: concertId,
      linkinpediaUrl: linkinpediaUrl,
    };

    console.debug("Request: ", request);

    return request;
  }

  onSaveClicked() {
    /*this.isSaving$ = true;
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
      });*/
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
