import {Component, inject, OnInit, TemplateRef} from '@angular/core';
import {ConcertsService} from '../services/concerts.service';
import {NgForOf, NgIf} from '@angular/common';
import {Concert} from '../data/concert';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {NgbModal, NgbModalRef} from '@ng-bootstrap/ng-bootstrap';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {LuxonModule} from 'luxon-angular';
import {DateTime} from 'luxon';
import timezones from 'timezones-list';
import {RouterLink} from '@angular/router';
import {listOfTours} from '../app.config';
import {ConcertBadgesComponent} from '../concert-badges/concert-badges.component';
import {ConcertTitleGenerator} from '../data/concert-title-generator';
import {CountdownComponent} from '../countdown/countdown.component';
import {ToastrService} from 'ngx-toastr';
import {ErrorResponse} from '../data/error-response';
import {ConcertFilterComponent} from '../concert-filter/concert-filter.component';
import {ConcertFilter} from '../data/concert-filter';

@Component({
  selector: 'app-concerts-list',
  imports: [
    NgForOf,
    ReactiveFormsModule,
    NgIf,
    FormsModule,
    LuxonModule,
    RouterLink,
    ConcertBadgesComponent,
    CountdownComponent,
    ConcertFilterComponent
  ],
  templateUrl: './concerts-list.component.html',
  styleUrl: './concerts-list.component.css'
})
export class ConcertsListComponent implements OnInit {
  private modalService = inject(NgbModal);

  concerts$: Concert[] = [];

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
  private readonly oidcSecurityService = inject(OidcSecurityService);

  hasWriteAccess$ = false;

  useNewTable$: boolean = true;

  // Filter that is used for loading the list
  currentFilter: ConcertFilter | null = null;

  constructor(private concertsService: ConcertsService, private toastr: ToastrService) {
    this.reloadConcertList(true);

    this.oidcSecurityService.checkAuth().subscribe(({ isAuthenticated }) => {
      this.hasWriteAccess$ = isAuthenticated;
    });
  }


  ngOnInit(): void {
  }


  private reloadConcertList(cache: boolean) {
    this.isLoading$ = true;
    this.concertsService.getFilteredConcerts(this.currentFilter, cache).subscribe(result => {
      this.concerts$ = result;
      this.isLoading$ = false;
    })
  }


  openModal(content: TemplateRef<any>) {
    return this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
  }


  onShowHistoricSwitchChanged() {
    this.reloadConcertList(true);
    console.log("Show historic: " + this.showHistoricConcerts$)
  }


  onUseNewTableSwitchChanged() {
    //this.reloadConcertList(true);
    console.log("use new table: " + this.useNewTable$)
  }


  onDeleteConcertClicked(content: TemplateRef<any>, concertId: string | undefined) {
    this.concertIdToDelete = concertId;

    if (this.concertIdToDelete == null) {
      return;
    }

    this.deleteConcertModal = this.openModal(content);
  }


  onDeleteConcertConfirm() {
    this.concertDeleting$ = true;
    if (this.concertIdToDelete == null) {
      this.deleteConcertModal?.dismiss();
      return;
    }

    let id = this.concertIdToDelete!;
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
        let errorResponse: ErrorResponse = err.error;
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
    console.log("filterEvent: ", filter);
    this.currentFilter = filter;
    this.reloadConcertList(false); // TODO: use cache
  }


  public getDateTime(inputDate: string) {
    return DateTime.fromISO(inputDate, {setZone: false});
  }


  public getDateTimeInTimezone(inputDate: string, timeZoneId: string) {
    return DateTime.fromISO(inputDate, {zone: timeZoneId});
  }


  protected readonly timezones = timezones;
  protected readonly DateTime = DateTime;
  protected readonly listOfTours = listOfTours;
  protected readonly ConcertTitleGenerator = ConcertTitleGenerator;
}
