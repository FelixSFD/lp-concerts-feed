import {Component, inject, viewChild} from '@angular/core';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {ErrorResponseDto, UpdateAlbumRequestDto} from '../../../../../modules/lpshows-api';
import {AlbumsService} from '../../../../../services/music/albums.service';
import {AlbumFormComponent, AlbumFormContent} from '../../../../../admin/setlists/album-form/album-form.component';
import {CommandError} from '@angular/cli/src/commands/mcp/host';
import {MessageService} from 'primeng/api';
import {Button} from 'primeng/button';
import {Card} from 'primeng/card';

@Component({
  selector: 'app-edit-album-page',
  imports: [
    AlbumFormComponent,
    RouterLink,
    Button,
    Card
  ],
  templateUrl: './edit-album-page.component.html',
  styleUrl: './edit-album-page.component.css',
})
export class EditAlbumPageComponent {
  private activeRoute = inject(ActivatedRoute);
  private messageService = inject(MessageService);
  private albumsService = inject(AlbumsService);

  private albumFormComponent = viewChild(AlbumFormComponent);

  currentAlbumId: number = 0;

  isSaving$ = false;


  ngOnInit() {
    this.activeRoute.data.subscribe(data => {
      console.debug("Resolved album data:", data);

      if (data['album'] instanceof CommandError) {
        this.messageService.add({severity: "error", summary: "Failed to load album", detail: data['album'].message, sticky: true});
        return;
      }

      this.currentAlbumId = data['album'].id;
      this.albumFormComponent()?.fillFormWith(data['album']);
    });
  }


  onSaveClicked(formContent: AlbumFormContent) {
    this.isSaving$ = true;

    let request: UpdateAlbumRequestDto = {
      title: formContent.title,
      linkinpediaUrl: formContent.linkinpediaUrl,
    };

    this.albumsService.updateAlbum(this.currentAlbumId, request).subscribe({
      next: updatedAlbum => {
        console.debug('Updated album', updatedAlbum);
        this.messageService.add({severity: "success", summary: "Successfully saved this album"});
        this.isSaving$ = false;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({severity: "error", summary: "Failed to save album", detail: errorResponse.message});
        this.isSaving$ = false;
      }
    });
  }
}
