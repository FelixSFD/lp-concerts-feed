import {Component, inject, OnInit, TemplateRef} from '@angular/core';
import {ConcertsService} from '../services/concerts.service';
import {NgClass, NgForOf, NgIf} from '@angular/common';
import {Concert} from '../data/concert';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {NgbCalendar, NgbDateStruct, NgbModal, NgbModalRef} from '@ng-bootstrap/ng-bootstrap';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {LuxonModule} from 'luxon-angular';
import {DateTime} from 'luxon';
import timezones from 'timezones-list';
import {RouterLink} from '@angular/router';

@Component({
  selector: 'app-concerts-list',
  imports: [
    NgForOf,
    ReactiveFormsModule,
    NgClass,
    NgIf,
    FormsModule,
    LuxonModule,
    RouterLink
  ],
  templateUrl: './concerts-list.component.html',
  styleUrl: './concerts-list.component.css'
})
export class ConcertsListComponent implements OnInit {
  private formBuilder = inject(FormBuilder);
  private modalService = inject(NgbModal);

  concerts$: Concert[] = [];
  addConcertForm = this.formBuilder.group({
    venue: new FormControl('', [Validators.min(3), Validators.required]),
    timezone: new FormControl('', [Validators.required]),
    city: new FormControl('', [Validators.required]),
    state: new FormControl('', []),
    country: new FormControl('', [Validators.required]),
    postedStartTime: new FormControl('', [Validators.required])
  });

  // property to show whether the form is currently being sent to the server
  addConcertFormSaving$ = false;

  // if open, the modal is referenced here
  openAddConcertModal: NgbModalRef | undefined;

  // properties for datepicker
  today = inject(NgbCalendar).getToday();
  postedStartDateModel: NgbDateStruct | undefined;

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
    this.reloadConcertList();

    this.oidcSecurityService.checkAuth().subscribe(({ isAuthenticated }) => {
      this.hasWriteAccess$ = isAuthenticated;
    });
  }


  ngOnInit(): void {
  }


  private reloadConcertList() {
    this.concertsService.getConcerts().subscribe(result => {
      this.concerts$ = result;
    })
  }


  openModal(content: TemplateRef<any>) {
    return this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
  }


  onCreateConcertClicked(content: TemplateRef<any>) {
    this.openAddConcertModal = this.openModal(content);
  }


  onCreateFormSubmit() {
    this.addConcertFormSaving$ = true;
    console.log("Submitting concert...");

    const postedStartTime = this.addConcertForm.value.postedStartTime!;
    let newConcert = new Concert();
    newConcert.venue = this.addConcertForm.value.venue?.valueOf();
    newConcert.city = this.addConcertForm.value.city?.valueOf();
    newConcert.state = this.addConcertForm.value.state?.valueOf();
    newConcert.country = this.addConcertForm.value.country?.valueOf()
    //newConcert.postedStartTime = this.addConcertForm.value.postedStartTime?.valueOf();

    // Read the datetime-local value and selected timezone
    const selectedTimezone = this.addConcertForm.value.timezone?.valueOf(); // e.g., "America/Los_Angeles"

    // Convert to the selected timezone
    const localDateTime = DateTime.fromISO(postedStartTime); // Interpret as local datetime
    const zonedDateTime = localDateTime.setZone(selectedTimezone, {keepLocalTime: true});

    console.log('Original datetime-local value:', postedStartTime);
    console.log('Converted datetime in selected timezone:', zonedDateTime.toString());

    newConcert.timeZoneId = selectedTimezone;
    newConcert.postedStartTime = zonedDateTime.toISO()!;

    console.log("Created concert object: ");
    console.log(newConcert);

    this.concertsService.addConcert(newConcert).subscribe(result => {
      console.log("Update concert request finished");
      console.log(result);

      this.openAddConcertModal?.dismiss();
      this.addConcertFormSaving$ = false;
      this.reloadConcertList();
    });
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
      this.reloadConcertList();
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
}
