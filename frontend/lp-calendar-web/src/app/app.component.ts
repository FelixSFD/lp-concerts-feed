import {Component, inject} from '@angular/core';
import {RouterLink, RouterOutlet} from '@angular/router';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {AsyncPipe, JsonPipe, NgIf} from '@angular/common';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NgbModule, NgIf, AsyncPipe, JsonPipe, RouterLink],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'lp-calendar-web';

  private readonly oidcSecurityService = inject(OidcSecurityService);

  configuration$ = this.oidcSecurityService.getConfiguration();

  userData$ = this.oidcSecurityService.userData$;

  isAuthenticated$ = false;

  ngOnInit(): void {
    this.oidcSecurityService.checkAuth().subscribe(({ isAuthenticated }) => {
      console.log('Authenticated:', isAuthenticated);
    });
  }

  login(): void {
    this.oidcSecurityService.authorize();
  }
}
