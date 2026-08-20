import { Component, inject, OnInit, signal } from '@angular/core';
import { ConfirmationService, MessageService } from 'primeng/api';
import { RouterLink } from '@angular/router';
import { ConcertsApi, ConcertDetailsDto } from '../../../../../modules/lpshows-api/v3';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { Button } from 'primeng/button';
import { ButtonGroup } from 'primeng/buttongroup';
import { Card } from 'primeng/card';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { InputText } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';

@Component({
  selector: 'app-manage-concerts-page',
  imports: [
    Button,
    ButtonGroup,
    Card,
    ConfirmDialog,
    FormsModule,
    IconField,
    InputIcon,
    InputText,
    RouterLink,
    TableModule
  ],
  templateUrl: './manage-concerts-page.component.html',
  styleUrl: './manage-concerts-page.component.css',
})
export class ManageConcertsPageComponent implements OnInit {
  private concertsApi = inject(ConcertsApi);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);

  concerts$ = signal<ConcertDetailsDto[]>([]);
  isLoading$ = signal<boolean>(true);
  isDeletingConcert$ = signal<boolean>(false);
  globalSearchText$ = signal<string>('');

  ngOnInit() {
    this.loadConcerts();
  }

  loadConcerts() {
    this.isLoading$.set(true);
    this.concertsApi.getConcerts().subscribe({
      next: (concerts) => {
        this.concerts$.set(concerts);
        this.isLoading$.set(false);
      },
      error: (err) => {
        console.error('Failed to load concerts', err);
        this.messageService.add({
          severity: 'error',
          summary: 'Failed to load concerts',
          detail: err?.error?.message ?? err?.message,
        });
        this.isLoading$.set(false);
      },
    });
  }

  onDeleteConcertClicked(event: MouseEvent, concert: ConcertDetailsDto) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `Do you really want to delete the concert "${concert.customTitle ?? concert.venue?.currentName ?? concert.id}"?`,
      header: 'Delete concert',
      icon: 'pi pi-info-circle',
      rejectLabel: 'Cancel',
      rejectButtonProps: {
        label: 'Cancel',
        severity: 'secondary',
        outlined: true,
      },
      acceptButtonProps: {
        label: 'Delete',
        severity: 'danger',
      },
      accept: () => {
        this.isDeletingConcert$.set(true);
        this.concertsApi.deleteConcertById(concert.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Successfully deleted concert',
            });
            this.isDeletingConcert$.set(false);
            this.loadConcerts();
          },
          error: (err) => {
            let errorResponse: ErrorResponseDto = err.error;
            this.messageService.add({
              severity: 'error',
              summary: 'Failed to delete concert',
              detail: errorResponse?.message ?? err?.message,
            });
            this.isDeletingConcert$.set(false);
          },
        });
      },
      reject: () => {},
    });
  }
}
