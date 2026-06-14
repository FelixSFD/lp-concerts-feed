import {Component, inject, OnInit, viewChild} from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {
  ErrorResponseDto,
  SetlistDto,
  UpdateSetlistHeaderRequestDto
} from '../../../../../modules/lpshows-api';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {SetlistsService} from '../../../../../services/setlists.service';
import {SetlistEntry} from '../../../../../data/setlists/setlist-entry';
import {
  AddSetlistEntryFormComponent
} from '../../../../../admin/setlists/add-setlist-entry-form/add-setlist-entry-form.component';
import {SetlistEntryIconsComponent} from '../../../setlists/setlist-entry-icons/setlist-entry-icons.component';
import {Button} from 'primeng/button';
import {Card} from 'primeng/card';
import {Divider} from 'primeng/divider';
import {TableModule, TableRowReorderEvent} from 'primeng/table';
import {ButtonGroup} from 'primeng/buttongroup';
import {ConfirmDialog} from 'primeng/confirmdialog';
import {ConfirmationService, MessageService} from 'primeng/api';
import {Dialog} from 'primeng/dialog';
import {FloatLabel} from 'primeng/floatlabel';
import {InputText} from 'primeng/inputtext';
import {InputGroup} from 'primeng/inputgroup';
import {InputGroupAddon} from 'primeng/inputgroupaddon';

@Component({
  selector: 'app-edit-setlist-page',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    AddSetlistEntryFormComponent,
    SetlistEntryIconsComponent,
    RouterLink,
    Button,
    Card,
    Divider,
    TableModule,
    ButtonGroup,
    ConfirmDialog,
    Dialog,
    FloatLabel,
    InputText,
    InputGroup,
    InputGroupAddon
  ],
  templateUrl: './edit-setlist-page.component.html',
  styleUrl: './edit-setlist-page.component.css',
})
export class EditSetlistPageComponent implements OnInit {
  private formBuilder = inject(FormBuilder);
  private activeRoute = inject(ActivatedRoute);
  private setlistService = inject(SetlistsService);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  private addEntryFormComponent = viewChild(AddSetlistEntryFormComponent);

  currentSetlistId: number = 0;

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

  isShowingAddEntryDialog$: boolean = false;
  isShowingEditEntryDialog$: boolean = false;

  // Setlist entry that will be edited. Is used to store the data for the form
  entryToEdit: SetlistEntry | undefined | null;

  // property to show whether the setlist is currently being deleted
  entryDeleting$ = false;

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
      .getSetlist(setlistId, false)
      .subscribe(setlist => {
        this.fillFormFromDto(setlist);
        this.setlistEntries$ = setlist.entries?.map(SetlistEntry.fromDto) ?? [];
      });
  }

  private reloadCurrentSetlist() {
    this.loadSetlist(this.currentSetlistId);
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
      concertId: concertId!,
      setName: setName,
      linkinpediaUrl: linkinpediaUrl,
    };

    console.debug("Request: ", request);

    return request;
  }

  private getLargestSongNumber(): number {
    const entries = [...this.setlistEntries$];
    return entries
      .sort((a, b) => b.songNumber - a.songNumber)
      .map(e => e.songNumber)
      .at(0) ?? 0;
  }

  onSaveClicked() {
    this.isSaving$ = true;
    let request = this.makeRequestDtoFromFormData();
    this.setlistService.updateSetlistHeader(this.currentSetlistId, request)
      .subscribe({
        next: (response) => {
          this.messageService.add({
            severity: 'success',
            summary: 'Successfully updated successfully',
          });

          this.isSaving$ = false;
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.messageService.add({
            severity: 'danger',
            summary: 'Could not update setlist',
            text: errorResponse.message,
          });
          this.isSaving$ = false;
        }
      });
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


  onAddEntryClicked() {
    let largestSongNumber = this.getLargestSongNumber();
    console.debug("Largest Song Number right now: ", largestSongNumber);
    this.addEntryFormComponent()?.setSongNumber(largestSongNumber + 1);

    this.isShowingAddEntryDialog$ = true;
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
            this.messageService.add({
              severity: 'success',
              summary: 'Entry was added successfully',
            });
            this.reloadCurrentSetlist();

            this.dismissAddEntryModal();
            this.isAddingEntry$ = false;
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            this.messageService.add({
              severity: 'danger',
              summary: 'Could not add entry to setlist',
              text: errorResponse.message,
            });
            this.dismissAddEntryModal();
            this.isAddingEntry$ = false;
          }
        });
    } else {
      this.messageService.add({
        severity: 'danger',
        summary: 'Failed to read form data!',
      });
      this.isAddingEntry$ = false;
    }
  }


  onRowReordered(event: TableRowReorderEvent) {
    console.debug("RowReordered: ", event);
    this.isPendingReorder$ = true;
  }


  onSaveOrderClicked() {
    console.debug("Save new order...");
    this.isSaving$ = true;

    this.setlistService.applyNewEntryOrder(this.currentSetlistId, this.setlistEntries$.map(e => e.id))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Successfully reordered setlist entries',
          });
          this.isPendingReorder$ = false;
          this.isSaving$ = false;
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.messageService.add({
            severity: 'danger',
            summary: 'Could not reorder setlist',
            text: errorResponse.message,
          });
          this.isSaving$ = false;
        }
      })
  }


  dismissAddEntryModal() {
    this.isShowingAddEntryDialog$ = false;
  }


  onDeleteEntryClicked(event: Event, entry: SetlistEntry) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `Do you want to delete the entry "${entry.title}"?`,
      header: 'Delete setlist entry',
      icon: 'pi pi-info-circle',
      rejectLabel: 'Cancel',
      rejectButtonProps: {
        label: 'Cancel',
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: 'Delete',
        severity: 'danger'
      },

      accept: () => {
        this.onDeleteEntryConfirm(entry);
      }
    });
  }


  onDeleteEntryConfirm(entry: SetlistEntry) {
    let id = entry.id;
    console.debug("Will delete setlist entry: " + id);

    this.setlistService.deleteSetlistEntry(this.currentSetlistId, id).subscribe({
      next: result => {
        console.debug("DELETE setlist request entry finished");
        console.debug(result);
        this.entryDeleting$ = false;
        this.reloadCurrentSetlist();
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        console.warn("Failed to delete setlist entry:", err);
        this.entryDeleting$ = false;

        this.messageService.add({
          severity: 'danger',
          summary: 'Could not delete setlist entry',
          text: errorResponse.message,
        });
      }
    });
  }


  onEditEntryClicked(content: AddSetlistEntryFormComponent, entry: SetlistEntry) {
    console.debug("Content: ", content);
    this.entryToEdit = entry;

    if (this.entryToEdit == null) {
      return;
    }

    this.isShowingEditEntryDialog$ = true;
    content?.loadEntry(this.currentSetlistId, entry.id);
  }


  onEditEntryConfirm(content: AddSetlistEntryFormComponent) {
    if (this.entryToEdit == null) {
      return;
    }

    this.isEditingEntry$ = true;

    let id = this.entryToEdit.id;
    console.debug("Will edit setlist entry: " + id);

    let formValues = content.readValuesFromForm();
    console.debug("Values read from form: ", formValues);

    if (formValues != null) {
      this.setlistService.updateSetlistEntry(formValues, this.currentSetlistId, id)
        .subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Entry was updated successfully',
            });
            this.entryToEdit = null;
            this.isEditingEntry$ = false;
            this.reloadCurrentSetlist();
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            this.messageService.add({
              severity: 'danger',
              summary: 'Could not update setlist entry',
              text: errorResponse.message,
            });
          }
        });
    } else {
      this.messageService.add({
        severity: 'danger',
        summary: 'Failed to read form data',
      });
      this.isEditingEntry$ = false;
    }
  }


  dismissEditEntryModal() {
    this.isShowingEditEntryDialog$ = false;
    this.entryToEdit = null;
  }
}
