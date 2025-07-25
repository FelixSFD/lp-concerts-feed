import {Component, inject, OnInit} from '@angular/core';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {AsyncPipe, JsonPipe, NgForOf} from '@angular/common';
import {RouterLink} from '@angular/router';

@Component({
  selector: 'app-user-profile',
  imports: [
    AsyncPipe,
    JsonPipe,
    NgForOf,
    RouterLink
  ],
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.css'
})
export class UserProfileComponent implements OnInit {
  private readonly oidcSecurityService = inject(OidcSecurityService);

  userData$ = this.oidcSecurityService.userData$;

  userId$ = ""
  email$ = ""
  username$ = ""
  groupNames$: string[] = [];


  ngOnInit(): void {
    this.oidcSecurityService.userData$.subscribe((usr) => {
      console.log("User -->", usr);
      this.userId$ = usr.userData["sub"];
      this.email$ = usr.userData["email"];
      this.username$ = usr.userData["custom:display_name"] ?? "";
    });

    this.oidcSecurityService.getPayloadFromAccessToken().subscribe(payload => {
      console.log(payload);
      if (payload.hasOwnProperty("cognito:groups")) {
        this.groupNames$ = payload["cognito:groups"] ?? [];
      }
    });
  }
}
