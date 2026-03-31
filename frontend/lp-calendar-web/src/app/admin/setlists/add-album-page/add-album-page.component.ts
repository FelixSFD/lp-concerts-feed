import {Component, inject} from '@angular/core';
import {ToastrService} from 'ngx-toastr';
import {Router, RouterLink} from '@angular/router';
import {CreateAlbumRequestDto, ErrorResponseDto} from '../../../modules/lpshows-api';
import {AlbumsService} from '../../../services/music/albums.service';
import {AlbumFormComponent, AlbumFormContent} from '../album-form/album-form.component';

@Component({
  selector: 'app-add-album-page',
  imports: [
    RouterLink,
    AlbumFormComponent
  ],
  templateUrl: './add-album-page.component.html',
  styleUrl: './add-album-page.component.css',
})
export class AddAlbumPageComponent {
  private toastr = inject(ToastrService);
  private albumsService = inject(AlbumsService);
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
          this.toastr.error(err.message);
        });
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.toastr.error(errorResponse.message, "Could not create album");
        this.isAdding$ = false;
      }
    });
  }
}
