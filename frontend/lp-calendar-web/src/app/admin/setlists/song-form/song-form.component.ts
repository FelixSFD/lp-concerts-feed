import {Component, EventEmitter, inject, Input, Output, TemplateRef} from '@angular/core';
import {NgbModal} from '@ng-bootstrap/ng-bootstrap';
import {ToastrService} from 'ngx-toastr';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {SongDto} from '../../../modules/lpshows-api';
import {NgClass} from '@angular/common';

@Component({
  selector: 'app-song-form',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgClass
  ],
  templateUrl: './song-form.component.html',
  styleUrl: './song-form.component.css',
})
export class SongFormComponent {
  private modalService = inject(NgbModal);
  private toastr = inject(ToastrService);
  private formBuilder = inject(FormBuilder);

  @Input("is-saving")
  isSaving$: boolean = false;

  @Output("saveClicked")
  saveClicked = new EventEmitter<SongFormContent>();

  songForm = this.formBuilder.group({
    title: new FormControl('', [Validators.required]),
    isrc: new FormControl('', [Validators.pattern(/^(?<country>\w{2})(?<issuedBy>\w{2}\d)(?<year>\d{2})(?<number>\d{5})$/)]),
    linkinpediaUrl: new FormControl('', [Validators.pattern(/^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)/)]),
  });

  openModal(content: TemplateRef<any>) {
    return this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
  }


  openLinkinpediaUrlClicked() {
    let url = this.songForm.value.linkinpediaUrl?.valueOf();
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


  private readFromForm(): SongFormContent | null {
    let title = this.songForm.value.title?.valueOf();
    let isrc = this.songForm.value.isrc?.valueOf();
    let linkinpediaUrl = this.songForm.value.linkinpediaUrl?.valueOf();

    if (title == undefined) {
      this.toastr.error("Title is required");
      return null;
    }

    return {
      title: title!,
      isrc: isrc!,
      linkinpediaUrl: linkinpediaUrl ?? null
    };
  }


  public fillFormWith(song: SongDto) {
    console.debug("Fill form with data:", song);
    this.songForm.controls.title.setValue(song.title ?? null);
    this.songForm.controls.isrc.setValue(song.isrc ?? null);
    this.songForm.controls.linkinpediaUrl.setValue(song.linkinpediaUrl ?? null);
  }
}

export class SongFormContent {
  title!: string;
  isrc: string | null = null;
  linkinpediaUrl: string | null = null;
}
