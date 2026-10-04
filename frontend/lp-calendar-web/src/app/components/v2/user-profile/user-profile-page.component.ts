import { Component, inject, OnInit, signal } from '@angular/core';
import {OidcSecurityService} from 'angular-auth-oidc-client';

import {RouterLink} from '@angular/router';
import {Card} from 'primeng/card';
import { Button, ButtonDirective } from 'primeng/button';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import {InputText} from 'primeng/inputtext';
import { CountryDto, ProblemDetailsDto, UpdateUserProfileDto } from '../../../modules/lpshows-api/v3';
import { Divider } from 'primeng/divider';
import { FloatLabel } from 'primeng/floatlabel';
import { UsersService } from '../../../services/users.service';
import { LocationsService } from '../../../services/locations.service';
import { MessageService } from 'primeng/api';
import { Select } from 'primeng/select';

@Component({
  selector: 'app-user-profile-page',
  imports: [
    Card,
    InputText,
    ReactiveFormsModule,
    Divider,
    ButtonDirective,
    FloatLabel,
    Select
  ],
  templateUrl: './user-profile-page.component.html',
  styleUrl: './user-profile-page.component.css'
})
export class UserProfilePageComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly oidcSecurityService = inject(OidcSecurityService);
  private usersService = inject(UsersService);
  private locationsService = inject(LocationsService);
  private messageService = inject(MessageService);

  userId$ = ""
  email$ = ""

  protected isSavingProfile = signal(false);
  protected availableCountries = signal<CountryDto[]>([]);

  protected userForm = this.formBuilder.group({
    username: new FormControl('', [Validators.min(3), Validators.required]),
    originCountryCode: new FormControl<string | null>(null, []),
  });


  ngOnInit(): void {
    this.oidcSecurityService.userData$.subscribe((usr) => {
      console.debug("User -->", usr);
      this.userId$ = usr.userData["sub"];
      this.email$ = usr.userData["email"];
    });

    this.locationsService.getCountries().subscribe((countries) => {
      this.availableCountries.set(countries);
    });

    this.usersService.getCurrentUser().then((usr) => {
      this.userForm.patchValue({
        username: usr.username,
        originCountryCode: usr.originCountryCode,
      });
    });
  }

  protected async onSaveUserProfileClicked() {
    let username = this.userForm.value.username;
    if (!username || username.length < 3) {
      this.messageService.add({ severity: 'error', summary: 'Username is required ant must be at least 3 characters long' });
      return;
    }

    this.isSavingProfile.set(true);
    let request: UpdateUserProfileDto = {
      username: this.userForm.value.username!,
      originCountryCode: this.userForm.value.originCountryCode ?? null
    };
    try {
      await this.usersService.updateCurrentUserProfile(request);
      this.messageService.add({ severity: 'success', summary: 'Successfully updated your profile' });
    } catch (error) {
      let problemDetails = error as ProblemDetailsDto;
      let message = problemDetails.detail;
      this.messageService.add({ severity: 'error', summary: 'Failed to update user profile', detail: message });
    } finally {
      this.isSavingProfile.set(false);
    }
  }
}
