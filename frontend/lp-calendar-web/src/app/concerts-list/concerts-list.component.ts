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

@Component({
  selector: 'app-concerts-list',
  imports: [
    NgForOf,
    ReactiveFormsModule,
    NgIf,
    FormsModule,
    LuxonModule,
    RouterLink,
    ConcertBadgesComponent
  ],
  templateUrl: './concerts-list.component.html',
  styleUrl: './concerts-list.component.css'
})
export class ConcertsListComponent implements OnInit {
  private modalService = inject(NgbModal);

  concerts$: Concert[] = [];

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

  constructor(private concertsService: ConcertsService) {
    this.reloadConcertList(true);

    this.oidcSecurityService.checkAuth().subscribe(({ isAuthenticated }) => {
      this.hasWriteAccess$ = isAuthenticated;
    });
  }


  ngOnInit(): void {
  }


  private reloadConcertList(cache: boolean) {
    this.concertsService.getConcerts(cache).subscribe(result => {
      this.concerts$ = result;
    })
  }


  openModal(content: TemplateRef<any>) {
    return this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
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
    console.log("Will delete concert: " + id);

    this.concertsService.deleteConcert(id).subscribe(result => {
      console.log("DELETE concert request finished");
      console.log(result);

      this.deleteConcertModal?.dismiss();
      this.concertDeleting$ = false;
      this.reloadConcertList(false);
    });
  }


  dismissConcertConfirmModal() {
    this.deleteConcertModal?.dismiss();
  }


  public getDateTime(inputDate: string) {
    return DateTime.fromISO(inputDate, {setZone: false});
  }


  public getDateTimeInTimezone(inputDate: string) {
    return DateTime.fromISO(inputDate, {setZone: true});
  }

  protected readonly timezones = timezones;
  protected readonly DateTime = DateTime;
  protected readonly listOfTours = listOfTours;
}
