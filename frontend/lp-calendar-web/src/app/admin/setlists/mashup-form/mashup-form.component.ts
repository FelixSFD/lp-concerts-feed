import {Component, EventEmitter, inject, Input, OnInit, Output, TemplateRef} from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {ErrorResponseDto, SongDto, SongMashupDto} from '../../../modules/lpshows-api';
import {NgClass} from '@angular/common';
import {AddSetlistEntryFormComponent} from '../add-setlist-entry-form/add-setlist-entry-form.component';
import {NgbModal, NgbModalRef} from '@ng-bootstrap/ng-bootstrap';
import {SelectSongComponent} from '../select-song/select-song.component';
import {SongsService} from '../../../services/songs.service';
import {ToastrService} from 'ngx-toastr';
import {RouterLink} from '@angular/router';

@Component({
  selector: 'app-mashup-form',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgClass,
    SelectSongComponent
  ],
  templateUrl: './mashup-form.component.html',
  styleUrl: './mashup-form.component.css',
})
export class MashupFormComponent implements OnInit {
  private modalService = inject(NgbModal);
  private toastr = inject(ToastrService);
  private formBuilder = inject(FormBuilder);
  private songsService = inject(SongsService);

  @Input("is-saving")
  isSaving$: boolean = false;

  @Output("saveClicked")
  saveClicked = new EventEmitter<MashupFormContent>();

  mashupForm = this.formBuilder.group({
    title: new FormControl('', [Validators.required]),
    linkinpediaUrl: new FormControl('', [Validators.pattern(/^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)/)]),
  });

  // if open, the modal is referenced here
  addSongModal: NgbModalRef | undefined;

  songsInMashup$: SongDto[] = [];

  availableSongs$: SongDto[] = [];

  selectedNewSong$: SongDto | null | undefined;


  ngOnInit() {
    this.songsService.getAllSongs(true).subscribe({
      next: data => {
        this.availableSongs$ = data;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.toastr.error(errorResponse.message, "Could not update setlist entry");
      }
    })
  }

  openModal(content: TemplateRef<any>) {
    return this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
  }


  onAddSongClicked(content: TemplateRef<any>) {
    this.addSongModal = this.openModal(content);
  }


  openLinkinpediaUrlClicked() {
    let url = this.mashupForm.value.linkinpediaUrl?.valueOf();
    if (url?.length == 0) {
      return;
    }

    window.open(url, "_blank");
  }


  onSaveClicked() {
    let content = this.readFromForm();
    if (content) {
      this.saveClicked.emit(content!);
    }
  }


  onSongSelected(song: SongDto) {
    console.debug("onSongSelected", song);
    this.selectedNewSong$ = song;
  }


  onConfirmSongSelectionClicked() {
    if (this.selectedNewSong$) {
      this.songsInMashup$.push(this.selectedNewSong$);
    }

    this.addSongModal?.dismiss();
  }


  onRemoveSongFromMashupClicked(id: number) {
    this.songsInMashup$ = this.songsInMashup$.filter(song => song.id !== id);
  }


  private readFromForm(): MashupFormContent | null {
    let title = this.mashupForm.value.title?.valueOf();
    let linkinpediaUrl = this.mashupForm.value.linkinpediaUrl?.valueOf();

    if (title == undefined) {
      this.toastr.error("Title is required");
      return null;
    }

    return {
      title: title!,
      linkinpediaUrl: linkinpediaUrl ?? null,
      songs: this.songsInMashup$
    };
  }


  public fillFormWith(mashup: SongMashupDto) {
    console.debug("Fill form with data:", mashup);
    this.mashupForm.controls.title.setValue(mashup.title ?? null);
    this.mashupForm.controls.linkinpediaUrl.setValue(mashup.linkinpediaUrl ?? null);

    this.songsInMashup$ = mashup.songs ?? [];
  }
}


export class MashupFormContent {
  title!: string;
  linkinpediaUrl: string | null = null;
  songs!: SongDto[];
}
