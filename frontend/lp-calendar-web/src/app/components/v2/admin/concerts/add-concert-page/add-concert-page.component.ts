import { Component, inject, OnInit, signal, viewChild } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ConcertDetailsDto, ConcertsApi, CreateConcertRequestDto } from '../../../../../modules/lpshows-api/v3';
import { ErrorResponseDto } from '../../../../../modules/lpshows-api';
import { ConcertFormComponent, ConcertFormContent } from '../concert-form/concert-form.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Divider } from 'primeng/divider';

@Component({
  selector: 'app-add-concert-page',
  imports: [
    Button,
    Card,
    RouterLink,
    ConcertFormComponent,
    Divider
  ],
  templateUrl: './add-concert-page.component.html',
  styleUrl: './add-concert-page.component.css',
})
export class AddConcertPageComponent implements OnInit {
  private router = inject(Router);
  private activeRoute = inject(ActivatedRoute);
  private messageService = inject(MessageService);
  private concertsApi = inject(ConcertsApi);

  isSaving = signal(false);

  concertFormComponent = viewChild(ConcertFormComponent);

  ngOnInit() {
    this.activeRoute.queryParams.subscribe(params => {
      console.debug("Query params:", params);
      let wikiPageId = params["wikiPageId"] as string | null | undefined;
      if (wikiPageId) {
        this.concertFormComponent()?.setWikiPageId(wikiPageId);
        this.concertFormComponent()?.importFromLinkinpediaUrlClicked();
      }
    });
  }

  onSaveClicked(formContent: ConcertFormContent) {
    this.isSaving.set(true);

    const request: CreateConcertRequestDto = {
      status: formContent.status,
      customTitle: formContent.customTitle ?? undefined,
      concertTypeId: formContent.concertTypeId != null ? formContent.concertTypeId : undefined,
      tourId: formContent.tourId ?? undefined,
      tourLegId: formContent.tourLegId ?? undefined,
      venueId: formContent.venueId ?? undefined,
      postedStartTime: formContent.postedStartTime.toISO()!,
      timeIsPlaceholder: formContent.timeIsPlaceholder ?? undefined,
      doorsTime: formContent.doorsTime?.toISO() ?? undefined,
      mainStageTime: formContent.mainStageTime?.toISO() ?? undefined,
      expectedSetDurationMinutes: formContent.expectedSetDuration ?? undefined,
      linkinpediaUrl: formContent.linkinpediaUrl ?? undefined,
    };

    this.concertsApi.createConcert(request).subscribe({
      next: (createdConcert: any) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Successfully created concert',
        });
        this.router.navigate(['/', 'admin', 'concerts', createdConcert?.id ?? '']);
      },
      error: (err) => {
        const errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({
          severity: 'error',
          summary: 'Failed to create concert',
          detail: errorResponse?.message ?? err?.message,
        });
        this.isSaving.set(false);
      },
    });
  }
}
