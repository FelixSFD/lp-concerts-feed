import {Component, inject, OnInit, viewChild} from '@angular/core';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {ToastrService} from 'ngx-toastr';
import {SongsService} from '../../../services/songs.service';
import {ErrorResponseDto, UpdateSongRequestDto} from '../../../modules/lpshows-api';
import {SongFormComponent, SongFormContent} from '../song-form/song-form.component';

@Component({
  selector: 'app-edit-song-page',
  imports: [
    RouterLink,
    SongFormComponent
  ],
  templateUrl: './edit-song-page.component.html',
  styleUrl: './edit-song-page.component.css',
})
export class EditSongPageComponent implements OnInit {
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
    this.isSaving$ = true;

    let request: UpdateSongRequestDto = {
      title: formContent.title,
      albumId: Number(formContent.albumId),
      isrc: formContent.isrc,
      linkinpediaUrl: formContent.linkinpediaUrl,
    };

    this.songsService.updateSong(this.currentSongId, request).subscribe({
      next: createdSong => {
        console.debug('Updated song', createdSong);
        this.toastr.success("Successfully saved song");
        this.isSaving$ = false;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.toastr.error(errorResponse.message, "Could not update song");
        this.isSaving$ = false;
      }
    });
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
