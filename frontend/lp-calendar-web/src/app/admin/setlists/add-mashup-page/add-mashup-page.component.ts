import {Component, inject} from '@angular/core';
import {MashupFormComponent, MashupFormContent} from '../mashup-form/mashup-form.component';
import {SongsService} from '../../../services/songs.service';
import {CreateSongMashupRequestDto, ErrorResponseDto} from '../../../modules/lpshows-api';
import {ToastrService} from 'ngx-toastr';

@Component({
  selector: 'app-add-mashup-page',
  imports: [
    MashupFormComponent
  ],
  templateUrl: './add-mashup-page.component.html',
  styleUrl: './add-mashup-page.component.css',
})
export class AddMashupPageComponent {
  private toastr = inject(ToastrService);
  private songsService = inject(SongsService);

  onSaveClicked(formContent: MashupFormContent) {
    console.log(formContent);
    let request: CreateSongMashupRequestDto = {
      title: formContent.title,
      linkinpediaUrl: formContent.linkinpediaUrl,
      songIds: formContent.songs.map(s => s.id!)
    };

    this.songsService.createMashup(request).subscribe({
      next: createdMashup => {
        console.debug('Created new mashup', createdMashup);
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.toastr.error(errorResponse.message, "Could not create mashup");
      }
    })
  }
}
