import {Component, inject, OnInit} from '@angular/core';
import {RouterLink, RouterLinkActive, RouterOutlet} from '@angular/router';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {AsyncPipe, JsonPipe, NgIf} from '@angular/common';
import {authConfig, logoutRedirectUrl} from './auth/auth.config';
import {environment} from '../environments/environment';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NgbModule, NgIf, AsyncPipe, JsonPipe, RouterLink, RouterLinkActive],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'lp-calendar-web';

  private readonly oidcSecurityService = inject(OidcSecurityService);

  configuration$ = this.oidcSecurityService.getConfiguration();

  userData$ = this.oidcSecurityService.userData$;

  isAuthenticated$ = false;

  ngOnInit(): void {
    this.oidcSecurityService.checkAuth().subscribe(({ isAuthenticated }) => {
      console.log('Authenticated:', isAuthenticated);
      this.isAuthenticated$ = isAuthenticated;

      this.oidcSecurityService.getAccessToken().subscribe(at => {
        console.log("ACCESS_TOKEN: " + at);
      });
    });

    // Manage dark/light-mode
    this.updateTheme();
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', event => {
      this.updateTheme();
    });
  }


  // Set theme to the user's preferred color scheme
  private updateTheme() {
    const colorMode = window.matchMedia("(prefers-color-scheme: dark)").matches ?
      "dark" :
      "light";
    document.querySelector("html")?.setAttribute("data-bs-theme", colorMode);
  }


  login(): void {
    this.oidcSecurityService.authorize();
  }

  logout(): void {
    // Clear session storage
    if (window.sessionStorage) {
      window.sessionStorage.clear();
    }

    window.location.href = "https://lpcalendar-dev-183771145359.auth.eu-central-1.amazoncognito.com/logout?client_id=1epkncdmjoklpcoa77pl17jatj&logout_uri=" + logoutRedirectUrl;
  }

  protected readonly environment = environment;
}
