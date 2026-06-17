import {ActivatedRouteSnapshot, CanActivateFn, RouterStateSnapshot, Routes} from '@angular/router';
import {inject} from '@angular/core';
import {AuthService} from './auth/auth.service';
import {map} from 'rxjs';
import {concertResolver} from './resolvers/concert-resolver';
import {userResolver} from './resolvers/user-resolver';
import {albumResolver} from './resolvers/album-resolver';
import {songResolver} from './resolvers/song-resolver';

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
      import("./components/v2/home-page/home-page.component").then(m => m.HomePageComponent),
    title: baseTitle + 'Overview',
    data: {
      breadcrumb: 'Home',
    },
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
    data: {
      breadcrumb: 'Concerts',
    },
    children: [
      {
        path: '',
        loadComponent: () =>
          import("./components/v2/concert-list/concert-list.component").then(m => m.ConcertListComponent),
        title: baseTitle + 'List',
      },
      {
        path: 'add',
        loadComponent: () =>
          import("./admin/add-concert-page/add-concert-page.component").then(m => m.AddConcertPageComponent),
        title: baseTitle + 'Add concert',
        canActivate: [addConcertsGuard],
        data: {
          breadcrumb: 'Add concert',
        },
      },
      {
        path: 'import',
        loadComponent: () =>
          import("./admin/linkinpedia-concert-importer-page/linkinpedia-concert-importer-page.component").then(m => m.LinkinpediaConcertImporterPageComponent),
        title: baseTitle + 'Import concert',
        canActivate: [authGuard, addConcertsGuard, manageSetlistsGuard],
        data: {
          breadcrumb: 'Create from Linkinpedia',
        },
      },
      {
        path: ':id',
        data: {
          breadcrumb: 'Concert Details',
        },
        resolve: {
          concert: concertResolver,
        },
        children: [
          {
            path: '',
            loadComponent: () =>
              import("./components/v2/concert-details-page/concert-details-page.component").then(m => m.ConcertDetailsPageComponent),
            title: baseTitle + 'Details',
            data: {
              breadcrumb: 'Concert Details',
            },
          },
          {
            path: 'import',
            loadComponent: () =>
              import("./admin/linkinpedia-concert-importer-page/linkinpedia-concert-importer-page.component").then(m => m.LinkinpediaConcertImporterPageComponent),
            title: baseTitle + 'Import concert',
            canActivate: [authGuard, addConcertsGuard, manageSetlistsGuard],
            data: {
              breadcrumb: 'Import from Linkinpedia',
            },
          },
          {
            path: 'edit',
            loadComponent: () =>
              import("./admin/edit-concert-page/edit-concert-page.component").then(m => m.EditConcertPageComponent),
            title: baseTitle + 'Edit concert',
            canActivate: [updateConcertsGuard],
            data: {
              breadcrumb: 'Edit',
            },
          },
        ],
      },
    ]
  },
  {
    path: 'map',
    loadComponent: () =>
      import("./components/v2/tour-map-page/tour-map-page.component").then(m => m.TourMapPageComponent),
    title: baseTitle + 'Map',
    data: {
      breadcrumb: 'Map',
    },
  },
  {
    path: 'about',
    loadComponent: () =>
      import("./components/v2/about-page/about-page.component").then(m => m.AboutPageComponent),
    title: baseTitle + 'About',
    data: {
      breadcrumb: 'About',
    },
  },
  {
    path: 'users',
    data: {
      breadcrumb: 'Users',
    },
    canActivate: [authGuard, manageUsersGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import("./components/v2/admin/users/manage-users-page/manage-users-page.component").then(m => m.ManageUsersPageComponent),
        title: baseTitle + 'Manage users',
        data: {
          breadcrumb: 'Users',
        },
      },
      {
        path: ':id',
        loadComponent: () =>
          import("./admin/users/edit-user/edit-user.component").then(m => m.EditUserComponent),
        title: baseTitle + 'Edit user',
        canActivate: [authGuard, manageUsersGuard],
        data: {
          breadcrumb: 'Edit',
        },
        resolve: {
          user: userResolver,
        },
      },
    ],
  },
  {
    path: 'admin',
    loadComponent: () =>
      import("./components/admin/setlist-admin-wrapper/setlist-admin-wrapper.component").then(m => m.SetlistAdminWrapperComponent),
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
          import("./components/v2/admin/setlists/manage-mashups-page/manage-mashups-page.component").then(m => m.ManageMashupsPageComponent),
        title: baseTitle + 'Manage mashups',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'mashups/add',
        loadComponent: () =>
          import("./components/v2/admin/setlists/add-mashup-page/add-mashup-page.component").then(m => m.AddMashupPageComponent),
        title: baseTitle + 'Add mashup',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'mashups/:mashupId',
        loadComponent: () =>
          import("./components/v2/admin/setlists/edit-mashup-page/edit-mashup-page.component").then(m => m.EditMashupPageComponent),
        title: baseTitle + 'Edit mashup',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'setlists',
        loadComponent: () =>
          import("./components/v2/admin/setlists/manage-setlists-page/manage-setlists-page.component").then(m => m.ManageSetlistsPageComponent),
        title: baseTitle + 'Manage setlists',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'setlists/add',
        loadComponent: () =>
          import("./components/v2/admin/setlists/add-setlist-page/add-setlist-page.component").then(m => m.AddSetlistPageComponent),
        title: baseTitle + 'Create a new setlist',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'setlists/:setlistId',
        loadComponent: () =>
          import("./components/v2/admin/setlists/edit-setlist-page/edit-setlist-page.component").then(m => m.EditSetlistPageComponent),
        title: baseTitle + 'Edit setlist',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'setlists/add/:concertId',
        loadComponent: () =>
          import("./components/v2/admin/setlists/add-setlist-page/add-setlist-page.component").then(m => m.AddSetlistPageComponent),
        title: baseTitle + 'Create a new setlist',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'songs',
        loadComponent: () =>
          import("./components/v2/admin/setlists/manage-songs-page/manage-songs-page.component").then(m => m.ManageSongsPageComponent),
        title: baseTitle + 'Manage songs',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'songs/add',
        loadComponent: () =>
          import("./components/v2/admin/setlists/add-song-page/add-song-page.component").then(m => m.AddSongPageComponent),
        title: baseTitle + 'Add song',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'songs/:songId',
        loadComponent: () =>
          import("./components/v2/admin/setlists/edit-song-page/edit-song-page.component").then(m => m.EditSongPageComponent),
        title: baseTitle + 'Edit song',
        canActivate: [authGuard, manageSetlistsGuard],
        resolve: {
          song: songResolver,
        },
      },
      {
        path: 'albums',
        loadComponent: () =>
          import("./components/v2/admin/setlists/manage-albums-page/manage-albums-page.component").then(m => m.ManageAlbumsPageComponent),
        title: baseTitle + 'Manage albums',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'albums/add',
        loadComponent: () =>
          import("./components/v2/admin/setlists/add-album-page/add-album-page.component").then(m => m.AddAlbumPageComponent),
        title: baseTitle + 'Add album',
        canActivate: [authGuard, manageSetlistsGuard],
      },
      {
        path: 'albums/:id',
        loadComponent: () =>
          import("./components/v2/admin/setlists/edit-album-page/edit-album-page.component").then(m => m.EditAlbumPageComponent),
        title: baseTitle + 'Edit album',
        canActivate: [authGuard, manageSetlistsGuard],
        resolve: {
          album: albumResolver,
        },
      },
    ]
  },
  {
    path: 'app',
    loadComponent: () =>
      import("./components/v2/app-info-page/app-info-page.component").then(m => m.AppInfoPageComponent),
    title: baseTitle + 'iOS App',
    data: {
      breadcrumb: 'App'
    }
  }
];
