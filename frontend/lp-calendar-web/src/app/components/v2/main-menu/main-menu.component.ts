import { Component, inject, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
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
  }


  loadMainMenuItems(): void {
    const items: MenuItem[] = [
      { label: 'Home', routerLink: '/home' },
      { label: 'Concerts', routerLink: '/concerts' },
      { label: 'Map', routerLink: '/map' },
      { label: 'About', routerLink: '/about' },
    ];

    // On mobile the "Get the app" CTA moves into the hamburger menu
    if (!this.isLoggedIn$) {
      items.push({
        label: 'Get the app',
        icon: 'pi pi-download',
        styleClass: 'menu-getapp-item',
        routerLink: '/app',
      });
    }

    this.mainMenuItems = items;
  }


  loadLoggedInMenuItems(): void {
    this.loggedInMenuItems = [
      {
        label: 'Songs & Setlists',
        items: [
          {
            label: 'Setlists',
            icon: 'pi pi-list',
            routerLink: '/admin/setlists',
            visible: this.canManageSetlists,
          },
          {
            label: 'Albums',
            icon: 'pi pi-images',
            routerLink: '/admin/albums',
            visible: this.canManageSetlists,
          },
          {
            label: 'Songs',
            icon: 'pi pi-headphones',
            routerLink: '/admin/songs',
            visible: this.canManageSetlists,
          },
          {
            label: 'Mashups',
            icon: 'pi pi-sliders-v',
            routerLink: '/admin/mashups',
            visible: this.canManageSetlists,
          },
        ],
        visible: this.canManageSetlists,
      },
      {
        label: 'Administration',
        items: [
          {
            label: 'Users',
            icon: 'pi pi-users',
            routerLink: '/users',
            visible: this.canManageUsers,
          }
        ],
        visible: this.canManageUsers,
      },
      {
        label: this.username ?? "No Username",
        items: [
          {
            label: "Your Profile",
            icon: "pi pi-user",
            routerLink: '/profile'
          },
          {
            label: "Logout",
            icon: "pi pi-sign-out",
            linkClass: '!text-red-500 dark:!text-red-400',
            command: (event => {
              this.logout()
            }),
          }
        ]
      },
    ];
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
