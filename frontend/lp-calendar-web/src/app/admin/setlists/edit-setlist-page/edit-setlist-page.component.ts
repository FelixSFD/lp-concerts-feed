import {Component, contentChild, inject, OnInit, TemplateRef, viewChild} from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {
  CreateSetlistRequestDto,
  ErrorResponseDto, ReorderSetlistEntriesRequestDto,
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
import {
  AddSetlistEntryFormComponent,
  AddSetlistEntryFormContent
} from '../add-setlist-entry-form/add-setlist-entry-form.component';

@Component({
  selector: 'app-edit-setlist-page',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgClass,
    AddSetlistEntryFormComponent
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

  private addEntryFormComponent = viewChild(AddSetlistEntryFormComponent);
  private editEntryFormComponent = viewChild(AddSetlistEntryFormComponent);

  private currentSetlistId: number = 0;

  setlistForm = this.formBuilder.group({
    concertId: new FormControl('', [Validators.required]),
    concertTitle: new FormControl('', [Validators.required]),
    setName: new FormControl('', []),
    linkinpediaUrl: new FormControl('', [Validators.pattern(/^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)/)]),
  });

  isSaving$: boolean = false;

  isAddingEntry$: boolean = false;
  isEditingEntry$: boolean = false;

  isPendingReorder$: boolean = false;

  // Setlist entry that will be deleted. Is used to store the data for the confirmation modal
  entryToDelete: SetlistEntry | undefined;

  // Setlist entry that will be edited. Is used to store the data for the form
  entryToEdit: SetlistEntry | undefined;

  // property to show whether the setlist is currently being deleted
  entryDeleting$ = false;

  // if open, the modal is referenced here
  addEntryModal: NgbModalRef | undefined;

  // if open, the modal is referenced here
  editEntryModal: NgbModalRef | undefined;

  // if open, the modal is referenced here
  deleteEntryModal: NgbModalRef | undefined;

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
    this.isAddingEntry$ = true;

    // read the values from the form
    console.debug("Found component: ", this.addEntryFormComponent);
    let formValues = this.addEntryFormComponent()?.readValuesFromForm();
    console.debug("Values read from form: ", formValues);

    if (formValues != null) {
      this.setlistService.addSetlistEntry(formValues, this.currentSetlistId)
        .subscribe({
          next: () => {
            this.toastr.success("Setlist was updated successfully");
            this.isAddingEntry$ = false;
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            this.toastr.error(errorResponse.message, "Could not update setlist");
          }
        });
    } else {
      this.toastr.error("Failed to read form data!");
      this.isAddingEntry$ = false;
    }
  }


  onEntryMoveUpClicked(entryId: string) {
    let currentIndex = this.setlistEntries$.findIndex(e => e.id === entryId);
    let newIndex = currentIndex - 1;

    console.debug("Current index: ", currentIndex);
    console.debug("New index: ", newIndex);

    if (newIndex >= 0) {
      const updated = [...this.setlistEntries$];
      [updated[newIndex], updated[currentIndex]] = [updated[currentIndex], updated[newIndex]];

      this.setlistEntries$ = updated;
      this.isPendingReorder$ = true;
    } else {
      console.warn("Already on top of the list");
    }
  }


  onEntryMoveDownClicked(entryId: string) {
    let currentIndex = this.setlistEntries$.findIndex(e => e.id === entryId);
    let newIndex = currentIndex + 1;

    console.debug("Current index: ", currentIndex);
    console.debug("New index: ", newIndex);

    if (newIndex < this.setlistEntries$.length) {
      const updated = [...this.setlistEntries$];
      [updated[newIndex], updated[currentIndex]] = [updated[currentIndex], updated[newIndex]];

      this.setlistEntries$ = updated;
      this.isPendingReorder$ = true;
    } else {
      console.warn("Already at the end of the list");
    }
  }


  onSaveOrderClicked() {
    console.debug("Save new order...");
    this.isSaving$ = true;

    this.setlistService.applyNewEntryOrder(this.currentSetlistId, this.setlistEntries$.map(e => e.id))
      .subscribe({
        next: () => {
          this.toastr.success("Setlist was reordered successfully");
          this.isPendingReorder$ = false;
          this.isSaving$ = false;
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not reorder setlist");
          this.isSaving$ = false;
        }
      })
  }


  dismissAddEntryModal() {
    this.addEntryModal?.dismiss();
  }


  onDeleteEntryClicked(content: TemplateRef<any>, entry: SetlistEntry) {
    this.entryToDelete = entry;

    if (this.entryToDelete == null) {
      return;
    }

    this.deleteEntryModal = this.openModal(content);
  }


  onDeleteEntryConfirm() {
    this.entryDeleting$ = true;
    if (this.entryToDelete == null) {
      this.deleteEntryModal?.dismiss();
      return;
    }

    let id = this.entryToDelete!.id;
    console.debug("Will delete setlist entry: " + id);

    this.setlistService.deleteSetlistEntry(this.currentSetlistId, id).subscribe({
      next: result => {
        console.debug("DELETE setlist request entry finished");
        console.debug(result);

        this.deleteEntryModal?.dismiss();
        this.entryDeleting$ = false;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        console.warn("Failed to delete setlist entry:", err);
        this.deleteEntryModal?.dismiss();
        this.entryDeleting$ = false;

        this.toastr.error(errorResponse.message, "Could not delete setlist!");
      }
    });
  }


  dismissDeleteEntryConfirmModal() {
    this.deleteEntryModal?.dismiss();
  }


  onEditEntryClicked(content: TemplateRef<any>, entry: SetlistEntry) {
    this.entryToEdit = entry;

    if (this.entryToEdit == null) {
      return;
    }

    this.editEntryModal = this.openModal(content);
    this.editEntryFormComponent()?.loadEntry(this.currentSetlistId, entry.id);
  }


  onEditEntryConfirm() {
    this.isEditingEntry$ = true;
    if (this.entryToEdit == null) {
      this.editEntryModal?.dismiss();
      return;
    }

    let id = this.entryToEdit!.id;
    console.debug("Will edit setlist entry: " + id);

    let formValues = this.editEntryFormComponent()?.readValuesFromForm();
    console.debug("Values read from form: ", formValues);
  }


  dismissEditEntryConfirmModal() {
    this.editEntryModal?.dismiss();
  }
}
