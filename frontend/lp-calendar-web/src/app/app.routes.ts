import { Routes } from '@angular/router';
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

let baseTitle = "LP Concerts - ";

export const routes: Routes = [
  {
    path: '',
    component: HomeComponent,
    title: baseTitle + 'Overview',
  },
  {
    path: 'home',
    component: HomeComponent,
    title: baseTitle + 'Overview',
    redirectTo: ''
  },
  {
    path: 'privacy',
    component: PrivacyPolicyComponent,
    title: baseTitle + 'Privacy Policy'
  },
  {
    path: 'imprint',
    component: ImprintComponent,
    title: baseTitle + 'Imprint'
  },
  {
    path: 'profile',
    component: UserProfileComponent,
    title: baseTitle + 'User Profile',
  },
  {
    path: 'concerts',
    component: ConcertsListComponent,
    title: baseTitle + 'List',
  },
  {
    path: 'map',
    component: TourMapPageComponent,
    title: baseTitle + 'Map',
  },
  {
    path: 'about',
    component: AboutPageComponent,
    title: baseTitle + 'About',
  },
  {
    path: 'concerts/add',
    component: AddConcertPageComponent,
    title: baseTitle + 'Add concert',
  },
  {
    path: 'concerts/:id',
    component: ConcertDetailsComponent,
    title: baseTitle + 'Details',
  },
  {
    path: 'concerts/:id/edit',
    component: EditConcertPageComponent,
    title: baseTitle + 'Edit concert',
  },
  {
    path: 'users',
    component: UsersListComponent,
    title: baseTitle + 'Manage users',
  },
  {
    path: 'users/:id',
    component: EditUserComponent,
    title: baseTitle + 'Edit user',
  },
  {
    path: 'admin/mashups',
    component: ManageMashupsPageComponent,
    title: baseTitle + 'Manage mashups',
  },
  {
    path: 'admin/mashups/add',
    component: AddMashupPageComponent,
    title: baseTitle + 'Add mashup',
  },
  {
    path: 'admin/mashups/:mashupId',
    component: EditMashupPageComponent,
    title: baseTitle + 'Edit mashup',
  },
  {
    path: 'admin/setlists',
    component: ManageSetlistsPageComponent,
    title: baseTitle + 'Manage setlists',
  },
  {
    path: 'admin/setlists/:setlistId',
    component: EditSetlistPageComponent,
    title: baseTitle + 'Edit setlist',
  },
  {
    path: 'admin/setlists/add',
    component: AddSetlistPageComponent,
    title: baseTitle + 'Create a new setlist',
  },
  {
    path: 'admin/setlists/add/:concertId',
    component: AddSetlistPageComponent,
    title: baseTitle + 'Create a new setlist',
  },
  {
    path: 'admin/songs',
    component: ManageSongsPageComponent,
    title: baseTitle + 'Manage songs',
  },
  {
    path: 'admin/songs/add',
    component: AddSongPageComponent,
    title: baseTitle + 'Add song',
  },
  {
    path: 'admin/songs/:songId',
    component: EditSongPageComponent,
    title: baseTitle + 'Edit song',
  },
  {
    path: 'admin/albums',
    component: ManageAlbumsPageComponent,
    title: baseTitle + 'Manage albums',
  },
  {
    path: 'admin/albums/add',
    component: AddAlbumPageComponent,
    title: baseTitle + 'Add album',
  },
  {
    path: 'app',
    component: AppInfoPageComponent,
    title: baseTitle + 'iOS App',
  },
];
