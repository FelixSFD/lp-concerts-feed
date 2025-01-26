import { Routes } from '@angular/router';
import {UserProfileComponent} from './user-profile/user-profile.component';
import {HomeComponent} from './home/home.component';
import {ConcertsListComponent} from './concerts-list/concerts-list.component';
import {ConcertDetailsComponent} from './concert-details/concert-details.component';
import {AboutPageComponent} from './about-page/about-page.component';
import {EditConcertPageComponent} from './admin/edit-concert-page/edit-concert-page.component';

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
    path: 'concerts/:id',
    component: ConcertDetailsComponent,
    title: baseTitle + 'Details',
  },
  {
    path: 'about',
    component: AboutPageComponent,
    title: baseTitle + 'About',
  },
  {
    path: 'concerts/:id/edit',
    component: EditConcertPageComponent,
    title: baseTitle + 'Edit concert',
  },
];
