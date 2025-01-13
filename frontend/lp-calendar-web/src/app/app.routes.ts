import { Routes } from '@angular/router';
import {UserProfileComponent} from './user-profile/user-profile.component';
import {HomeComponent} from './home/home.component';

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
];
