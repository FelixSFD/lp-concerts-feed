import { Component, effect, inject, OnInit, signal, viewChild } from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {
  ErrorResponseDto,
  SetlistDto,
  UpdateSetlistHeaderRequestDto
} from '../../../../../modules/lpshows-api';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {SetlistEntry} from '../../../../../data/setlists/setlist-entry';
import {
  AddSetlistEntryFormComponent
} from '../add-setlist-entry-form/add-setlist-entry-form.component';
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
import { SetlistsService } from '../../../../../services/setlists.service';
import { ProblemDetailsDto } from '../../../../../modules/lpshows-api/v3';

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

  currentSetlistId = signal<number>(0);

  setlistForm = this.formBuilder.group({
    concertId: new FormControl('', [Validators.required]),
    concertTitle: new FormControl('', [Validators.required]),
    setName: new FormControl('', []),
    linkinpediaUrl: new FormControl('', [Validators.pattern(/^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)/)]),
  });

  isSaving = signal(false);

  isAddingEntry = signal(false);
  isEditingEntry = signal(false);

  isPendingReorder = signal(false);

  isShowingAddEntryDialog = signal(false);
  isShowingEditEntryDialog = signal(false);

  // Setlist entry that will be edited. Is used to store the data for the form
  entryToEdit = signal<SetlistEntry | null>(null);

  // property to show whether the setlist is currently being deleted
  entryDeleting = signal(false);

  setlistEntries = signal<SetlistEntry[]>([]);

  ngOnInit() {
    this.activeRoute.params.subscribe(async params => {
      let setlistId = params['setlistId'];
      if (setlistId != null && setlistId > 0) {
        await this.loadSetlist(setlistId);
        this.currentSetlistId.set(setlistId);
      }
    });
  }

  private async loadSetlist(setlistId: number) {
    let setlistResult = await this.setlistService.getCompleteSetlist(setlistId, false);

    if (setlistResult) {
      this.fillFormFromDto(setlistResult);
    }

    const entries: SetlistEntry[] = setlistResult?.entries?.map(SetlistEntry.fromDto) ?? [];
    this.setlistEntries.set(entries);
  }

  private async reloadCurrentSetlist() {
    await this.loadSetlist(this.currentSetlistId());
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
    const entries = this.setlistEntries();
    return entries
      .sort((a, b) => b.songNumber - a.songNumber)
      .map(e => e.songNumber)
      .at(0) ?? 0;
  }

  async onSaveClicked() {
    this.isSaving.set(true);
    let request = this.makeRequestDtoFromFormData();

    try {
      await this.setlistService.updateSetlistHeader(this.currentSetlistId(), request)

      this.messageService.add({
        severity: 'success',
        summary: 'Successfully updated successfully',
      });
    } catch (err) {
      let errorResponse: ProblemDetailsDto = err as ProblemDetailsDto;
      this.messageService.add({
        severity: 'error',
        summary: 'Could not update setlist',
        detail: errorResponse.detail,
      });
    } finally {
      this.isSaving.set(false);
    }
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
    this.addEntryFormComponent()?.clearForm();
    this.addEntryFormComponent()?.setSongNumber(largestSongNumber + 1);

    this.isShowingAddEntryDialog.set(true);
  }


  async onAddEntryConfirm() {
    this.isAddingEntry.set(true);

    // read the values from the form
    console.debug("Found component: ", this.addEntryFormComponent);
    let formValues = this.addEntryFormComponent()?.readValuesFromForm();
    console.debug("Values read from form: ", formValues);

    if (formValues != null) {
      try {
        await this.setlistService.addSetlistEntry(formValues, this.currentSetlistId());

        this.messageService.add({
          severity: 'success',
          summary: 'Entry was added successfully',
        });

        await this.reloadCurrentSetlist();

        this.dismissAddEntryModal();
      } catch (err) {
        let errorResponse: ProblemDetailsDto = err as ProblemDetailsDto;
        this.messageService.add({
          severity: 'error',
          summary: 'Could not add entry to setlist',
          detail: errorResponse.detail,
        });
        this.dismissAddEntryModal();
      } finally {
        this.isAddingEntry.set(false);
      }
    } else {
      this.messageService.add({
        severity: 'danger',
        summary: 'Failed to read form data!',
      });
      this.isAddingEntry.set(false);
    }
  }


  onRowReordered(event: TableRowReorderEvent) {
    console.debug("RowReordered: ", event);
    this.isPendingReorder.set(true);
  }


  async onSaveOrderClicked() {
    console.debug("Save new order...");
    this.isSaving.set(true);

    try {
      await this.setlistService.applyNewEntryOrder(this.currentSetlistId(), this.setlistEntries().map(e => e.id));

      this.messageService.add({
        severity: 'success',
        summary: 'Successfully reordered setlist entries',
      });
      this.isPendingReorder.set(false);
    } catch (err) {
      let errorResponse: ProblemDetailsDto = err as ProblemDetailsDto;
      this.messageService.add({
        severity: 'error',
        summary: 'Could not reorder setlist',
        detail: errorResponse.detail,
      });
      this.isSaving.set(false);
    } finally {
      this.isPendingReorder.set(false);
    }
  }


  dismissAddEntryModal() {
    this.isShowingAddEntryDialog.set(false);
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
        severity: 'error'
      },

      accept: async () => {
        await this.onDeleteEntryConfirm(entry);
      }
    });
  }


  async onDeleteEntryConfirm(entry: SetlistEntry) {
    let id = entry.id;
    console.debug("Will delete setlist entry: " + id);

    try {
      await this.setlistService.deleteSetlistEntry(this.currentSetlistId(), id);

      console.debug("DELETE setlist request entry finished");
      this.entryDeleting.set(false);
      await this.reloadCurrentSetlist();
    } catch (err) {
      let errorResponse: ErrorResponseDto = err as ErrorResponseDto;
      console.warn("Failed to delete setlist entry:", err);
      this.entryDeleting.set(false);

      this.messageService.add({
        severity: 'danger',
        summary: 'Could not delete setlist entry',
        detail: errorResponse.message,
      });
    }
  }


  onEditEntryClicked(content: AddSetlistEntryFormComponent, entry: SetlistEntry) {
    console.debug("Content: ", content);
    this.entryToEdit.set(entry);

    if (this.entryToEdit() == null) {
      return;
    }

    this.isShowingEditEntryDialog.set(true);
    content?.clearForm();
    content?.loadEntry(this.currentSetlistId(), entry.id);
  }


  async onEditEntryConfirm(content: AddSetlistEntryFormComponent) {
    let entry = this.entryToEdit();
    if (entry == null) {
      return;
    }

    this.isEditingEntry.set(true);

    let id = entry.id;
    console.debug("Will edit setlist entry: " + id);

    let formValues = content.readValuesFromForm();
    console.debug("Values read from form: ", formValues);

    if (formValues != null) {
      try {
        await this.setlistService.updateSetlistEntry(formValues, this.currentSetlistId(), id);

        this.messageService.add({
          severity: 'success',
          summary: 'Entry was updated successfully',
        });

        await this.reloadCurrentSetlist();
      } catch (err) {
        let errorResponse: ErrorResponseDto = err as ErrorResponseDto;
        this.messageService.add({
          severity: 'error',
          summary: 'Could not update setlist entry',
          detail: errorResponse.message,
        });
      } finally {
        this.entryToEdit.set(null);
        this.isEditingEntry.set(false);
      }
    } else {
      this.messageService.add({
        severity: 'error',
        summary: 'Failed to read form data',
      });
      this.isEditingEntry.set(false);
    }
  }


  dismissEditEntryModal() {
    this.isShowingEditEntryDialog.set(false);
    this.entryToEdit.set(null);
  }
}
