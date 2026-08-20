import { Component, inject, OnInit, viewChild } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ConcertsApi, ConcertDetailsDto } from '../../../../../modules/lpshows-api/v3';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { ConcertFormComponent, ConcertFormContent } from '../concert-form/concert-form.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { TableModule } from 'primeng/table';

@Component({
  selector: 'app-edit-concert-page',
  imports: [
    Button,
    Card,
    ConfirmDialog,
    RouterLink,
    TableModule,
    ConcertFormComponent
  ],
  templateUrl: './edit-concert-page.component.html',
  styleUrl: './edit-concert-page.component.css',
})
export class EditConcertPageComponent implements OnInit {
  private activeRoute = inject(ActivatedRoute);
  private messageService = inject(MessageService);
  private concertsApi = inject(ConcertsApi);
  private confirmationService = inject(ConfirmationService);

  concertFormComponent = viewChild(ConcertFormComponent);

  currentConcertId: string = '';
  currentConcertTitle: string = '';

  isSaving$ = false;

  ngOnInit() {
    this.activeRoute.data.subscribe((data) => {
      const concert: ConcertDetailsDto | ErrorResponseDto = data['concert'];
      if (!concert) {
        return;
      }

      if ('type' in concert && concert.type === 'ErrorResponseDto') {
        this.messageService.add({
          severity: 'error',
          summary: 'Failed to load concert',
          detail: (concert as ErrorResponseDto).message,
          sticky: true,
        });
        return;
      }

      const concertDto = concert as ConcertDetailsDto;
      this.currentConcertId = concertDto.id ?? '';
      this.currentConcertTitle = concertDto.customTitle ?? concertDto.venue?.currentName ?? '';
      this.concertFormComponent()?.fillFormWith(concertDto);
    });
  }

  reloadConcert() {
    if (!this.currentConcertId) {
      return;
    }
    this.concertsApi.getConcertById(this.currentConcertId).subscribe({
      next: (concert) => {
        this.currentConcertTitle = concert.customTitle ?? concert.venue?.currentName ?? '';
        this.concertFormComponent()?.fillFormWith(concert);
      },
      error: (err) => {
        console.error('Failed to reload concert', err);
      },
    });
  }
}
