import {Component, inject} from '@angular/core';
import {SongsService} from '../../../../../services/songs.service';
import {Router, RouterLink} from '@angular/router';
import {SongFormComponent, SongFormContent} from '../song-form/song-form.component';
import {CreateSongRequestDto, ErrorResponseDto} from '../../../../../modules/lpshows-api';
import {Button} from 'primeng/button';
import {Card} from 'primeng/card';
import {MessageService} from 'primeng/api';

@Component({
  selector: 'app-add-song-page',
  imports: [
    RouterLink,
    SongFormComponent,
    Button,
    Card
  ],
  templateUrl: './add-song-page.component.html',
  styleUrl: './add-song-page.component.css',
})
export class AddSongPageComponent {
  private messageService = inject(MessageService);
  private songsService = inject(SongsService);
  private router = inject(Router);

  isAdding$ = false;

  onSaveClicked(formContent: SongFormContent) {
    this.isAdding$ = true;

    let request: CreateSongRequestDto = {
      title: formContent.title,
      albumId: Number(formContent.albumId),
      isrc: formContent.isrc,
      appleMusicId: formContent.appleMusicId,
      linkinpediaUrl: formContent.linkinpediaUrl,
    };

    this.songsService.createSong(request).subscribe({
      next: createdSong => {
        console.debug('Created new song', createdSong);
        this.isAdding$ = false;
        this.router.navigate(["/", "admin", "songs", createdSong.id]).catch(err => {
          this.messageService.add({severity: "error", summary: "Failed to navigate to the new song"});
        });
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({severity: "error", summary: "Could not create the song", text: errorResponse.message});
        this.isAdding$ = false;
      }
    });
  }
}
