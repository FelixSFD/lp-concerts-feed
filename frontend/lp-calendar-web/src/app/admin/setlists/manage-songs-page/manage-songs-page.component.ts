import {Component, inject, TemplateRef} from '@angular/core';
import {ToastrService} from 'ngx-toastr';
import {NgbModal, NgbModalRef} from '@ng-bootstrap/ng-bootstrap';
import {SongsService} from '../../../services/songs.service';
import {ErrorResponseDto, SongDto} from '../../../modules/lpshows-api';
import {RouterLink} from '@angular/router';

@Component({
  selector: 'app-manage-songs-page',
  imports: [
    RouterLink
  ],
  templateUrl: './manage-songs-page.component.html',
  styleUrl: './manage-songs-page.component.css',
})
export class ManageSongsPageComponent {
  private toastr = inject(ToastrService);
  private modalService = inject(NgbModal);
  private songsService = inject(SongsService);


  songs$: SongDto[] = [];

  songToDelete: SongDto | null = null;
  isDeletingSong$ = false;

  // if open, the modal is referenced here
  deleteSongModal: NgbModalRef | undefined;


  ngOnInit() {
    this.reloadList(false);
  }


  dismissDeleteSongConfirmModal() {
    this.deleteSongModal?.dismiss();
  }


  openModal(content: TemplateRef<any>) {
    return this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
  }


  onDeleteSongClicked(content: TemplateRef<any>, song: SongDto) {
    this.songToDelete = song;

    if (this.songToDelete == null) {
      return;
    }

    this.deleteSongModal = this.openModal(content);
  }


  onDeleteSongConfirm() {
    this.isDeletingSong$ = true;

    if (this.songToDelete) {
      this.songsService.deleteSong(this.songToDelete.id!)
        .subscribe({
          next: () => {
            this.reloadList(false);
            this.deleteSongModal?.dismiss();
            this.isDeletingSong$ = false;
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            this.toastr.error(errorResponse.message, "Could not delete song!");
            this.isDeletingSong$ = false;
          }
        });
    }
  }


  private reloadList(cache: boolean) {
    this.songsService.getAllSongs(cache).subscribe({
      next: songs => {
        this.songs$ = songs;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.toastr.error(errorResponse.message, "Could not load songs!");
      }
    })
  }
}
