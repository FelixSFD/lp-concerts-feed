import {Component, inject, OnInit, TemplateRef} from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {ErrorResponseDto, SongDto} from '../../../modules/lpshows-api';
import {NgClass} from '@angular/common';
import {AddSetlistEntryFormComponent} from '../add-setlist-entry-form/add-setlist-entry-form.component';
import {NgbModal, NgbModalRef} from '@ng-bootstrap/ng-bootstrap';
import {SelectSongComponent} from '../select-song/select-song.component';
import {SongsService} from '../../../services/songs.service';
import {ToastrService} from 'ngx-toastr';

@Component({
  selector: 'app-mashup-form',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgClass,
    AddSetlistEntryFormComponent,
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

  mashupForm = this.formBuilder.group({
    title: new FormControl('', [Validators.required]),
    linkinpediaUrl: new FormControl('', [Validators.pattern(/^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)/)]),
  });

  isSaving$: boolean = false;

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
}
