import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ConcertStatusValueDto } from '../../../modules/lpshows-api';
import { AuthService } from '../../../auth/auth.service';
import { ConcertFilter } from '../../../data/concert-filter';
import { DateTime } from "luxon";
import { ConcertTitleGenerator } from "../../../data/concert-title-generator";
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Skeleton } from 'primeng/skeleton';
import { ConcertBadgesComponent } from '../concert-badges/concert-badges.component';
import { ConcertBadgesData } from '../concert-details/concert-details.view-model';
import { CountdownComponent } from '../countdown/countdown.component';
import { ButtonGroup } from 'primeng/buttongroup';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Tooltip } from 'primeng/tooltip';
import { ConcertFilterComponent } from '../concert-filter/concert-filter.component';
import { ConcertDetailsDto } from '../../../modules/lpshows-api/v3';
import { ConcertsService } from '../../../services/concerts.service';
import { ToursService } from '../../../services/tours.service';
import { Scroller, ScrollerLazyLoadEvent } from 'primeng/scroller';

@Component({
  selector: 'app-concert-list',
  imports: [
    RouterLink,
    Button,
    Card,
    Skeleton,
    ConcertBadgesComponent,
    CountdownComponent,
    ButtonGroup,
    ConfirmDialog,
    Tooltip,
    ConcertFilterComponent,
    Scroller,
  ],
  templateUrl: './concert-list.component.html',
  styleUrl: './concert-list.component.css',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class ConcertListComponent implements OnInit {
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);
  private concertsService = inject(ConcertsService);
  private toursService = inject(ToursService);

  concerts$ = signal<(ConcertDetailsDto | undefined)[]>([]);

  // status if the table is currently loading
  isLoading$ = signal(false);

  // property to indicate if all concerts or only future should be displayed
  showHistoricConcerts$ = signal(false);

  // property to show whether the concert is currently being deleted
  concertDeleting$ = signal(false);

  // Service to check auth information
  private readonly authService = inject(AuthService);

  canAddConcerts$ = signal(false);
  canUpdateConcerts$ = signal(false);
  canDeleteConcerts$ = signal(false);
  canManageSetlists$ = signal(false);

  // default filter that is used when loading the list
  defaultFilter: ConcertFilter = {
    onlyFuture: true,
    tour: null,
    dateFrom: DateTime.now().set({hour: 0, minute: 0, second: 0, millisecond: 0}),
    dateTo: null,
    orderBy: ["-date"]
  };

  // Filter that is used for loading the list
  currentFilter: ConcertFilter = this.defaultFilter;

  constructor() {
    this.authService.canAddConcerts.subscribe(hasPermission => {
      this.canAddConcerts$.set(hasPermission);
    });
    this.authService.canUpdateConcerts.subscribe(hasPermission => {
      this.canUpdateConcerts$.set(hasPermission);
    });
    this.authService.canDeleteConcerts.subscribe(hasPermission => {
      this.canDeleteConcerts$.set(hasPermission);
    });
    this.authService.canManageSetlists.subscribe(hasPermission => {
      this.canManageSetlists$.set(hasPermission);
    });
  }

  ngOnInit(): void {
    this.reloadConcertList(true);
  }

  public async reloadConcertList(cache: boolean = true) {
    this.isLoading$.set(true);
    try {
      const response = await this.concertsService.getFilteredConcerts(this.currentFilter, 20, 0, cache);
      const total = response.metadata?.totalElements ?? response.concerts?.length ?? 0;
      const items: (ConcertDetailsDto | undefined)[] = Array.from({ length: total });
      const loaded = response.concerts ?? [];
      for (let i = 0; i < loaded.length; i++) {
        items[i] = loaded[i];
      }
      this.concerts$.set(items);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : String(err);
      this.messageService.add({
        severity: "danger",
        summary: "Failed to load concerts",
        text: message,
      });
    } finally {
      this.isLoading$.set(false);
    }
  }

  async onLazyLoad(event: ScrollerLazyLoadEvent) {
    const first = typeof event.first === 'number' ? event.first : 0;
    const last = typeof event.last === 'number' ? event.last : first + 10;
    const limit = Math.max(last - first, 1);
    const skip = first;

    const current = this.concerts$();
    const needLoad = current.length === 0 || current.slice(first, last).some(item => item === undefined);
    if (!needLoad) {
      return;
    }

    this.isLoading$.set(true);
    try {
      const response = await this.concertsService.getFilteredConcerts(this.currentFilter, limit, skip);
      const total = response.metadata?.totalElements ?? response.concerts?.length ?? 0;
      let items = this.concerts$();
      if (items.length !== total) {
        items = Array.from({ length: total });
      } else {
        items = [...items];
      }
      const loaded = response.concerts ?? [];
      for (let i = 0; i < loaded.length; i++) {
        items[skip + i] = loaded[i];
      }
      this.concerts$.set(items);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : String(err);
      this.messageService.add({
        severity: "danger",
        summary: "Failed to load concerts",
        text: message,
      });
    } finally {
      this.isLoading$.set(false);
    }
  }

  onShowHistoricSwitchChanged() {
    this.reloadConcertList(true);
    console.log("Show historic: " + this.showHistoricConcerts$())
  }

  onDeleteConcertClicked(event: Event, concertId: string | null) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: 'Do you want to delete this concert?',
      header: 'Delete Concert',
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
        this.onDeleteConcertConfirm(concertId);
      }
    });
  }

  private async onDeleteConcertConfirm(concertId: string | null) {
    this.concertDeleting$.set(true);
    if (concertId == null) {
      this.concertDeleting$.set(false);
      return;
    }

    const id = concertId;
    console.debug("Will delete concert: " + id);

    try {
      await this.toursService.deleteConcert(id);
      console.debug("DELETE concert request finished");
      this.reloadConcertList(false);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : String(err);
      console.warn("Failed to delete concert:", err);
      this.messageService.add({
        severity: "danger",
        summary: "Could not delete concert!",
        text: message,
      });
    } finally {
      this.concertDeleting$.set(false);
    }
  }

  onFilterChanged(filter: ConcertFilter) {
    console.debug("filterEvent: ", filter);
    this.currentFilter = filter;
    this.reloadConcertList(true);
  }

  public getDateTime(inputDate: string) {
    return DateTime.fromISO(inputDate, {setZone: false});
  }

  public getDateTimeInTimezone(inputDate: string, timeZoneId: string) {
    return DateTime.fromISO(inputDate, {zone: timeZoneId});
  }

  public isPast(concert: ConcertDetailsDto): boolean {
    const dt = concert.mainStageTime ?? concert.postedStartTime;
    if (!dt) {
      return false;
    }
    return DateTime.fromISO(dt, {setZone: false}) < DateTime.now();
  }

  public getBadgesData(concert: ConcertDetailsDto): ConcertBadgesData {
    return {
      concertStatus: concert.status,
      showType: concert.concertType?.name,
      mainStageTime: concert.mainStageTime ? DateTime.fromISO(concert.mainStageTime, { setZone: false }) : undefined,
      lpuEarlyEntryConfirmed: concert.lpuEarlyEntryConfirmed,
      lpuEarlyEntryTime: concert.lpuEarlyEntryTime ? DateTime.fromISO(concert.lpuEarlyEntryTime, { setZone: false }) : undefined,
    };
  }

  protected readonly DateTime = DateTime;
  protected readonly ConcertTitleGenerator = ConcertTitleGenerator;
  protected readonly ConcertStatusValueDto = ConcertStatusValueDto;
}
