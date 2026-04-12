import {Component, EventEmitter, inject, Input, Output, TemplateRef} from '@angular/core';
import {NgbModal} from '@ng-bootstrap/ng-bootstrap';
import {ToastrService} from 'ngx-toastr';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {AlbumDto} from '../../../modules/lpshows-api';
import {NgClass} from '@angular/common';

@Component({
  selector: 'app-album-form',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgClass
  ],
  templateUrl: './album-form.component.html',
  styleUrl: './album-form.component.css',
})
export class AlbumFormComponent {
  private modalService = inject(NgbModal);
  private toastr = inject(ToastrService);
  private formBuilder = inject(FormBuilder);

  @Input("is-saving")
  isSaving$: boolean = false;

  @Output("saveClicked")
  saveClicked = new EventEmitter<AlbumFormContent>();

  albumForm = this.formBuilder.group({
    title: new FormControl('', [Validators.required]),
    linkinpediaUrl: new FormControl('', [Validators.pattern(/^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)/)]),
  });

  openModal(content: TemplateRef<any>) {
    return this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
  }


  openLinkinpediaUrlClicked() {
    let url = this.albumForm.value.linkinpediaUrl?.valueOf();
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


  private readFromForm(): AlbumFormContent | null {
    let title = this.albumForm.value.title?.valueOf();
    let linkinpediaUrl = this.albumForm.value.linkinpediaUrl?.valueOf();

    if (title == undefined) {
      this.toastr.error("Title is required");
      return null;
    }

    return {
      title: title!,
      linkinpediaUrl: linkinpediaUrl ?? null
    };
  }


  public fillFormWith(album: AlbumDto) {
    console.debug("Fill form with data:", album);
    this.albumForm.controls.title.setValue(album.title ?? null);
    this.albumForm.controls.linkinpediaUrl.setValue(album.linkinpediaUrl ?? null);
  }
}

export class AlbumFormContent {
  title!: string;
  linkinpediaUrl: string | null = null;
}
