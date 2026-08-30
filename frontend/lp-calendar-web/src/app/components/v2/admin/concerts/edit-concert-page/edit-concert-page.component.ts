import { Component, inject, OnInit, viewChild } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ConcertsApi, ConcertDetailsDto } from '../../../../../modules/lpshows-api/v3';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { ConcertFormComponent } from '../concert-form/concert-form.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { TableModule } from 'primeng/table';
import { Divider } from 'primeng/divider';

@Component({
  selector: 'app-edit-concert-page',
  imports: [
    Button,
    Card,
    ConfirmDialog,
    RouterLink,
    TableModule,
    ConcertFormComponent,
    Divider
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
      const concert = data['concert'] as ConcertDetailsDto | null;
      if (!concert || concert.id == null) {
        this.messageService.add({
          severity: 'error',
          summary: 'Failed to load concert',
          detail: (concert as ErrorResponseDto).message,
          sticky: true,
        });

        this.concertFormComponent()?.concertForm.disable();
        return;
      }

      this.currentConcertId = concert.id ?? null;
      this.concertFormComponent()?.fillFormWith(concert);
    });
  }
}
