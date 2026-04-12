import {Component, inject, viewChild} from '@angular/core';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {ToastrService} from 'ngx-toastr';
import {ErrorResponseDto, UpdateAlbumRequestDto} from '../../../modules/lpshows-api';
import {AlbumsService} from '../../../services/music/albums.service';
import {AlbumFormComponent, AlbumFormContent} from '../album-form/album-form.component';

@Component({
  selector: 'app-edit-album-page',
  imports: [
    AlbumFormComponent,
    RouterLink
  ],
  templateUrl: './edit-album-page.component.html',
  styleUrl: './edit-album-page.component.css',
})
export class EditAlbumPageComponent {
  private activeRoute = inject(ActivatedRoute);
  private toastr = inject(ToastrService);
  private albumsService = inject(AlbumsService);

  private albumFormComponent = viewChild(AlbumFormComponent);

  currentAlbumId: number = 0;

  isSaving$ = false;


  ngOnInit() {
    this.activeRoute.params.subscribe(params => {
      let albumId = params['albumId'];
      if (albumId != null && albumId > 0) {
        this.currentAlbumId = albumId;
        this.loadAlbum(albumId);
      }
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
        this.toastr.success("Successfully saved album");
        this.isSaving$ = false;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.toastr.error(errorResponse.message, "Could not update album");
        this.isSaving$ = false;
      }
    });
  }


  private loadAlbum(albumId: number) {
    this.albumsService.getAlbum(albumId, false)
      .subscribe({
        next: data => {
          this.albumFormComponent()?.fillFormWith(data);
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not load album");
        }
      });
  }
}
