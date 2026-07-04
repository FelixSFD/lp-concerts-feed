import {Component, inject, OnInit} from '@angular/core';
import {SongsService} from '../../../../../services/songs.service';
import {ErrorResponseDto, SongMashupDto} from '../../../../../modules/lpshows-api';
import {RouterLink} from '@angular/router';
import {Button} from 'primeng/button';
import {ButtonGroup} from 'primeng/buttongroup';
import {Card} from 'primeng/card';
import {ConfirmDialog} from 'primeng/confirmdialog';
import {IconField} from 'primeng/iconfield';
import {InputIcon} from 'primeng/inputicon';
import {InputText} from 'primeng/inputtext';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {TableModule} from 'primeng/table';
import {ConfirmationService, MessageService} from 'primeng/api';

@Component({
  selector: 'app-manage-mashups-page',
  imports: [
    RouterLink,
    Button,
    ButtonGroup,
    Card,
    ConfirmDialog,
    IconField,
    InputIcon,
    InputText,
    ReactiveFormsModule,
    TableModule,
    FormsModule
  ],
  templateUrl: './manage-mashups-page.component.html',
  styleUrl: './manage-mashups-page.component.css',
})
export class ManageMashupsPageComponent implements OnInit {
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  private songsService = inject(SongsService);

  mashups$: SongMashupDto[] = [];

  isDeletingMashup$ = false;

  // true while data is being loaded
  isLoading$ = false;

  globalSearchText$: string = "";


  ngOnInit() {
    this.reloadList(false);
  }


  onDeleteMashupClicked(event: Event, mashup: SongMashupDto) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `Do you want to delete the mashup "${mashup.title}"?`,
      header: 'Delete Mashup',
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
        this.onDeleteMashupConfirm(mashup);
      }
    });
  }


  onDeleteMashupConfirm(mashup: SongMashupDto) {
    this.isDeletingMashup$ = true;

    if (mashup) {
      this.songsService.deleteMashup(mashup.id!)
        .subscribe({
          next: () => {
            this.reloadList(false);
            this.isDeletingMashup$ = false;
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            this.messageService.add({
              severity: "danger",
              summary: "Could not delete mashup!",
              text: errorResponse.message,
            });
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
        this.messageService.add({
          severity: "danger",
          summary: "Could not load mashups!",
          text: errorResponse.message,
        });
      }
    })
  }
}
