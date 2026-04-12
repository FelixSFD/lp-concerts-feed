import {Component, inject, OnInit, TemplateRef} from '@angular/core';
import {NgbModal, NgbModalRef} from '@ng-bootstrap/ng-bootstrap';
import {SongsService} from '../../../services/songs.service';
import {ToastrService} from 'ngx-toastr';
import {ErrorResponseDto, SongMashupDto} from '../../../modules/lpshows-api';
import {RouterLink} from '@angular/router';
import {Setlist} from '../../../data/setlists/setlist';

@Component({
  selector: 'app-manage-mashups-page',
  imports: [
    RouterLink
  ],
  templateUrl: './manage-mashups-page.component.html',
  styleUrl: './manage-mashups-page.component.css',
})
export class ManageMashupsPageComponent implements OnInit {
  private toastr = inject(ToastrService);
  private modalService = inject(NgbModal);
  private songsService = inject(SongsService);


  mashups$: SongMashupDto[] = [];

  mashupToDelete: SongMashupDto | null = null;
  isDeletingMashup$ = false;

  // if open, the modal is referenced here
  deleteMashupModal: NgbModalRef | undefined;


  ngOnInit() {
    this.reloadList(false);
  }


  dismissDeleteMashupConfirmModal() {
    this.deleteMashupModal?.dismiss();
  }


  openModal(content: TemplateRef<any>) {
    return this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
  }


  onDeleteSetlistClicked(content: TemplateRef<any>, mashup: SongMashupDto) {
    this.mashupToDelete = mashup;

    if (this.mashupToDelete == null) {
      return;
    }

    this.deleteMashupModal = this.openModal(content);
  }


  onDeleteMashupConfirm() {
    this.isDeletingMashup$ = true;

    if (this.mashupToDelete) {
      this.songsService.deleteMashup(this.mashupToDelete.id!)
        .subscribe({
          next: () => {
            this.reloadList(false);
            this.deleteMashupModal?.dismiss();
            this.isDeletingMashup$ = false;
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            this.toastr.error(errorResponse.message, "Could not delete mashup!");
            this.isDeletingMashup$ = false;
          }
        });
    }
  }


  private reloadList(cache: boolean) {
    this.songsService.getAllMashups(cache).subscribe({
      next: mashups => {
        this.mashups$ = mashups;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.toastr.error(errorResponse.message, "Could not load mashups!");
      }
    })
  }
}
