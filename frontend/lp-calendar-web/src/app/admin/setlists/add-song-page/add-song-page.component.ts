import {Component, inject} from '@angular/core';
import {ToastrService} from 'ngx-toastr';
import {SongsService} from '../../../services/songs.service';
import {Router, RouterLink} from '@angular/router';
import {SongFormComponent, SongFormContent} from '../song-form/song-form.component';
import {MashupFormComponent} from '../mashup-form/mashup-form.component';

@Component({
  selector: 'app-add-song-page',
  imports: [
    MashupFormComponent,
    RouterLink,
    SongFormComponent
  ],
  templateUrl: './add-song-page.component.html',
  styleUrl: './add-song-page.component.css',
})
export class AddSongPageComponent {
  private toastr = inject(ToastrService);
  private songsService = inject(SongsService);
  private router = inject(Router);

  isAdding$ = false;

  onSaveClicked(formContent: SongFormContent) {
    /*this.isAdding$ = true;

    let request: CreateSongMashupRequestDto = {
      title: formContent.title,
      linkinpediaUrl: formContent.linkinpediaUrl,
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
   */
  }
}
