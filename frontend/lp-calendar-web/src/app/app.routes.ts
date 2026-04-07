import {ActivatedRouteSnapshot, CanActivateFn, RouterStateSnapshot, Routes} from '@angular/router';
import {UserProfileComponent} from './user-profile/user-profile.component';
import {HomeComponent} from './home/home.component';
import {ConcertsListComponent} from './concerts-list/concerts-list.component';
import {ConcertDetailsComponent} from './concert-details/concert-details.component';
import {AboutPageComponent} from './about-page/about-page.component';
import {EditConcertPageComponent} from './admin/edit-concert-page/edit-concert-page.component';
import {AddConcertPageComponent} from './admin/add-concert-page/add-concert-page.component';
import {TourMapPageComponent} from './tour-map-page/tour-map-page.component';
import {PrivacyPolicyComponent} from './privacy-policy/privacy-policy.component';
import {ImprintComponent} from './imprint/imprint.component';
import {UsersListComponent} from './admin/users/users-list/users-list.component';
import {EditUserComponent} from './admin/users/edit-user/edit-user.component';
import {AppInfoPageComponent} from './app-info-page/app-info-page.component';
import {AddSetlistPageComponent} from './admin/setlists/add-setlist-page/add-setlist-page.component';
import {ManageSetlistsPageComponent} from './admin/setlists/manage-setlists-page/manage-setlists-page.component';
import {EditSetlistPageComponent} from './admin/setlists/edit-setlist-page/edit-setlist-page.component';
import {ManageMashupsPageComponent} from './admin/setlists/manage-mashups-page/manage-mashups-page.component';
import {AddMashupPageComponent} from './admin/setlists/add-mashup-page/add-mashup-page.component';
import {EditMashupPageComponent} from './admin/setlists/edit-mashup-page/edit-mashup-page.component';
import {ManageSongsPageComponent} from './admin/setlists/manage-songs-page/manage-songs-page.component';
import {AddSongPageComponent} from './admin/setlists/add-song-page/add-song-page.component';
import {EditSongPageComponent} from './admin/setlists/edit-song-page/edit-song-page.component';
import {ManageAlbumsPageComponent} from './admin/setlists/manage-albums-page/manage-albums-page.component';
import {AddAlbumPageComponent} from './admin/setlists/add-album-page/add-album-page.component';
import {EditAlbumPageComponent} from './admin/setlists/edit-album-page/edit-album-page.component';
import {SetlistAdminWrapperComponent} from './admin/setlist-admin-wrapper/setlist-admin-wrapper.component';
import {inject} from '@angular/core';
import {AuthService} from './auth/auth.service';
import {map} from 'rxjs';
import {
  LinkinpediaConcertImporterPageComponent
} from './admin/linkinpedia-concert-importer-page/linkinpedia-concert-importer-page.component';

let baseTitle = "LP Concerts - ";

export const authGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot,
) => {
  const authService = inject(AuthService);
  return authService.isAuthenticated$;
};

export const adminGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot,
) => {
  const authService = inject(AuthService);
  return authService.currentUserGroups.pipe(map(groups => groups.some(g => g == "Admin")));
};

export const manageUsersGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot,
) => {
  const authService = inject(AuthService);
  return authService.canManageUsers;
};

export const addConcertsGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot,
) => {
  const authService = inject(AuthService);
  return authService.currentUserGroups.pipe(map(groups => groups.some(g => g == "AddConcerts" || g == "Admin")));
};

export const updateConcertsGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot,
) => {
  const authService = inject(AuthService);
  return authService.currentUserGroups.pipe(map(groups => groups.some(g => g == "UpdateConcerts" || g == "Admin")));
};

export const manageSetlistsGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot,
) => {
  const authService = inject(AuthService);
  return authService.canManageSetlists;
};

export const routes: Routes = [
  {
    path: 'home',
    redirectTo: '',
  },
  {
    path: '',
    loadComponent: () =>
      import("./home/home.component").then(m => m.HomeComponent),
    title: baseTitle + 'Overview',
  },
  {
    path: 'privacy',
    loadComponent: () =>
      import("./privacy-policy/privacy-policy.component").then(m => m.PrivacyPolicyComponent),
    title: baseTitle + 'Privacy Policy'
  },
  {
    path: 'imprint',
    loadComponent: () =>
      import("./imprint/imprint.component").then(m => m.ImprintComponent),
    title: baseTitle + 'Imprint'
  },
  {
    path: 'profile',
    loadComponent: () =>
      import("./user-profile/user-profile.component").then(m => m.UserProfileComponent),
    title: baseTitle + 'User Profile',
  },
  {
    path: 'concerts',
    loadComponent: () =>
      import("./concerts-list/concerts-list.component").then(m => m.ConcertsListComponent),
    title: baseTitle + 'List',
  },
  {
    path: 'map',
    loadComponent: () =>
      import("./tour-map-page/tour-map-page.component").then(m => m.TourMapPageComponent),
    title: baseTitle + 'Map',
  },
  {
    path: 'about',
    loadComponent: () =>
      import("./about-page/about-page.component").then(m => m.AboutPageComponent),
    title: baseTitle + 'About',
  },
  {
    path: 'concerts/add',
    loadComponent: () =>
      import("./admin/add-concert-page/add-concert-page.component").then(m => m.AddConcertPageComponent),
    title: baseTitle + 'Add concert',
    canActivate: [addConcertsGuard]
  },
  {
    path: 'concerts/:id',
    loadComponent: () =>
      import("./concert-details/concert-details.component").then(m => m.ConcertDetailsComponent),
    title: baseTitle + 'Details',
  },
  {
    path: 'concerts/:id/edit',
    loadComponent: () =>
      import("./admin/edit-concert-page/edit-concert-page.component").then(m => m.EditConcertPageComponent),
    title: baseTitle + 'Edit concert',
    canActivate: [updateConcertsGuard]
  },
  {
    path: 'users',
    loadComponent: () =>
      import("./admin/users/users-list/users-list.component").then(m => m.UsersListComponent),
    title: baseTitle + 'Manage users',
    canActivate: [authGuard, manageUsersGuard],
  },
  {
    path: 'users/:id',
    loadComponent: () =>
      import("./admin/users/edit-user/edit-user.component").then(m => m.EditUserComponent),
    title: baseTitle + 'Edit user',
    canActivate: [authGuard, manageUsersGuard],
  },
  {
    path: 'admin',
    loadComponent: () =>
      import("./admin/setlist-admin-wrapper/setlist-admin-wrapper.component").then(m => m.SetlistAdminWrapperComponent),
    canActivateChild: [authGuard, manageSetlistsGuard],
    children: [
      {
        path: '',
        redirectTo: 'setlists',
        pathMatch: 'full',
      },
      {
        path: 'mashups',
        loadComponent: () =>
          import("./admin/setlists/manage-mashups-page/manage-mashups-page.component").then(m => m.ManageMashupsPageComponent),
        title: baseTitle + 'Manage mashups',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'mashups/add',
        loadComponent: () =>
          import("./admin/setlists/add-mashup-page/add-mashup-page.component").then(m => m.AddMashupPageComponent),
        title: baseTitle + 'Add mashup',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'mashups/:mashupId',
        loadComponent: () =>
          import("./admin/setlists/edit-mashup-page/edit-mashup-page.component").then(m => m.EditMashupPageComponent),
        title: baseTitle + 'Edit mashup',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'setlists',
        loadComponent: () =>
          import("./admin/setlists/manage-setlists-page/manage-setlists-page.component").then(m => m.ManageSetlistsPageComponent),
        title: baseTitle + 'Manage setlists',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'setlists/:setlistId',
        loadComponent: () =>
          import("./admin/setlists/edit-setlist-page/edit-setlist-page.component").then(m => m.EditSetlistPageComponent),
        title: baseTitle + 'Edit setlist',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'setlists/add',
        loadComponent: () =>
          import("./admin/setlists/add-setlist-page/add-setlist-page.component").then(m => m.AddSetlistPageComponent),
        title: baseTitle + 'Create a new setlist',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'setlists/add/:concertId',
        loadComponent: () =>
          import("./admin/setlists/add-setlist-page/add-setlist-page.component").then(m => m.AddSetlistPageComponent),
        title: baseTitle + 'Create a new setlist',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'songs',
        loadComponent: () =>
          import("./admin/setlists/manage-songs-page/manage-songs-page.component").then(m => m.ManageSongsPageComponent),
        title: baseTitle + 'Manage songs',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'songs/add',
        loadComponent: () =>
          import("./admin/setlists/add-song-page/add-song-page.component").then(m => m.AddSongPageComponent),
        title: baseTitle + 'Add song',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'songs/:songId',
        loadComponent: () =>
          import("./admin/setlists/edit-song-page/edit-song-page.component").then(m => m.EditSongPageComponent),
        title: baseTitle + 'Edit song',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'albums',
        loadComponent: () =>
          import("./admin/setlists/manage-albums-page/manage-albums-page.component").then(m => m.ManageAlbumsPageComponent),
        title: baseTitle + 'Manage albums',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'albums/add',
        loadComponent: () =>
          import("./admin/setlists/add-album-page/add-album-page.component").then(m => m.AddAlbumPageComponent),
        title: baseTitle + 'Add album',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'albums/:albumId',
        loadComponent: () =>
          import("./admin/setlists/edit-album-page/edit-album-page.component").then(m => m.EditAlbumPageComponent),
        title: baseTitle + 'Edit album',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'concerts/import',
        loadComponent: () =>
          import("./admin/linkinpedia-concert-importer-page/linkinpedia-concert-importer-page.component").then(m => m.LinkinpediaConcertImporterPageComponent),
        title: baseTitle + 'Import concert',
        canActivate: [authGuard, addConcertsGuard, manageSetlistsGuard],
      },
    ]
  },
  {
    path: 'app',
    loadComponent: () =>
      import("./app-info-page/app-info-page.component").then(m => m.AppInfoPageComponent),
    title: baseTitle + 'iOS App',
  }
];
