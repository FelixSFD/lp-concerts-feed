import {Component, inject, TemplateRef} from '@angular/core';
import {NgbModal, NgbModalRef} from '@ng-bootstrap/ng-bootstrap';
import {ConcertDto, ConcertStatusValueDto, ErrorResponseDto} from '../../../modules/lpshows-api';
import {AuthService} from '../../../auth/auth.service';
import {ConcertFilter} from '../../../data/concert-filter';
import {ConcertsService} from '../../../services/concerts.service';
import {ToastrService} from 'ngx-toastr';
import { DateTime } from "luxon";
import { ConcertTitleGenerator } from "../../../data/concert-title-generator";
import { listOfTours } from "../../../app.config";
import {Message} from 'primeng/message';
import {RouterLink} from '@angular/router';
import {Button} from 'primeng/button';
import {DataView} from 'primeng/dataview';
import {Card} from 'primeng/card';
import {Skeleton} from 'primeng/skeleton';
import {ConcertBadgesComponent} from '../concert-badges/concert-badges.component';
import {CountdownComponent} from '../countdown/countdown.component';
import {ButtonGroup} from 'primeng/buttongroup';
import {ConfirmDialog} from 'primeng/confirmdialog';
import {ConfirmationService} from 'primeng/api';
import {Tooltip} from 'primeng/tooltip';
import {ConcertFilterComponent} from '../concert-filter/concert-filter.component';

@Component({
  selector: 'app-concert-list',
  imports: [
    Message,
    RouterLink,
    Button,
    Card,
    Skeleton,
    ConcertBadgesComponent,
    CountdownComponent,
    ButtonGroup,
    ConfirmDialog,
    Tooltip,
    ConcertFilterComponent
  ],
  templateUrl: './concert-list.component.html',
  styleUrl: './concert-list.component.css',
})
export class ConcertListComponent {
  private modalService = inject(NgbModal);
  private confirmationService = inject(ConfirmationService);

  concerts$: ConcertDto[] = [];

  // status if the table is currently loading
  isLoading$ = false;

  // property to indicate if all concerts or only future should be displayed
  showHistoricConcerts$ = false;

  // property to show whether the form is currently being sent to the server
  addConcertFormSaving$ = false;

  // property to show whether the concert is currently being deleted
  concertDeleting$ = false;

  // if open, the modal is referenced here
  deleteConcertModal: NgbModalRef | undefined;

  // ID that will be deleted. Is used to store the ID for the confirmation modal
  private concertIdToDelete: string | undefined;

  // Service to check auth information
  private readonly authService = inject(AuthService);

  canAddConcerts$ = false;
  canUpdateConcerts$ = false;
  canDeleteConcerts$ = false;
  canManageSetlists$ = false;

  // default filter that is used when loading the list
  defaultFilter: ConcertFilter = {
    onlyFuture: true,
    tour: null,
    dateFrom: DateTime.now().set({hour: 0, minute: 0, second: 0, millisecond: 0}),
    dateTo: null
  };

  // Filter that is used for loading the list
  currentFilter: ConcertFilter = this.defaultFilter;

  constructor(private concertsService: ConcertsService, private toastr: ToastrService) {
    this.reloadConcertList(true);

    this.authService.canAddConcerts.subscribe(hasPermission => {
      this.canAddConcerts$ = hasPermission;
    });
    this.authService.canUpdateConcerts.subscribe(hasPermission => {
      this.canUpdateConcerts$ = hasPermission;
    });
    this.authService.canDeleteConcerts.subscribe(hasPermission => {
      this.canDeleteConcerts$ = hasPermission;
    });
    this.authService.canManageSetlists.subscribe(hasPermission => {
      this.canManageSetlists$ = hasPermission;
    });
  }


  ngOnInit(): void {
  }


  private reloadConcertList(cache: boolean) {
    this.isLoading$ = true;
    this.concertsService.getFilteredConcerts(this.currentFilter, cache).subscribe({
      next: result => {
        this.concerts$ = result;
        this.isLoading$ = false;
      },
      error: err => {
        this.toastr.error(err.message, "Failed to load concerts");
      }
    })
  }


  openModal(content: TemplateRef<any>) {
    return this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
  }


  onShowHistoricSwitchChanged() {
    this.reloadConcertList(true);
    console.log("Show historic: " + this.showHistoricConcerts$)
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


  private onDeleteConcertConfirm(concertId: string | null) {
    this.concertDeleting$ = true;
    if (concertId == null) {
      this.deleteConcertModal?.dismiss();
      return;
    }

    let id = concertId!;
    console.debug("Will delete concert: " + id);

    this.concertsService.deleteConcert(id).subscribe({
      next: result => {
        console.debug("DELETE concert request finished");
        console.debug(result);

        this.deleteConcertModal?.dismiss();
        this.concertDeleting$ = false;
        this.reloadConcertList(false);
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        console.warn("Failed to delete concert:", err);
        this.deleteConcertModal?.dismiss();
        this.concertDeleting$ = false;

        this.toastr.error(errorResponse.message, "Could not delete concert!");
      }
    });
  }


  dismissConcertConfirmModal() {
    this.deleteConcertModal?.dismiss();
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


  protected readonly DateTime = DateTime;
  protected readonly listOfTours = listOfTours;
  protected readonly ConcertTitleGenerator = ConcertTitleGenerator;
  protected readonly ConcertStatusValueDto = ConcertStatusValueDto;
}
