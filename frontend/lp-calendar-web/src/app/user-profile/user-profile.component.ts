import {Component, inject} from '@angular/core';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {AsyncPipe, JsonPipe} from '@angular/common';

@Component({
  selector: 'app-user-profile',
  imports: [
    AsyncPipe,
    JsonPipe
  ],
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.css'
})
export class UserProfileComponent {
  private readonly oidcSecurityService = inject(OidcSecurityService);

  userData$ = this.oidcSecurityService.userData$;

  userId$ = ""
  email$ = ""


  ngOnInit(): void {
    this.oidcSecurityService.userData$.subscribe((usr) => {
      console.log("User -->", usr);
      this.userId$ = usr.userData["username"];
      this.email$ = usr.userData["email"];
    })
  }
}
