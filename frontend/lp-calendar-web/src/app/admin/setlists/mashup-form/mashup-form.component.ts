import {Component, inject} from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {SetlistEntryIconsComponent} from "../setlist-entry-icons/setlist-entry-icons.component";
import {ErrorResponseDto} from '../../../modules/lpshows-api';
import {NgClass} from '@angular/common';

@Component({
  selector: 'app-mashup-form',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgClass
  ],
  templateUrl: './mashup-form.component.html',
  styleUrl: './mashup-form.component.css',
})
export class MashupFormComponent {
  private formBuilder = inject(FormBuilder);

  mashupForm = this.formBuilder.group({
    title: new FormControl('', [Validators.required]),
    linkinpediaUrl: new FormControl('', [Validators.pattern(/^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)/)]),
  });

  isSaving$: boolean = false;


  openLinkinpediaUrlClicked() {
    let url = this.mashupForm.value.linkinpediaUrl?.valueOf();
    if (url?.length == 0) {
      return;
    }

    window.open(url, "_blank");
  }


  onSaveClicked() {
  }
}
