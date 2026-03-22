import {Component, inject, OnInit, viewChild} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {ToastrService} from 'ngx-toastr';
import {SetlistEntry} from '../../../data/setlists/setlist-entry';
import {SongsService} from '../../../services/songs.service';
import {
  CreateSongMashupRequestDto,
  ErrorResponseDto,
  SongMashupDto,
  UpdateSongMashupRequestDto
} from '../../../modules/lpshows-api';
import {AddSetlistEntryFormComponent} from '../add-setlist-entry-form/add-setlist-entry-form.component';
import {MashupFormComponent, MashupFormContent} from '../mashup-form/mashup-form.component';

@Component({
  selector: 'app-edit-mashup-page',
  imports: [
    MashupFormComponent
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
    let request: UpdateSongMashupRequestDto = {
      title: formContent.title,
      linkinpediaUrl: formContent.linkinpediaUrl,
      songIds: formContent.songs.map(s => s.id!)
    };

    this.songsService.updateMashup(this.currentMashupId, request).subscribe({
      next: createdMashup => {
        console.debug('Update mashup', createdMashup);
        this.toastr.success("Successfully saved mashup");
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.toastr.error(errorResponse.message, "Could not update mashup");
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
