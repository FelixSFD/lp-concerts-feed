import { Component } from '@angular/core';
import {UsersService} from '../../../services/users.service';
import {Concert} from '../../../data/concert';
import {User} from '../../../data/users/user';
import {NgForOf, NgIf} from '@angular/common';
import {ConcertTitleGenerator} from '../../../data/concert-title-generator';
import {DateTime} from 'luxon';
import {ConcertBadgesComponent} from '../../../concert-badges/concert-badges.component';
import {CountdownComponent} from '../../../countdown/countdown.component';
import {RouterLink} from '@angular/router';
import {NgbTooltip} from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-users-list',
  imports: [
    NgIf,
    ConcertBadgesComponent,
    CountdownComponent,
    NgForOf,
    RouterLink,
    NgbTooltip
  ],
  templateUrl: './users-list.component.html',
  styleUrl: './users-list.component.css'
})
export class UsersListComponent {
  users$: User[] = [];

  // status if the table is currently loading
  isLoading$ = false;


  constructor(private usersService: UsersService) {
    this.reloadUserList(false);
  }


  private reloadUserList(cache: boolean) {
    this.isLoading$ = true;
    this.usersService.getUsers(cache).subscribe(result => {
      this.users$ = result;
      this.isLoading$ = false;
    });
  }

  protected readonly ConcertTitleGenerator = ConcertTitleGenerator;
  protected readonly DateTime = DateTime;
}
