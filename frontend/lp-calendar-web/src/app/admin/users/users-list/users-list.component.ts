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
import {ToastrService} from 'ngx-toastr';
import {ErrorResponse} from '../../../data/error-response';

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


  constructor(private usersService: UsersService, private toastr: ToastrService) {
    this.reloadUserList(false);
  }


  private reloadUserList(cache: boolean) {
    this.isLoading$ = true;
    this.usersService.getUsers(cache).subscribe({
      next: result => {
        this.users$ = result;
        this.isLoading$ = false;
      },
      error: err => {
        let errorResponse: ErrorResponse = err.error;
        console.warn("Failed to load users:", err);
        this.isLoading$ = false;

        this.toastr.error(errorResponse.message, "Could not load users!");
      }
    });
  }

  protected readonly ConcertTitleGenerator = ConcertTitleGenerator;
  protected readonly DateTime = DateTime;
}
