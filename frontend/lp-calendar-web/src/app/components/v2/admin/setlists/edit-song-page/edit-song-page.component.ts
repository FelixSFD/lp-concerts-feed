import {Component, inject, OnInit, viewChild} from '@angular/core';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {SongsService} from '../../../../../services/songs.service';
import {ErrorResponseDto, UpdateSongRequestDto} from '../../../../../modules/lpshows-api';
import {SongFormComponent, SongFormContent} from '../song-form/song-form.component';
import {CommandError} from '@angular/cli/src/commands/mcp/host';
import {MessageService} from 'primeng/api';
import {AlbumFormComponent} from '../album-form/album-form.component';
import {Button} from 'primeng/button';
import {Card} from 'primeng/card';

@Component({
  selector: 'app-edit-song-page',
  imports: [
    RouterLink,
    SongFormComponent,
    AlbumFormComponent,
    Button,
    Card
  ],
  templateUrl: './edit-song-page.component.html',
  styleUrl: './edit-song-page.component.css',
})
export class EditSongPageComponent implements OnInit {
  private activeRoute = inject(ActivatedRoute);
  private messageService = inject(MessageService);
  private songsService = inject(SongsService);

  private songFormComponent = viewChild(SongFormComponent);

  currentSongId: number = 0;

  isSaving$ = false;


  ngOnInit() {
    this.activeRoute.data.subscribe(data => {
      console.debug("Resolved song data:", data);

      if (data['song'] instanceof CommandError) {
        this.messageService.add({severity: "error", summary: "Failed to load song", detail: data['song'].message, sticky: true});
        return;
      }

      this.currentSongId = data['song'].id;
      this.songFormComponent()?.fillFormWith(data['song']);
    });
  }


  onSaveClicked(formContent: SongFormContent) {
    this.isSaving$ = true;

    let request: UpdateSongRequestDto = {
      title: formContent.title,
      albumId: formContent.albumId != null ? Number(formContent.albumId) : null,
      isrc: formContent.isrc,
      appleMusicId: formContent.appleMusicId,
      linkinpediaUrl: formContent.linkinpediaUrl,
    };

    this.songsService.updateSong(this.currentSongId, request).subscribe({
      next: createdSong => {
        console.debug('Updated song', createdSong);
        this.messageService.add({severity: "success", summary: "Successfully saved this song"});
        this.isSaving$ = false;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({severity: "error", summary: "Failed to save song", detail: errorResponse.message});
        this.isSaving$ = false;
      }
    });
  }
}
