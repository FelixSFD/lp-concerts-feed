import {Component, inject, TemplateRef} from '@angular/core';
import {ToastrService} from 'ngx-toastr';
import {NgbModal, NgbModalRef} from '@ng-bootstrap/ng-bootstrap';
import {SongsService} from '../../../services/songs.service';
import {AlbumDto, ErrorResponseDto, SongDto} from '../../../modules/lpshows-api';
import {AlbumsService} from '../../../services/music/albums.service';
import {RouterLink} from '@angular/router';

@Component({
  selector: 'app-manage-albums-page',
  imports: [
    RouterLink
  ],
  templateUrl: './manage-albums-page.component.html',
  styleUrl: './manage-albums-page.component.css',
})
export class ManageAlbumsPageComponent {
  private toastr = inject(ToastrService);
  private modalService = inject(NgbModal);
  private albumsService = inject(AlbumsService);


  albums$: AlbumDto[] = [];

  albumToDelete: AlbumDto | null = null;
  isDeletingAlbum$ = false;

  // if open, the modal is referenced here
  deleteAlbumModal: NgbModalRef | undefined;


  ngOnInit() {
    this.reloadList(false);
  }


  dismissDeleteAlbumConfirmModal() {
    this.deleteAlbumModal?.dismiss();
  }


  openModal(content: TemplateRef<any>) {
    return this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
  }


  onDeleteAlbumClicked(content: TemplateRef<any>, album: AlbumDto) {
    this.albumToDelete = album;

    if (this.albumToDelete == null) {
      return;
    }

    this.deleteAlbumModal = this.openModal(content);
  }


  onDeleteAlbumConfirm() {
    this.isDeletingAlbum$ = true;

    if (this.albumToDelete) {
      this.albumsService.deleteAlbum(this.albumToDelete.id!)
        .subscribe({
          next: () => {
            this.reloadList(false);
            this.deleteAlbumModal?.dismiss();
            this.isDeletingAlbum$ = false;
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            this.toastr.error(errorResponse.message, "Could not delete album!");
            this.isDeletingAlbum$ = false;
          }
        });
    }
  }


  private reloadList(cache: boolean) {
    this.albumsService.getAllAlbums(cache).subscribe({
      next: albums => {
        this.albums$ = albums;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.toastr.error(errorResponse.message, "Could not load albums!");
      }
    })
  }
}
