import {DatePipe} from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import {FormsModule} from '@angular/forms';
import {RouterLink} from '@angular/router';
import {MessageService} from 'primeng/api';
import {Button} from 'primeng/button';
import {ButtonGroup} from 'primeng/buttongroup';
import {Card} from 'primeng/card';
import {IconField} from 'primeng/iconfield';
import {InputIcon} from 'primeng/inputicon';
import {InputText} from 'primeng/inputtext';
import {TableModule} from 'primeng/table';
import {ConcertDto, ErrorResponseDto} from '../../../../modules/lpshows-api';
import {ConcertTitleGenerator} from '../../../../data/concert-title-generator';
import {ConcertsService} from '../../../../services/concerts.service';
import { DateTime } from 'luxon';
import { ConcertFilter } from '../../../../data/concert-filter';

@Component({
  selector: 'app-manage-concerts-page',
  imports: [
    Button,
    ButtonGroup,
    Card,
    DatePipe,
    FormsModule,
    IconField,
    InputIcon,
    InputText,
    RouterLink,
    TableModule,
  ],
  templateUrl: './manage-concerts-page.component.html',
  styleUrl: './manage-concerts-page.component.css',
})
export class ManageConcertsPageComponent implements OnInit {
  private readonly concertsService = inject(ConcertsService);
  private readonly messageService = inject(MessageService);

  concertsOld$ = signal<ConcertDto[]>([]);
  isLoading$ = false;
  globalSearchText$ = '';

  ngOnInit() {
    this.reloadList();
  }

  getTitle(concert: ConcertDto): string {
    return ConcertTitleGenerator.getTitleFor(concert);
  }

  private reloadList() {
    this.isLoading$ = true;
    let allConcertsFilter: ConcertFilter = {
      dateFrom: DateTime.fromMillis(0, {zone: 'UTC'}),
      dateTo: null,
      tour: null,
      onlyFuture: false
    };
    this.concertsService.getFilteredConcerts(allConcertsFilter, false).subscribe({
      next: concerts => {
        console.debug('Loaded concerts:', concerts);
        this.concertsOld$.set(concerts);
        this.isLoading$ = false;
      },
      error: err => {
        const errorResponse: ErrorResponseDto = err.error;
        this.isLoading$ = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Could not load concerts!',
          text: errorResponse?.message,
        });
      },
    });
  }
}
