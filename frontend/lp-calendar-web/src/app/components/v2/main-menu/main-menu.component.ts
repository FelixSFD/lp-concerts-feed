import { ChangeDetectionStrategy, Component, inject, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import {MenuItem} from 'primeng/api';
import { Router, RouterLink } from '@angular/router';
import {Menubar} from 'primeng/menubar';
import {NgOptimizedImage} from '@angular/common';
import {Button} from 'primeng/button';
import {DateTime} from 'luxon';
import {Menu} from 'primeng/menu';
import {AuthService} from '../../../auth/auth.service';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {environment} from '../../../../environments/environment';

@Component({
  selector: 'app-main-menu',
  imports: [
    Menubar,
    NgOptimizedImage,
    Button,
    Menu,
    RouterLink
  ],
  templateUrl: './main-menu.component.html',
  styleUrl: './main-menu.component.css',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class MainMenuComponent implements OnInit, OnChanges {
  private router = inject(Router);
  private readonly authStateService = inject(AuthService);
  private readonly oidcSecurityService = inject(OidcSecurityService);

  mainMenuItems: MenuItem[] | undefined;
  loggedInMenuItems: MenuItem[] | undefined;

  private username: string | null = null;
  private canManageUsers: boolean = false;
  private canManageSetlists: boolean = false;
  private canManageLocations: boolean = false;

  @Input("clock")
  currentDateTime$: DateTime = DateTime.now();

  @Input("isLoggedIn")
  isLoggedIn$: boolean = false;

  ngOnChanges(changes: SimpleChanges): void {
    // Rebuild only when the auth state flips
    if (changes['isLoggedIn$']) {
      this.loadMainMenuItems();
    }
  }

  ngOnInit() {
    this.loadMainMenuItems();

    this.loggedInMenuItems = [];

    this.authStateService.userData$.subscribe(userData => {
      this.username = userData?.username ?? null;

      this.loadLoggedInMenuItems();
    });

    this.authStateService.canManageUsers.subscribe(hasPermission => {
      this.canManageUsers = hasPermission;

      this.loadLoggedInMenuItems();
    });

    this.authStateService.canManageSetlists.subscribe(hasPermission => {
      this.canManageSetlists = hasPermission;

      this.loadLoggedInMenuItems();
    });

    this.authStateService.canManageLocations.subscribe(hasPermission => {
      this.canManageLocations = hasPermission;

      this.loadLoggedInMenuItems();
    });
  }


  loadMainMenuItems(): void {
    const items: MenuItem[] = [
      { id: 'home', label: 'Home', routerLink: '/home' },
      { id: 'concerts', label: 'Concerts', routerLink: '/concerts' },
      { id: 'map', label: 'Map', routerLink: '/map' },
      { id: 'about', label: 'About', routerLink: '/about' },
    ];

    // On mobile the "Get the app" CTA moves into the hamburger menu
    if (!this.isLoggedIn$) {
      items.push({
        id: 'get-app',
        label: 'Get the app',
        icon: 'pi pi-download',
        styleClass: 'menu-getapp-item',
        routerLink: '/app',
      });
    }

    this.mainMenuItems = items;
  }


  loadLoggedInMenuItems(): void {
    const items: MenuItem[] = [
      {
        label: 'TEST',
        routerLink: '/test'
      }
    ];

    /*
    if (this.canManageSetlists) {
      items.push({
        id: 'songs-and-setlists',
        label: 'Songs & Setlists',
        items: [
          { id: 'setlists', label: 'Setlists', icon: 'pi pi-list', routerLink: '/admin/setlists' },
          { id: 'albums', label: 'Albums', icon: 'pi pi-images', routerLink: '/admin/albums' },
          { id: 'songs', label: 'Songs', icon: 'pi pi-headphones', routerLink: '/admin/songs' },
          { id: 'mashups', label: 'Mashups', icon: 'pi pi-sliders-v', routerLink: '/admin/mashups' },
        ],
      });
    }

    if (this.canManageLocations) {
      items.push({
        id: 'locations',
        label: 'Locations',
        items: [
          { id: 'countries', label: 'Countries', icon: 'pi pi-globe', routerLink: '/admin/countries' },
          { id: 'cities', label: 'Cities', icon: 'pi pi-map-marker', routerLink: '/admin/cities' },
          { id: 'venues', label: 'Venues', icon: 'pi pi-warehouse', routerLink: '/admin/venues' },
        ],
      });
    }

    if (this.canManageUsers) {
      items.push({
        id: 'administration',
        label: 'Administration',
        items: [
          { id: 'users', label: 'Users', icon: 'pi pi-users', routerLink: '/users' },
        ],
      });
    }*/

    /*items.push({
      id: 'account',
      label: this.username ?? 'Account',
      items: [
        { id: 'your-profile', label: 'Your Profile', icon: 'pi pi-user', routerLink: '/profile' },
        {
          id: 'logout',
          label: 'Logout',
          icon: 'pi pi-sign-out',
          linkClass: '!text-red-500 dark:!text-red-400',
          command: (event => {
            this.logout()
          }),
        },
      ],
    });*/

    this.loggedInMenuItems = items;
  }


  get userInitial(): string | null {
    const name = this.username?.trim();
    return name ? name.charAt(0).toUpperCase() : null;
  }

  login(): void {
    this.oidcSecurityService.authorize();
  }

  logout(): void {
    this.oidcSecurityService.logoffLocal();
    window.location.href = environment.cognitoLogoutUrl;
  }

  protected readonly DateTime = DateTime;
}
