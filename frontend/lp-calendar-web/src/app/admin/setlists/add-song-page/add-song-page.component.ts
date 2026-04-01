import {Component, inject} from '@angular/core';
import {ToastrService} from 'ngx-toastr';
import {SongsService} from '../../../services/songs.service';
import {Router, RouterLink} from '@angular/router';
import {SongFormComponent, SongFormContent} from '../song-form/song-form.component';
import {CreateSongRequestDto, ErrorResponseDto} from '../../../modules/lpshows-api';

@Component({
  selector: 'app-add-song-page',
  imports: [
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
    this.isAdding$ = true;

    let request: CreateSongRequestDto = {
      title: formContent.title,
      albumId: Number(formContent.albumId),
      isrc: formContent.isrc,
      linkinpediaUrl: formContent.linkinpediaUrl,
    };

    this.songsService.createSong(request).subscribe({
      next: createdSong => {
        console.debug('Created new song', createdSong);
        this.isAdding$ = false;
        this.router.navigate(["/", "admin", "songs", createdSong.id]).catch(err => {
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
