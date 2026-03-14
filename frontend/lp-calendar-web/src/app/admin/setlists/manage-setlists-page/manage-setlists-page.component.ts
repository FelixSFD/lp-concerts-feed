import {Component, inject, OnInit, TemplateRef} from '@angular/core';
import {RouterLink} from '@angular/router';
import {SetlistsService} from '../../../services/setlists.service';
import {ToastrService} from 'ngx-toastr';
import {Setlist} from '../../../data/setlists/setlist';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {NgbModal, NgbModalRef} from '@ng-bootstrap/ng-bootstrap';
import {ErrorResponseDto} from '../../../modules/lpshows-api';

@Component({
  selector: 'app-manage-setlists-page',
  imports: [
    RouterLink
  ],
  templateUrl: './manage-setlists-page.component.html',
  styleUrl: './manage-setlists-page.component.css',
})
export class ManageSetlistsPageComponent implements OnInit {
  private modalService = inject(NgbModal);

  setlists$: Setlist[] = [];

  // Setlist that will be deleted. Is used to store the data for the confirmation modal
  setlistToDelete: Setlist | undefined;

  // property to show whether the setlist is currently being deleted
  setlistDeleting$ = false;

  // if open, the modal is referenced here
  deleteSetlistModal: NgbModalRef | undefined;

  constructor(private setlistService: SetlistsService, private toastr: ToastrService) {
  }


  ngOnInit() {
    this.reloadList();
  }


  private reloadList() {
    this.setlistService.getSetlists().subscribe(setlists => {
      this.setlists$ = setlists.map(setlist => Setlist.fromDto(setlist));
    });
  }


  openModal(content: TemplateRef<any>) {
    return this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
  }


  onDeleteSetlistClicked(content: TemplateRef<any>, setlist: Setlist) {
    this.setlistToDelete = setlist;

    if (this.setlistToDelete == null) {
      return;
    }

    this.deleteSetlistModal = this.openModal(content);
  }


  onDeleteSetlistConfirm() {
    this.setlistDeleting$ = true;
    if (this.setlistToDelete == null) {
      this.deleteSetlistModal?.dismiss();
      return;
    }

    let id = this.setlistToDelete!.id;
    console.debug("Will delete setlist: " + id);

    this.setlistService.deleteSetlist(id).subscribe({
      next: result => {
        console.debug("DELETE setlist request finished");
        console.debug(result);

        this.reloadList();
        this.deleteSetlistModal?.dismiss();
        this.setlistDeleting$ = false;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        console.warn("Failed to delete setlist:", err);
        this.deleteSetlistModal?.dismiss();
        this.setlistDeleting$ = false;

        this.toastr.error(errorResponse.message, "Could not delete setlist!");
      }
    });
  }


  dismissSetlistConfirmModal() {
    this.deleteSetlistModal?.dismiss();
  }
}
