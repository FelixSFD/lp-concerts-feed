import { Component, EventEmitter, inject, OnInit, Output, signal } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { ButtonDirective } from 'primeng/button';
import { Fluid } from 'primeng/fluid';
import { UsersService } from '../../../../services/users.service';
import { ProblemDetailsDto, UpdateUserProfileDto } from '../../../../modules/lpshows-api/v3';
import { MessageService } from 'primeng/api';

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
  private usersService = inject(UsersService);
  private messageService = inject(MessageService);

  protected setupForm = this.formBuilder.group({
    username: new FormControl('', [Validators.min(3), Validators.required]),
  });

  protected isSavingProfile = signal(false);

  private randomUsernames: string[] = [];

  @Output("onProfileCompleted") profileCompleted = new EventEmitter<void>();

  protected async onSetRandomNameClicked() {
    if (this.randomUsernames.length === 0) {
      await this.loadSuggestedUsernames();
    }

    let randomUsername = this.randomUsernames[Math.floor(Math.random() * this.randomUsernames.length)];
    this.setupForm.get('username')?.setValue(randomUsername);
  }

  protected async onSaveUserProfileClicked() {
    let username = this.setupForm.value.username;
    if (!username || username.length < 3) {
      this.messageService.add({ severity: 'error', summary: 'Username is required ant must be at least 3 characters long' });
      return;
    }

    this.isSavingProfile.set(true);
    let request: UpdateUserProfileDto = {
      username: this.setupForm.value.username!
    };
    try {
      await this.usersService.updateCurrentUserProfile(request);
      this.profileCompleted.emit();
    } catch (error) {
      let problemDetails = error as ProblemDetailsDto;
      let message = problemDetails.detail;
      this.messageService.add({ severity: 'error', summary: 'Failed to update user profile', detail: message });
    } finally {
      this.isSavingProfile.set(false);
    }
  }

  private async loadSuggestedUsernames() {
    try {
      this.randomUsernames = await this.usersService.getSuggestedUsernames();
    } catch (error) {
      console.error('Failed to load suggested usernames:', error);
      let problemDetails = error as ProblemDetailsDto;
      this.messageService.add({ severity: 'error', summary: 'Failed to load suggested usernames', detail: problemDetails.detail });
    }
  }
}
