import {Component, inject, Input} from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {NgClass} from '@angular/common';

@Component({
  selector: 'app-add-setlist-page',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgClass
  ],
  templateUrl: './add-setlist-page.component.html',
  styleUrl: './add-setlist-page.component.css',
})
export class AddSetlistPageComponent {
  private formBuilder = inject(FormBuilder);

  setlistForm = this.formBuilder.group({
    concertId: new FormControl('', [Validators.required]),
    linkinpediaUrl: new FormControl('', [Validators.pattern(/^https?:\/\/[a-z0-9]+(?:[-.][a-z0-9]+)*(?::[0-9]{1,5})?(?:\/[^\/\r\n]+)*\.[a-z]{2,5}(?:[?#]\S*)?$/)]),
  });

  isSaving$: boolean = false;

  onSaveClicked() {
    this.isSaving$ = true;
    this.isSaving$ = false;
  }
}
