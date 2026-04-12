import {Component, inject, OnInit, viewChild} from '@angular/core';
import {ActivatedRoute, Router, RouterLink} from '@angular/router';
import {ToastrService} from 'ngx-toastr';
import {SongsService} from '../../../services/songs.service';
import {
  ErrorResponseDto,
  UpdateSongMashupRequestDto
} from '../../../modules/lpshows-api';
import {MashupFormComponent, MashupFormContent} from '../mashup-form/mashup-form.component';

@Component({
  selector: 'app-edit-mashup-page',
  imports: [
    MashupFormComponent,
    RouterLink
  ],
  templateUrl: './edit-mashup-page.component.html',
  styleUrl: './edit-mashup-page.component.css',
})
export class EditMashupPageComponent implements OnInit {
  private router = inject(Router);
  private activeRoute = inject(ActivatedRoute);
  private toastr = inject(ToastrService);
  private songsService = inject(SongsService);

  private mashupFormComponent = viewChild(MashupFormComponent);

  currentMashupId: number = 0;

  isSaving$ = false;


  ngOnInit() {
    this.activeRoute.params.subscribe(params => {
      let mashupId = params['mashupId'];
      if (mashupId != null && mashupId > 0) {
        this.currentMashupId = mashupId;
        this.loadMashup(mashupId);
      }
    });
  }


  onSaveClicked(formContent: MashupFormContent) {
    this.isSaving$ = true;

    let request: UpdateSongMashupRequestDto = {
      title: formContent.title,
      linkinpediaUrl: formContent.linkinpediaUrl,
      songIds: formContent.songs.map(s => s.id!)
    };

    this.songsService.updateMashup(this.currentMashupId, request).subscribe({
      next: createdMashup => {
        console.debug('Update mashup', createdMashup);
        this.toastr.success("Successfully saved mashup");
        this.isSaving$ = false;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.toastr.error(errorResponse.message, "Could not update mashup");
        this.isSaving$ = false;
      }
    });
  }


  private loadMashup(mashupId: number) {
    this.songsService.getMashup(mashupId, false)
      .subscribe({
        next: data => {
          this.mashupFormComponent()?.fillFormWith(data);
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not load mashup");
        }
      });
  }
}
