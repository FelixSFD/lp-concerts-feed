import { Routes } from '@angular/router';
import {UserProfileComponent} from './user-profile/user-profile.component';
import {HomeComponent} from './home/home.component';
import {ConcertsListComponent} from './concerts-list/concerts-list.component';
import {ConcertDetailsComponent} from './concert-details/concert-details.component';
import {AboutPageComponent} from './about-page/about-page.component';

export const routes: Routes = [
  {
    path: '',
    component: HomeComponent,
    title: 'Startpage',
  },
  {
    path: 'home',
    component: HomeComponent,
    title: 'Startpage',
  },
  {
    path: 'profile',
    component: UserProfileComponent,
    title: 'User Profile',
  },
  {
    path: 'concerts',
    component: ConcertsListComponent,
    title: 'Concerts',
  },
  {
    path: 'concerts/:id',
    component: ConcertDetailsComponent,
    title: 'Concert Details',
  },
  {
    path: 'about',
    component: AboutPageComponent,
    title: 'About',
  },
];
