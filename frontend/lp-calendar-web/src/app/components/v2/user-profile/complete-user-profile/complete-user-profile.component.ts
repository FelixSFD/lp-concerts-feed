import { Component, inject } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { ButtonDirective } from 'primeng/button';
import { Fluid } from 'primeng/fluid';

@Component({
  imports: [
    ReactiveFormsModule,
    FloatLabel,
    InputText,
    ButtonDirective,
    Fluid
  ],
  selector: 'app-complete-user-profile',
  styleUrl: './complete-user-profile.component.css',
  templateUrl: './complete-user-profile.component.html',
})
export class CompleteUserProfileComponent {
  private formBuilder = inject(FormBuilder);

  protected setupForm = this.formBuilder.group({
    username: new FormControl('', [Validators.min(3), Validators.required]),
  });
}
