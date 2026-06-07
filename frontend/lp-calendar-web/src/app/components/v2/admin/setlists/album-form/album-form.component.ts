import {Component, EventEmitter, inject, Input, Output} from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {AlbumDto} from '../../../../../modules/lpshows-api';
import {InputText} from 'primeng/inputtext';
import {Card} from 'primeng/card';
import {FloatLabel} from 'primeng/floatlabel';
import {InputGroupAddon} from 'primeng/inputgroupaddon';
import {InputGroup} from 'primeng/inputgroup';
import {Button} from 'primeng/button';
import {Divider} from 'primeng/divider';
import {MessageService} from 'primeng/api';

@Component({
  selector: 'app-album-form',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    InputText,
    Card,
    FloatLabel,
    InputGroupAddon,
    InputGroup,
    Button,
    Divider
  ],
  templateUrl: './album-form.component.html',
  styleUrl: './album-form.component.css',
})
export class AlbumFormComponent {
  private messageService = inject(MessageService);
  private formBuilder = inject(FormBuilder);

  @Input("is-saving")
  isSaving$: boolean = false;

  @Output("saveClicked")
  saveClicked = new EventEmitter<AlbumFormContent>();

  albumForm = this.formBuilder.group({
    title: new FormControl('', [Validators.required]),
    linkinpediaUrl: new FormControl('', [Validators.pattern(/^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)/)]),
  });

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
      this.messageService.add({severity: 'error', summary: 'Please enter a valid title.'});
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
