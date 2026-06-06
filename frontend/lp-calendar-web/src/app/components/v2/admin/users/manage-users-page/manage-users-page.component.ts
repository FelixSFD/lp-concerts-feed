import {Component, inject, OnInit} from '@angular/core';
import {ErrorResponseDto, UserDto} from '../../../../../modules/lpshows-api';
import {UsersService} from '../../../../../services/users.service';
import {Toast} from 'primeng/toast';
import {MessageService} from 'primeng/api';
import {Button} from 'primeng/button';
import {ButtonGroup} from 'primeng/buttongroup';
import {Card} from 'primeng/card';
import {IconField} from 'primeng/iconfield';
import {InputIcon} from 'primeng/inputicon';
import {InputText} from 'primeng/inputtext';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {TableModule} from 'primeng/table';
import {RouterLink} from '@angular/router';
import {NgbTooltip} from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-manage-users-page',
  imports: [
    Toast,
    Button,
    ButtonGroup,
    Card,
    IconField,
    InputIcon,
    InputText,
    ReactiveFormsModule,
    TableModule,
    RouterLink,
    FormsModule,
    NgbTooltip
  ],
  templateUrl: './manage-users-page.component.html',
  styleUrl: './manage-users-page.component.css',
})
export class ManageUsersPageComponent implements OnInit {
  private usersService = inject(UsersService);
  private messageService = inject(MessageService);

  users$: UserDto[] = [];

  // status if the table is currently loading
  isLoading$ = false;

  globalSearchText$: string = "";

  ngOnInit() {
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
        let errorResponse: ErrorResponseDto = err.error;
        console.warn("Failed to load users:", err);
        this.isLoading$ = false;

        this.messageService.add({ severity: 'error', summary: 'Could not load users!', text: errorResponse.message });
      }
    });
  }
}
