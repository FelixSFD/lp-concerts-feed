import { DatePipe, NgTemplateOutlet } from '@angular/common';
import { Component, effect, inject, OnInit, signal } from '@angular/core';
import {FormsModule} from '@angular/forms';
import {RouterLink} from '@angular/router';
import { ConfirmationService, FilterMetadata, MessageService } from 'primeng/api';
import { Button, ButtonDirective } from 'primeng/button';
import {ButtonGroup} from 'primeng/buttongroup';
import {Card} from 'primeng/card';
import {IconField} from 'primeng/iconfield';
import {InputIcon} from 'primeng/inputicon';
import {InputText} from 'primeng/inputtext';
import { TableFilterEvent, TableLazyLoadEvent, TableModule } from 'primeng/table';
import { ConcertDto, ConcertStatusValueDto, ErrorResponseDto } from '../../../../modules/lpshows-api';
import {ConcertTitleGenerator} from '../../../../data/concert-title-generator';
import {LegacyConcertsService} from '../../../../services/legacy-concerts.service';
import { DateTime } from 'luxon';
import { ConcertFilter } from '../../../../data/concert-filter';
import {
  ConcertDetailsDto,
  LinkinpediaImportConcertStatusDto,
  LinkinpediaImportStatusDto
} from '../../../../modules/lpshows-api/v3';
import { ToursService } from '../../../../services/tours.service';
import { Divider } from 'primeng/divider';
import { ConcertStatus } from '../../../../data/concert-status';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { AuthService } from '../../../../auth/auth.service';
import { Panel } from 'primeng/panel';
import { ConcertsService } from '../../../../services/concerts.service';
import { MeterGroup, MeterItem } from 'primeng/metergroup';
import { Badge } from 'primeng/badge';
import { makeRealArray } from '../../../../helper/array-helper';

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
    Divider,
    ConfirmDialog,
    Panel,
    MeterGroup,
    ButtonDirective,
    Badge,
    NgTemplateOutlet,
  ],
  templateUrl: './manage-concerts-page.component.html',
  styleUrl: './manage-concerts-page.component.css',
})
export class ManageConcertsPageComponent implements OnInit {
  private readonly legacyConcertsService = inject(LegacyConcertsService);
  private readonly concertsService = inject(ConcertsService);
  private readonly toursService = inject(ToursService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);
  protected readonly authService = inject(AuthService);

  concertsOld$ = signal<ConcertDto[]>([]);
  concerts$ = signal<ConcertDetailsDto[]>([]);
  totalConcertCount$ = signal(0);
  isDeletingConcert$ = signal(false);
  concertImportStats$ = signal<LinkinpediaImportStatusDto | null>(null);
  isLoadingImportStats$ = signal(false);

  concertImportStatusMeterGroup$ = signal<MeterItem[]>([
    { label: `Fully imported (${this.concertImportStats$()?.importedWithSetlistCount ?? 0})`, value: 0, color: '#10B981' },
    { label: `Imported without setlist (${this.concertImportStats$()?.importedWithoutSetlistCount ?? 0})`, value: 0, color: '#EAB308' },
    { label: `Not imported yet (${this.concertImportStats$()?.notImportedCount ?? 0})`, value: 0, color: 'var(--p-surface-300)' },
  ]);

  isLoadingOld$ = signal(false);
  isLoading$ = signal(false);
  globalSearchTextOld$ = signal("");
  globalSearchText$ = signal("");
  globalSearchTextImportStatus$ = signal("");

  private currentOffset: number | null = null;
  private currentLimit: number | null = null;
  private currentSortFields: string[] = [];
  private currentSortOrder: number | null = null;
  private currentFilter: ConcertFilter = {
    dateFrom: DateTime.fromMillis(0, {zone: 'UTC'}),
    dateTo: null,
    tour: null,
    orderBy: undefined,
    onlyFuture: false
  };

  private updateImportStatsEffect = effect(() => {
    this.concertImportStatusMeterGroup$.update(stats => {
      console.debug("Updating stats...");

      let notImportedCount = this.concertImportStats$()?.notImportedCount ?? 0;
      let importedCount = this.concertImportStats$()?.importedWithSetlistCount ?? 0;
      let importedWithoutSetlistCount = this.concertImportStats$()?.importedWithoutSetlistCount ?? 0;
      let totalCount = notImportedCount + importedCount + importedWithoutSetlistCount;

      let importedPercentage = importedCount / totalCount;
      let importedWithoutSetlistPercentage = importedWithoutSetlistCount / totalCount;
      let notImportedPercentage = notImportedCount / totalCount;

      console.debug("Imported percentage:", importedPercentage, "Without setlist:", importedWithoutSetlistPercentage, "Not imported:", notImportedPercentage);

      return [
        { label: `Fully imported (${importedCount})`, value: importedPercentage * 100, color: '#10B981'},
        { label: `Imported without setlist (${importedWithoutSetlistCount})`, value: importedWithoutSetlistPercentage * 100, color: '#EAB308' },
        { label: `Not imported yet (${notImportedCount})`, value: notImportedPercentage * 100, color: 'var(--p-surface-300)' },
      ];
    });
  });

  ngOnInit() {
    //this.reloadList();
    this.reloadConcertImportStats();
  }

  getTitle(concert: ConcertDto): string {
    return ConcertTitleGenerator.getTitleFor(concert);
  }

  async loadConcertsLazy(event: TableLazyLoadEvent) {
    console.debug("loadConcertsLazy", event);

    let sortFields = event.sortField == null
      ? []
      : Array.isArray(event.sortField)
        ? event.sortField
        : [event.sortField];
    sortFields = sortFields.map(f => `${(event.sortOrder ?? 0) == -1 ? '-' : ''}${f}`)

    let concertFilter = this.makeConcertFilter(event.filters, sortFields);

    if (this.isLoading$()) {
      console.debug("loadConcertsLazy: skipping because it's already loading.");
      return;
    }
    if (this.currentOffset === event.first
      && this.currentLimit === event.rows
      && this.currentSortOrder == event.sortOrder
      && this.currentSortFields.join(",") === sortFields.join(",")
      && this.currentFilter == concertFilter
    ) {
      console.debug("loadConcertsLazy: skipping because it's already loaded.");
      return;
    }

    this.isLoading$.set(true);
    console.debug("loadConcertsLazy: sortFields", sortFields);

    try {
      let response = await this.concertsService.getFilteredConcerts(concertFilter ?? this.currentFilter, event.rows ?? 100, event.first);
      this.totalConcertCount$.set(response.metadata?.totalElements ?? 0);
      this.concerts$.set(response.concerts ?? []);
      this.currentOffset = event.first ?? null;
      this.currentLimit = event.rows ?? null;
      this.currentSortFields = sortFields;
      this.currentSortOrder = event.sortOrder ?? null;
    } catch (err) {
      console.error('Could not load concerts', err);
      this.messageService.add({
        severity: 'error',
        summary: 'Could not load concerts',
      });
    } finally {
      this.isLoading$.set(false);
    }
  }

  private makeConcertFilter(filter: {[p: string]: FilterMetadata | FilterMetadata[] | undefined} | undefined, orderBy: string[] | undefined): ConcertFilter | null {
    if (filter) {
      let countryFilter = makeRealArray(filter["country"]).pop() ?? null;

      return {
        countryCode: undefined,
        country: countryFilter?.value,
        tour: undefined,
        onlyFuture: false,
        dateFrom: null,
        dateTo: null,
        orderBy: orderBy,
      };
    } else {
      return null;
    }
  }

  private reloadConcertImportStats() {
    this.isLoadingImportStats$.set(true);
    this.concertsService.getLinkinpediaImportStatus()
      .then(stats => {
        this.concertImportStats$.set(stats);
      })
      .catch(err => {
        console.error('Could not load concert import stats', err);
      })
      .finally(() => {
        this.isLoadingImportStats$.set(false);
      });
  }

  private reloadList() {
    this.isLoadingOld$.set(true);
    this.isLoading$.set(true);
    let allConcertsFilter: ConcertFilter = {
      dateFrom: DateTime.fromMillis(0, {zone: 'UTC'}),
      dateTo: null,
      tour: null,
      onlyFuture: false
    };
    this.legacyConcertsService.getFilteredConcerts(allConcertsFilter, false).subscribe({
      next: concerts => {
        console.debug('Loaded OLD concerts:', concerts);
        this.concertsOld$.set(concerts);
        this.isLoadingOld$.set(false);
      },
      error: err => {
        const errorResponse: ErrorResponseDto = err.error;
        this.isLoadingOld$.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Could not load concerts in old database!',
          text: errorResponse?.message,
        });
      },
    });

    this.toursService.getFilteredConcerts(allConcertsFilter, false).subscribe({
      next: concerts => {
        console.debug('Loaded concerts:', concerts);
        this.concerts$.set(concerts);
        this.isLoading$.set(false);
      },
      error: err => {
        const errorResponse: ErrorResponseDto = err.error;
        this.isLoading$.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Could not load concerts!',
          text: errorResponse?.message,
        });
      },
    });
  }

  async onDeleteClicked(event: MouseEvent, concert: ConcertDetailsDto) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `Do you really want to delete the concert on "${concert.postedStartTime}" in "${concert.venue.city.name}"?`,
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
      accept: async () => {
        this.isDeletingConcert$.set(true);
        try {
          await this.toursService.deleteConcert(concert.id);
        } catch (e) {
          console.error('Could not delete concert', e);
        } finally {
          this.reloadList();
          this.isDeletingConcert$.set(false);
        }
      },
      reject: () => {},
    });
  }

  protected readonly ConcertStatusValueDto = ConcertStatusValueDto;
  protected readonly ConcertStatus = ConcertStatus;
  protected readonly LinkinpediaImportConcertStatusDto = LinkinpediaImportConcertStatusDto;
}
