import {Component, inject} from '@angular/core';
import {FormBuilder, FormControl, ReactiveFormsModule, Validators} from "@angular/forms";

@Component({
  selector: 'app-add-setlist-entry-form',
    imports: [
        ReactiveFormsModule
    ],
  templateUrl: './add-setlist-entry-form.component.html',
  styleUrl: './add-setlist-entry-form.component.css',
})
export class AddSetlistEntryFormComponent {
  private formBuilder = inject(FormBuilder);

  setlistEntryForm = this.formBuilder.group({
    showType: new FormControl('', [Validators.required]),
    tourName: new FormControl('', []),
  });
}
