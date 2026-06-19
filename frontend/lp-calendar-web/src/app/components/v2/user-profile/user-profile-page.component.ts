import {Component, inject, OnInit} from '@angular/core';
import {OidcSecurityService} from 'angular-auth-oidc-client';

import {RouterLink} from '@angular/router';
import {Card} from 'primeng/card';
import {Button} from 'primeng/button';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {InputText} from 'primeng/inputtext';

@Component({
  selector: 'app-user-profile-page',
  imports: [
    RouterLink,
    Card,
    Button,
    FormsModule,
    InputText,
    ReactiveFormsModule
  ],
  templateUrl: './user-profile-page.component.html',
  styleUrl: './user-profile-page.component.css'
})
export class UserProfilePageComponent implements OnInit {
  private readonly oidcSecurityService = inject(OidcSecurityService);

  userId$ = ""
  email$ = ""
  username$ = ""
  groupNames$: string[] = [];


  ngOnInit(): void {
    this.oidcSecurityService.userData$.subscribe((usr) => {
      console.debug("User -->", usr);
      this.userId$ = usr.userData["sub"];
      this.email$ = usr.userData["email"];
      this.username$ = usr.userData["custom:display_name"] ?? "";
    });

    this.oidcSecurityService.getPayloadFromAccessToken().subscribe(payload => {
      console.debug(payload);
      if (payload.hasOwnProperty("cognito:groups")) {
        this.groupNames$ = payload["cognito:groups"] ?? [];
      }
    });
  }
}
