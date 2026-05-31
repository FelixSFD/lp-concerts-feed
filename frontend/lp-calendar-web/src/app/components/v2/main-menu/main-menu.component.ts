import {Component, inject, Input, OnInit} from '@angular/core';
import {MenuItem} from 'primeng/api';
import {Router} from '@angular/router';
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
    Menu
  ],
  templateUrl: './main-menu.component.html',
  styleUrl: './main-menu.component.css',
})
export class MainMenuComponent implements OnInit {
  private router = inject(Router);
  private readonly authStateService = inject(AuthService);
  private readonly oidcSecurityService = inject(OidcSecurityService);

  mainMenuItems: MenuItem[] | undefined;
  loggedInMenuItems: MenuItem[] | undefined;

  private username: string | null = null;
  private canManageUsers: boolean = false;
  private canManageSetlists: boolean = false;

  // the current clock
  @Input("clock")
  currentDateTime$: DateTime = DateTime.now();

  @Input("isLoggedIn")
  isLoggedIn$: boolean = false;

  ngOnInit() {
    this.mainMenuItems = [
      {
        label: 'Home',
        icon: 'pi pi-home',
        routerLink: '/home',
      },
      {
        label: 'Concerts',
        icon: 'pi pi-calendar',
        routerLink: '/concerts'
      },
      {
        label: 'Map',
        icon: 'pi pi-map',
        routerLink: '/map'
      },
      {
        label: 'App',
        icon: 'pi pi-mobile',
        routerLink: '/app'
      },
      {
        label: 'About',
        icon: 'pi pi-info-circle',
        routerLink: '/about'
      },
    ];

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
