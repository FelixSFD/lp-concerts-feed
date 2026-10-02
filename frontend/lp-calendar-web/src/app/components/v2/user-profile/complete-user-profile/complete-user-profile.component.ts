import { Component, EventEmitter, inject, OnInit, Output, signal } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { ButtonDirective } from 'primeng/button';
import { Fluid } from 'primeng/fluid';
import { UsersService } from '../../../../services/users.service';
import { CountryDto, ProblemDetailsDto, UpdateUserProfileDto } from '../../../../modules/lpshows-api/v3';
import { MessageService } from 'primeng/api';
import { LocationsService } from '../../../../services/locations.service';
import { firstValueFrom } from 'rxjs';
import { Select } from 'primeng/select';

@Component({
  imports: [
    ReactiveFormsModule,
    FloatLabel,
    InputText,
    ButtonDirective,
    Fluid,
    Select
  ],
  selector: 'app-complete-user-profile',
  styleUrl: './complete-user-profile.component.css',
  templateUrl: './complete-user-profile.component.html',
})
export class CompleteUserProfileComponent implements OnInit {
  private formBuilder = inject(FormBuilder);
  private usersService = inject(UsersService);
  private locationsService = inject(LocationsService);
  private messageService = inject(MessageService);

  protected setupForm = this.formBuilder.group({
    username: new FormControl('', [Validators.min(3), Validators.required]),
    originCountry: new FormControl<CountryDto | null>(null, []),
  });

  protected isSavingProfile = signal(false);
  protected availableCountries = signal<CountryDto[]>([]);

  private randomUsernames: string[] = [];

  @Output("onProfileCompleted") profileCompleted = new EventEmitter<void>();

  ngOnInit() {
    this.loadAvailableCountries().then();
  }

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
      username: this.setupForm.value.username!,
      originCountryCode: this.setupForm.value.originCountry?.isoCode ?? null
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

  private async loadAvailableCountries() {
    try {
      this.availableCountries.set(await firstValueFrom(this.locationsService.getCountries()));
    } catch (error) {
      console.error('Failed to load available countries:', error);
      let problemDetails = error as ProblemDetailsDto;
      this.messageService.add({ severity: 'error', summary: 'Failed to load available countries', detail: problemDetails.detail });
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
