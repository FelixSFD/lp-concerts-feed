import {Component, inject} from '@angular/core';
import {ToastrService} from 'ngx-toastr';
import {Router, RouterLink} from '@angular/router';
import {CreateAlbumRequestDto, ErrorResponseDto} from '../../../../../modules/lpshows-api';
import {AlbumsService} from '../../../../../services/music/albums.service';
import {AlbumFormComponent, AlbumFormContent} from '../../../../../admin/setlists/album-form/album-form.component';
import {MessageService} from 'primeng/api';
import {Button} from 'primeng/button';
import {Card} from 'primeng/card';

@Component({
  selector: 'app-add-album-page',
  imports: [
    RouterLink,
    AlbumFormComponent,
    Button,
    Card
  ],
  templateUrl: './add-album-page.component.html',
  styleUrl: './add-album-page.component.css',
})
export class AddAlbumPageComponent {
  private albumsService = inject(AlbumsService);
  private messageService = inject(MessageService);
  private router = inject(Router);

  isAdding$ = false;

  onSaveClicked(formContent: AlbumFormContent) {
    this.isAdding$ = true;

    let request: CreateAlbumRequestDto = {
      title: formContent.title,
      linkinpediaUrl: formContent.linkinpediaUrl,
    };

    this.albumsService.createAlbum(request).subscribe({
      next: createdAlbum => {
        console.debug('Created new album', createdAlbum);
        this.isAdding$ = false;
        this.router.navigate(["/", "admin", "albums", createdAlbum.id]).catch(err => {
          this.messageService.add({severity: "error", summary: "Failed to navigate to the new album"});
        });
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({severity: "error", summary: "Could not create the album", text: errorResponse.message});
        this.isAdding$ = false;
      }
    });
  }
}
