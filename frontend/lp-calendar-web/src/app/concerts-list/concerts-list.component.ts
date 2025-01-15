import {Component, inject, OnInit, signal, TemplateRef, WritableSignal} from '@angular/core';
import {ConcertsService} from '../services/concerts.service';
import {DatePipe, NgClass, NgForOf, NgIf} from '@angular/common';
import {Concert} from '../data/concert';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {ModalDismissReasons, NgbCalendar, NgbDateStruct, NgbModal, NgbModalRef} from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-concerts-list',
  imports: [
    DatePipe,
    NgForOf,
    ReactiveFormsModule,
    NgClass,
    NgIf,
    FormsModule
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

  constructor(private concertsService: ConcertsService) {
    this.reloadConcertList()
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
    let newConcert = new Concert();
    newConcert.venue = this.addConcertForm.value.venue?.valueOf();
    newConcert.city = this.addConcertForm.value.city?.valueOf();
    newConcert.state = this.addConcertForm.value.state?.valueOf();
    newConcert.country = this.addConcertForm.value.country?.valueOf()
    newConcert.postedStartTime = this.addConcertForm.value.postedStartTime?.valueOf();

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
}
