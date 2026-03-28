import {Component, inject, viewChild} from '@angular/core';
import {ActivatedRoute, Router, RouterLink} from '@angular/router';
import {ToastrService} from 'ngx-toastr';
import {SongsService} from '../../../services/songs.service';
import {ErrorResponseDto, UpdateSongMashupRequestDto} from '../../../modules/lpshows-api';
import {SongFormComponent, SongFormContent} from '../song-form/song-form.component';
import {MashupFormComponent} from '../mashup-form/mashup-form.component';

@Component({
  selector: 'app-edit-song-page',
  imports: [
    MashupFormComponent,
    RouterLink,
    SongFormComponent
  ],
  templateUrl: './edit-song-page.component.html',
  styleUrl: './edit-song-page.component.css',
})
export class EditSongPageComponent {
  private router = inject(Router);
  private activeRoute = inject(ActivatedRoute);
  private toastr = inject(ToastrService);
  private songsService = inject(SongsService);

  private songFormComponent = viewChild(SongFormComponent);

  currentSongId: number = 0;

  isSaving$ = false;


  ngOnInit() {
    this.activeRoute.params.subscribe(params => {
      let songId = params['songId'];
      if (songId != null && songId > 0) {
        this.currentSongId = songId;
        this.loadSong(songId);
      }
    });
  }


  onSaveClicked(formContent: SongFormContent) {
    /*
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

     */
  }


  private loadSong(songId: number) {
    this.songsService.getSong(songId, false)
      .subscribe({
        next: data => {
          this.songFormComponent()?.fillFormWith(data);
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not load song");
        }
      });
  }
}
