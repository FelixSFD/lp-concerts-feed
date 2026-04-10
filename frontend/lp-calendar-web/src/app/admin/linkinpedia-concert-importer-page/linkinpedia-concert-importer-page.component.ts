import {Component, inject, TemplateRef, viewChild} from '@angular/core';
import {FormBuilder, FormControl, ReactiveFormsModule, Validators} from '@angular/forms';
import {NgClass} from '@angular/common';
import {HttpClient} from '@angular/common/http';
import {ToastrService} from 'ngx-toastr';
import {SetlistsService} from '../../services/setlists.service';
import {
  CreateSongRequestDto,
  ErrorResponseDto,
  ImportSetlistEntryPreviewDto,
  ImportSetlistPreviewDto,
  SongDto
} from '../../modules/lpshows-api';
import {SongFormComponent} from '../setlists/song-form/song-form.component';
import {NgbModal, NgbModalRef} from '@ng-bootstrap/ng-bootstrap';
import {SongsService} from '../../services/songs.service';

@Component({
  selector: 'app-linkinpedia-concert-importer-page',
  imports: [
    ReactiveFormsModule,
    NgClass,
    SongFormComponent
  ],
  templateUrl: './linkinpedia-concert-importer-page.component.html',
  styleUrl: './linkinpedia-concert-importer-page.component.css',
})
export class LinkinpediaConcertImporterPageComponent {
  private modalService = inject(NgbModal);
  private readonly formBuilder = inject(FormBuilder);
  private readonly httpClient = inject(HttpClient);
  private readonly toastr = inject(ToastrService);
  private readonly setlistsService = inject(SetlistsService);
  private readonly songsService = inject(SongsService);

  // true if the page is currently reading the source information
  isReadingSource$ = false;

  tmpLinkinpediaSource$: string | null = null;

  sourceDataForm = this.formBuilder.group({
    linkinpediaUrl: new FormControl('https://linkinpedia.com/wiki/Live:20240905', [Validators.required, Validators.pattern(/^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)/)]),
  });

  generatedSetlist$: ImportSetlistPreviewDto | null = null;

  // properties for the add song modal
  isAddingSong$: boolean = false;
  addSongModal: NgbModalRef | undefined;
  private newSongPrefillData: ImportSetlistEntryPreviewDto | undefined;
  private newSongFormComponent = viewChild(SongFormComponent);

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

        this.toastr.error(errorResponse.message, "Could not get import data!");
        this.isAddingSong$ = false;
      }
    })
  }


  dismissAddSongModal() {
    this.addSongModal?.dismiss();
  }


  private loadFromLinkinpedia() {
    let url = this.sourceDataForm.value.linkinpediaUrl?.valueOf() ?? null;
    if (url) {
      let apiUrl = this.getApiUrlForPage(url);
      console.debug("Linkinpedia API URL: ", apiUrl);

      this.httpClient.get(apiUrl).subscribe({
        next: data => {
          console.debug("API result: ", data);
        },
        error: error => {
          console.error("API call to Linkinpedia failed: ", error);
          this.toastr.error(error);
        }
      })
    } else {
      this.toastr.error('URL is not set.');
    }
  }


  private getApiUrlForPage(linkinpediaUrl: string): string {
    const regex = /linkinpedia\.com\/(?<originalPath>wiki\/)(?<page>[^\/]+)$/gm;
    const subst = `linkinpedia.com/w/rest.php/v1/page/$2`;
    return linkinpediaUrl.replace(regex, subst);
  }
}
