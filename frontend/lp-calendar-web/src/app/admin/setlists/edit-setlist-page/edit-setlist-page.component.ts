import {Component, inject, OnInit, TemplateRef} from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {
  CreateSetlistRequestDto,
  ErrorResponseDto,
  SetlistDto,
  UpdateSetlistHeaderRequestDto
} from '../../../modules/lpshows-api';
import {ActivatedRoute, Router} from '@angular/router';
import {SetlistsService} from '../../../services/setlists.service';
import {ToastrService} from 'ngx-toastr';
import {NgClass} from '@angular/common';
import {Observable} from 'rxjs';
import {SetlistEntry} from '../../../data/setlists/setlist-entry';
import {Setlist} from '../../../data/setlists/setlist';
import {NgbModal, NgbModalRef} from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-edit-setlist-page',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgClass
  ],
  templateUrl: './edit-setlist-page.component.html',
  styleUrl: './edit-setlist-page.component.css',
})
export class EditSetlistPageComponent implements OnInit {
  private modalService = inject(NgbModal);
  private formBuilder = inject(FormBuilder);
  private router = inject(Router);
  private activeRoute = inject(ActivatedRoute);
  private setlistService = inject(SetlistsService);
  private toastr = inject(ToastrService);

  private currentSetlistId: number = 0;

  setlistForm = this.formBuilder.group({
    concertId: new FormControl('', [Validators.required]),
    concertTitle: new FormControl('', [Validators.required]),
    setName: new FormControl('', []),
    linkinpediaUrl: new FormControl('', [Validators.pattern(/^https?:\/\/[a-z0-9]+(?:[-.][a-z0-9]+)*(?::[0-9]{1,5})?(?:\/[^\/\r\n]+)*\.[a-z]{2,5}(?:[?#]\S*)?$/)]),
  });

  isSaving$: boolean = false;

  isAddingEntry$: boolean = false;

  // if open, the modal is referenced here
  addEntryModal: NgbModalRef | undefined;

  setlistEntries$: SetlistEntry[] = [];

  ngOnInit() {
    this.activeRoute.params.subscribe(params => {
      let setlistId = params['setlistId'];
      if (setlistId != null && setlistId > 0) {
        this.loadSetlist(setlistId);
        this.currentSetlistId = setlistId;
      }
    });
  }

  private loadSetlist(setlistId: number) {
    this.setlistService
      .getSetlist(setlistId)
      .subscribe(setlist => {
        this.fillFormFromDto(setlist);
        this.setlistEntries$ = setlist.entries?.map(SetlistEntry.fromDto) ?? [];
      });
  }

  private fillFormFromDto(setlist: SetlistDto) {
    this.setlistForm.controls.concertId.setValue(setlist.concertId ?? null);
    this.setlistForm.controls.concertTitle.setValue(setlist.concertTitle ?? null);
    this.setlistForm.controls.linkinpediaUrl.setValue(setlist.linkinpediaUrl ?? null);
    this.setlistForm.controls.setName.setValue(setlist.setName ?? null);
  }

  private makeRequestDtoFromFormData(): UpdateSetlistHeaderRequestDto {
    let concertId = this.setlistForm.getRawValue().concertId?.valueOf();
    let linkinpediaUrl = this.setlistForm.value.linkinpediaUrl?.valueOf();
    let setName = this.setlistForm.value.setName?.valueOf();

    console.log('concertId', concertId);
    console.log('linkinpediaUrl', linkinpediaUrl);

    let request: UpdateSetlistHeaderRequestDto = {
      //concertId: concertId!,
      setName: setName,
      linkinpediaUrl: linkinpediaUrl,
    };

    console.debug("Request: ", request);

    return request;
  }

  onSaveClicked() {
    this.isSaving$ = true;
    let request = this.makeRequestDtoFromFormData();
    this.setlistService.updateSetlistHeader(this.currentSetlistId, request)
      .subscribe({
        next: (response) => {
          this.toastr.success("Setlist was updated successfully");

          this.isSaving$ = false;
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not update setlist");
          this.isSaving$ = false;
        }
      });
  }

  private navigateToSetlist(id: string | undefined) {
    this.router.navigate(['/admin/setlists/' + id]).catch(err => this.toastr.error(err));
  }

  openConcertDetailsClicked() {
    let concertId = this.setlistForm.getRawValue().concertId?.valueOf();
    if (concertId?.length == 0) {
      return;
    }

    window.open("/concerts/" + concertId, "_blank");
  }


  openLinkinpediaUrlClicked() {
    let url = this.setlistForm.value.linkinpediaUrl?.valueOf();
    if (url?.length == 0) {
      return;
    }

    window.open(url, "_blank");
  }


  openModal(content: TemplateRef<any>) {
    return this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
  }


  onAddEntryClicked(content: TemplateRef<any>) {
    //this.setlistToDelete = setlist;

    /*if (this.setlistToDelete == null) {
      return;
    }*/

    this.addEntryModal = this.openModal(content);
  }


  onAddEntryConfirm() {
    /*this.isAddingEntry$ = true;
    if (this.setlistToDelete == null) {
      this.addEntryModal?.dismiss();
      return;
    }*/

    /*let id = this.setlistToDelete!.id;
    console.debug("Will delete setlist: " + id);

    this.setlistService.deleteSetlist(id).subscribe({
      next: result => {
        console.debug("DELETE setlist request finished");
        console.debug(result);

        this.loadSetlist(this.currentSetlistId);
        this.addEntryModal?.dismiss();
        this.isAddingEntry$ = false;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        console.warn("Failed to delete setlist:", err);
        this.addEntryModal?.dismiss();
        this.isAddingEntry$ = false;

        this.toastr.error(errorResponse.message, "Could not delete setlist!");
      }
    });*/
  }


  dismissAddEntryModal() {
    this.addEntryModal?.dismiss();
  }
}
