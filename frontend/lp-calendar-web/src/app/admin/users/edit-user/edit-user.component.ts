import {Component, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {ToastrService} from 'ngx-toastr';
import {UsersService} from '../../../services/users.service';
import {UserFormComponent} from '../user-form/user-form.component';
import {ErrorResponseDto, UserDto} from '../../../modules/lpshows-api';
import {CommandError} from '@angular/cli/src/commands/mcp/host';
import {Message} from 'primeng/message';


@Component({
  selector: 'app-edit-user',
  imports: [
    UserFormComponent,
    Message
  ],
  templateUrl: './edit-user.component.html',
  styleUrl: './edit-user.component.css',
  standalone: true,
})
export class EditUserComponent implements OnInit {
  user$ : UserDto | null = null;
  resolverError$: CommandError | null = null;

  // true while the user is saved on the server
  userIsSaving$: boolean = false;

  constructor(private route: ActivatedRoute, private userService: UsersService, private toastr: ToastrService) {

  }


  ngOnInit(): void {
    this.route.data.subscribe(data => {
      console.debug("Resolved data:", data);

      if (data['user'] instanceof CommandError) {
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
        this.toastr.success("User updated successfully");
        this.userIsSaving$ = false;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.toastr.error(errorResponse.message, "Could not save user");
        this.userIsSaving$ = false;
      }
    });
  }
}
