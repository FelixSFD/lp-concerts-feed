import {Component, inject} from '@angular/core';
import {MashupFormComponent, MashupFormContent} from '../mashup-form/mashup-form.component';
import {SongsService} from '../../../services/songs.service';
import {CreateSongMashupRequestDto, ErrorResponseDto} from '../../../modules/lpshows-api';
import {ToastrService} from 'ngx-toastr';
import {Router, RouterLink} from '@angular/router';

@Component({
  selector: 'app-add-mashup-page',
  imports: [
    MashupFormComponent,
    RouterLink
  ],
  templateUrl: './add-mashup-page.component.html',
  styleUrl: './add-mashup-page.component.css',
})
export class AddMashupPageComponent {
  private toastr = inject(ToastrService);
  private songsService = inject(SongsService);
  private router = inject(Router);

  isAdding$ = false;

  onSaveClicked(formContent: MashupFormContent) {
    this.isAdding$ = true;

    let request: CreateSongMashupRequestDto = {
      title: formContent.title,
      linkinpediaUrl: formContent.linkinpediaUrl,
      songIds: formContent.songs.map(s => s.id!)
    };

    this.songsService.createMashup(request).subscribe({
      next: createdMashup => {
        console.debug('Created new mashup', createdMashup);
        this.isAdding$ = false;
        this.router.navigate(["/", "admin", "mashups", createdMashup.id]).catch(err => {
          this.toastr.error(err.message);
        });
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.toastr.error(errorResponse.message, "Could not create mashup");
        this.isAdding$ = false;
      }
    });
  }
}
