import {Component, inject, OnInit, TemplateRef, viewChild} from '@angular/core';
import {FormBuilder, FormControl, ReactiveFormsModule, Validators} from '@angular/forms';
import {NgClass} from '@angular/common';
import {HttpClient} from '@angular/common/http';
import {ToastrService} from 'ngx-toastr';
import {SetlistsService} from '../../services/setlists.service';
import {
  CreateSongMashupRequestDto,
  CreateSongRequestDto,
  ErrorResponseDto,
  ImportSetlistEntryPreviewDto,
  ImportSetlistPreviewDto,
  SongDto, SongMashupDto
} from '../../modules/lpshows-api';
import {SongFormComponent} from '../setlists/song-form/song-form.component';
import {NgbModal, NgbModalRef} from '@ng-bootstrap/ng-bootstrap';
import {SongsService} from '../../services/songs.service';
import {MashupFormComponent} from '../setlists/mashup-form/mashup-form.component';
import {ActivatedRoute} from '@angular/router';
import {DateTime} from 'luxon';
import {ConcertsService} from '../../services/concerts.service';

@Component({
  selector: 'app-linkinpedia-concert-importer-page',
  imports: [
    ReactiveFormsModule,
    NgClass,
    SongFormComponent,
    MashupFormComponent
  ],
  templateUrl: './linkinpedia-concert-importer-page.component.html',
  styleUrl: './linkinpedia-concert-importer-page.component.css',
})
export class LinkinpediaConcertImporterPageComponent implements OnInit {
  private modalService = inject(NgbModal);
  private readonly formBuilder = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private readonly toastr = inject(ToastrService);
  private readonly setlistsService = inject(SetlistsService);
  private readonly songsService = inject(SongsService);
  private readonly concertsService = inject(ConcertsService);

  // true if the page is currently reading the source information
  isReadingSource$ = false;

  sourceDataForm = this.formBuilder.group({
    concertId: new FormControl('', [Validators.required]),
    linkinpediaUrl: new FormControl('https://linkinpedia.com/wiki/Live:20240905', [Validators.required, Validators.pattern(/^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)/)]),
  });

  generatedSetlist$: ImportSetlistPreviewDto | null = null;

  // properties for the add song modal
  isAddingSong$: boolean = false;
  addSongModal: NgbModalRef | undefined;
  private newSongFormComponent = viewChild(SongFormComponent);

  // properties for the add mashup modal
  isAddingMashup$: boolean = false;
  addMashupModal: NgbModalRef | undefined;
  private newMashupFormComponent = viewChild(MashupFormComponent);


  ngOnInit() {
    this.sourceDataForm.controls.concertId.valueChanges.subscribe(id => {
      console.debug("Changed concertId", id);
      if (id != null) {
        this.concertsService.getConcert(id).subscribe(concert => {
          let startDate = DateTime.fromISO(concert.postedStartTime!, {zone: concert.timeZoneId});
          let year = startDate.year.toString();
          let month = startDate.month < 10 ? '0' + startDate.month : startDate.month;
          let day = startDate.day < 10 ? '0' + startDate.day : startDate.day;
          this.sourceDataForm.controls.linkinpediaUrl.setValue(`https://linkinpedia.com/wiki/Live:${year}${month}${day}`);
        });
      }
    });

    this.route.params.subscribe(params => {
      let concertId = params['concertId'];
      if (concertId != null && concertId.length > 0) {
        this.sourceDataForm.controls.concertId.setValue(params['concertId'], { emitEvent: true });
        this.sourceDataForm.controls.concertId.disable();
      }
    });
  }


  onLoadSourceClicked() {
    this.startImport();
  }


  private startImport() {
    let url = this.sourceDataForm.value.linkinpediaUrl?.valueOf();
    if (url) {
      this.isReadingSource$ = true;
      this.setlistsService.getImportInfosFromLinkinpedia(url)
        .subscribe({
          next: data => {
            this.generatedSetlist$ = data;

            this.toastr.success('Successfully read setlist from Linkinpedia');
            this.isReadingSource$ = false;
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            console.warn("Failed to fetch import data:", err);

            this.toastr.error(errorResponse.message, "Could not get import data!");
            this.isReadingSource$ = false;
          }
        });
    }
  }


  openModal(content: TemplateRef<any>) {
    return this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
  }


  openCreateSongBtnClicked(entryPreview: ImportSetlistEntryPreviewDto, content: TemplateRef<any>) {
    let songPrefill: SongDto = {
      title: entryPreview.title,
    };

    this.addSongModal = this.openModal(content);
    this.newSongFormComponent()?.fillFormWith(songPrefill);
  }


  onAddSongConfirm() {
    this.isAddingSong$ = true;

    // find the song information
    let newSongValues = this.newSongFormComponent()?.readFromForm();
    if (!newSongValues) {
      this.toastr.error('Failed to read data from form');
      return;
    }

    let createRequest: CreateSongRequestDto = {
      title: newSongValues.title,
      albumId: Number(newSongValues.albumId),
      isrc: newSongValues.isrc,
      appleMusicId: newSongValues.appleMusicId,
      linkinpediaUrl: newSongValues.linkinpediaUrl,
    }

    console.debug('Creating new song...', createRequest);

    this.songsService.createSong(createRequest).subscribe({
      next: data => {
        this.onLoadSourceClicked();
        this.isAddingSong$ = false;
        this.dismissAddSongModal();
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        console.warn("Failed to fetch import data:", err);

        this.toastr.error(errorResponse.message, "Could not create new song!");
        this.isAddingSong$ = false;
      }
    })
  }


  dismissAddSongModal() {
    this.addSongModal?.dismiss();
  }


  openCreateMashupBtnClicked(entryPreview: ImportSetlistEntryPreviewDto, content: TemplateRef<any>) {
    let mashupPrefill: SongMashupDto = {
      title: entryPreview.title,
    };

    this.addMashupModal = this.openModal(content);
    this.newMashupFormComponent()?.fillFormWith(mashupPrefill);
  }


  onAddMashupConfirm() {
    this.isAddingMashup$ = true;

    // find the song information
    let newMashupValues = this.newMashupFormComponent()?.readFromForm();
    if (!newMashupValues) {
      this.toastr.error('Failed to read data from form');
      return;
    }

    let createRequest: CreateSongMashupRequestDto = {
      title: newMashupValues.title,
      linkinpediaUrl: newMashupValues.linkinpediaUrl,
      songIds: newMashupValues.songs.map(s => s.id!),
    }

    console.debug('Creating new song mashup...', createRequest);

    this.songsService.createMashup(createRequest).subscribe({
      next: data => {
        this.onLoadSourceClicked();
        this.isAddingMashup$ = false;
        this.dismissAddMashupModal();
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        console.warn("Failed to fetch import data:", err);

        this.toastr.error(errorResponse.message, "Could not create new song mashup!");
        this.isAddingMashup$ = false;
      }
    })
  }


  dismissAddMashupModal() {
    this.addMashupModal?.dismiss();
  }


  openConcertDetailsClicked() {
    let concertId = this.sourceDataForm.getRawValue().concertId?.valueOf();
    if (concertId?.length == 0) {
      return;
    }

    window.open("/concerts/" + concertId, "_blank");
  }
}
