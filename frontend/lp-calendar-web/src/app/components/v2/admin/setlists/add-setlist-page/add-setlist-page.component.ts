import { Component, inject, OnInit, signal } from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {ActivatedRoute, Router, RouterLink} from '@angular/router';
import {DateTime} from 'luxon';
import {Button} from 'primeng/button';
import {Card} from 'primeng/card';
import {FloatLabel} from 'primeng/floatlabel';
import {InputGroup} from 'primeng/inputgroup';
import {InputGroupAddon} from 'primeng/inputgroupaddon';
import {InputText} from 'primeng/inputtext';
import {Divider} from 'primeng/divider';
import {MessageService} from 'primeng/api';
import { ConcertsService } from '../../../../../services/concerts.service';
import { SetlistsService } from '../../../../../services/setlists.service';
import { CreateSetlistRequestDto, ProblemDetailsDto } from '../../../../../modules/lpshows-api/v3';

@Component({
  selector: 'app-add-setlist-page',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    Button,
    Card,
    RouterLink,
    FloatLabel,
    InputGroup,
    InputGroupAddon,
    InputText,
    Divider
  ],
  templateUrl: './add-setlist-page.component.html',
  styleUrl: './add-setlist-page.component.css',
})
export class AddSetlistPageComponent implements OnInit {
  private formBuilder = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private concertsService = inject(ConcertsService);
  private setlistService = inject(SetlistsService);
  private messageService = inject(MessageService);

  setlistForm = this.formBuilder.group({
    concertId: new FormControl('', [Validators.required]),
    concertTitle: new FormControl('', [Validators.min(5)]),
    setName: new FormControl('', []),
    linkinpediaUrl: new FormControl('', [Validators.pattern(/^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)/)]),
  });

  isSaving = signal(false);

  constructor() {
  }

  ngOnInit() {
    this.setlistForm.controls.concertId.valueChanges.subscribe(async id => {
      console.debug("Changed concertId", id);
      if (id != null) {
        let concertDetails = await this.concertsService.getDetailsById(id);
        let startDate = DateTime.fromISO(concertDetails.postedStartTime!);
        let year = startDate.year.toString();
        let month = startDate.month < 10 ? '0' + startDate.month : startDate.month;
        let day = startDate.day < 10 ? '0' + startDate.day : startDate.day;
        this.setlistForm.controls.concertTitle.setValue(`${concertDetails.venue.city.name}, ${concertDetails.venue.city.country.name} - ${year}-${month}-${day}`);
        this.setlistForm.controls.linkinpediaUrl.setValue(`https://linkinpedia.com/wiki/Live:${year}${month}${day}`);
      }
    });

    this.route.params.subscribe(params => {
      let concertId = params['concertId'];
      if (concertId != null && concertId.length > 0) {
        this.setlistForm.controls.concertId.setValue(params['concertId'], { emitEvent: true });
        this.setlistForm.controls.concertId.disable();
      }
    });
  }

  private makeRequestDtoFromFormData(): CreateSetlistRequestDto {
    let concertId = this.setlistForm.getRawValue().concertId?.valueOf();
    let concertTitle = this.setlistForm.value.concertTitle?.valueOf();
    let setName = this.setlistForm.value.setName?.valueOf();
    let linkinpediaUrl = this.setlistForm.value.linkinpediaUrl?.valueOf();

    console.log('concertId', concertId);
    console.log('linkinpediaUrl', linkinpediaUrl);

    let request: CreateSetlistRequestDto = {
      concertId: concertId,
      concertTitle: concertTitle,
      setName: setName,
      linkinpediaUrl: linkinpediaUrl,
    };

    console.debug("Request: ", request);

    return request;
  }

  async onSaveClicked() {
    this.isSaving.set(true);
    let request = this.makeRequestDtoFromFormData();
    try {
      let response = await this.setlistService.createSetlist(request);

      this.messageService.add({
        severity: 'success',
        summary: 'Setlist was created successfully',
        key: 'created-setlist'
      });

      this.navigateToSetlist(response.id);

      this.isSaving.set(false);
      this.setlistForm.reset();
    } catch (err) {
      let errorResponse: ProblemDetailsDto = err as ProblemDetailsDto;
      this.messageService.add({
        severity: 'error',
        summary: 'Could not create setlist',
        detail: errorResponse.detail,
      });
    } finally {
      this.isSaving.set(false);
    }
  }

  private navigateToSetlist(id: string | undefined) {
    this.router.navigate(['/admin/setlists/' + id]).catch(err => {
      return this.messageService.add({
        severity: 'danger',
        summary: 'Could not navigate to created setlist',
        text: err,
      });
    });
  }

  openConcertDetailsClicked() {
    let concertId = this.setlistForm.getRawValue().concertId?.valueOf();
    if (concertId?.length == 0) {
      return;
    }

    window.open("/concerts/" + concertId, "_blank");
  }


  openLinkinpediaUrlClicked() {
    let url = this.setlistForm.value.linkinpediaUrl?.valueOf();
    if (url?.length == 0) {
      return;
    }

    window.open(url, "_blank");
  }
}
