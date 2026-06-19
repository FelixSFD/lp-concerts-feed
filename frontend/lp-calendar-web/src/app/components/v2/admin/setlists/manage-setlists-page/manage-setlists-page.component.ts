import {Component, inject, OnInit} from '@angular/core';
import {RouterLink} from '@angular/router';
import {SetlistsService} from '../../../../../services/setlists.service';
import {Setlist} from '../../../../../data/setlists/setlist';
import {ErrorResponseDto} from '../../../../../modules/lpshows-api';
import {Card} from 'primeng/card';
import {TableModule} from 'primeng/table';
import {Button} from 'primeng/button';
import {InputText} from 'primeng/inputtext';
import {IconField} from 'primeng/iconfield';
import {InputIcon} from 'primeng/inputicon';
import {ButtonGroup} from 'primeng/buttongroup';
import {ConfirmationService, MessageService} from 'primeng/api';
import {FormsModule} from '@angular/forms';
import {ConfirmDialog} from 'primeng/confirmdialog';

@Component({
  selector: 'app-manage-setlists-page',
  imports: [
    RouterLink,
    Card,
    TableModule,
    Button,
    InputText,
    IconField,
    InputIcon,
    ButtonGroup,
    FormsModule,
    ConfirmDialog
  ],
  templateUrl: './manage-setlists-page.component.html',
  styleUrl: './manage-setlists-page.component.css',
})
export class ManageSetlistsPageComponent implements OnInit {
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  setlists$: Setlist[] = [];

  // property to show whether the setlist is currently being deleted
  setlistDeleting$ = false;

  // true while data is being loaded
  isLoading$ = false;

  globalSearchText$: string = "";

  constructor(private setlistService: SetlistsService) {
  }


  ngOnInit() {
    this.reloadList(true);
  }


  private reloadList(cache: boolean) {
    this.isLoading$ = true;
    this.setlistService.getSetlists(cache).subscribe({
      next: res => {
        this.setlists$ = res.map(setlist => Setlist.fromDto(setlist));
        this.isLoading$ = false;
      },
      error: err => {
        this.isLoading$ = false;
      }
    });
  }

  onDeleteSetlistClicked(event: Event, setlist: Setlist) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `Do you want to delete the setlist for "${setlist.concertTitle}"?`,
      header: 'Delete Setlist',
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
        this.onDeleteSetlistConfirm(setlist);
      }
    });
  }


  onDeleteSetlistConfirm(setlist: Setlist) {
    this.setlistDeleting$ = true;
    if (setlist == null) {
      this.confirmationService.close();
      return;
    }

    let id = setlist.id;
    console.debug("Will delete setlist: " + id);

    this.setlistService.deleteSetlist(id).subscribe({
      next: result => {
        console.debug("DELETE setlist request finished");
        console.debug(result);

        this.reloadList(false);
        this.setlistDeleting$ = false;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        console.warn("Failed to delete setlist:", err);
        this.setlistDeleting$ = false;

        this.messageService.add({
          severity: "danger",
          summary: "Could not delete setlist!",
          text: errorResponse.message,
        });
      }
    });
  }
}
