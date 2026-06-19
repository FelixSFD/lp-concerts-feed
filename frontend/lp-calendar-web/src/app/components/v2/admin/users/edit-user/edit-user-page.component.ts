import {Component, inject, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {UsersService} from '../../../../../services/users.service';
import {UserFormComponent} from '../../../../../admin/users/user-form/user-form.component';
import {ErrorResponseDto, UserDto} from '../../../../../modules/lpshows-api';
import {Message} from 'primeng/message';
import {Card} from 'primeng/card';
import {MessageService} from 'primeng/api';


@Component({
  selector: 'app-edit-user-page',
  imports: [
    UserFormComponent,
    Message,
    Card
  ],
  templateUrl: './edit-user-page.component.html',
  styleUrl: './edit-user-page.component.css',
  standalone: true,
})
export class EditUserPageComponent implements OnInit {
  private messageService = inject(MessageService);

  user$ : UserDto | null = null;
  resolverError$: ErrorResponseDto | null = null;

  // true while the user is saved on the server
  userIsSaving$: boolean = false;

  constructor(private route: ActivatedRoute, private userService: UsersService) {

  }


  ngOnInit(): void {
    this.route.data.subscribe(data => {
      console.debug("Resolved data:", data);

      if (data['user'].type === 'ErrorResponseDto') {
        this.resolverError$ = data['user'];

        return;
      }

      this.user$ = data['user'];
    });
  }


  onFormSaved(user: UserDto) {
    console.log("Received event for user", user);
    this.userIsSaving$ = true;
    user.id = this.user$?.id;
    this.userService.updateUser(user).subscribe({
      next: response => {
        this.messageService.add({
          severity: 'success',
          summary: 'User updated successfully',
        });
        this.userIsSaving$ = false;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({
          severity: 'danger',
          summary: 'Could not save user',
          text: errorResponse.message,
        });
        this.userIsSaving$ = false;
      }
    });
  }
}
