import {Component, inject, OnInit} from '@angular/core';
import {User} from '../../../data/users/user';
import {environment} from '../../../../environments/environment';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {ToastrService} from 'ngx-toastr';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {UsersService} from '../../../services/users.service';
import {UserFormComponent} from '../user-form/user-form.component';
import {ErrorResponse} from '../../../data/error-response';


@Component({
  selector: 'app-edit-user',
  imports: [
    UserFormComponent,
    RouterLink
  ],
  templateUrl: './edit-user.component.html',
  styleUrl: './edit-user.component.css'
})
export class EditUserComponent implements OnInit {
  private readonly oidcSecurityService = inject(OidcSecurityService);

  userId$: string = "";
  user$ : User | null = null;

  // true while the user is saved on the server
  userIsSaving$: boolean = false;

  constructor(private route: ActivatedRoute, private userService: UsersService, private toastr: ToastrService) {

  }


  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.userId$ = params['id'];
      this.loadUser(this.userId$);
    })
  }


  private loadUser(id: string) {
    this.userService.getUserById(id).subscribe({
      next: user => {
        this.user$ = user;
      },
      error: err => {
        let errorResponse: ErrorResponse = err.error;
        this.toastr.error(errorResponse.message, "Could not load user");
      }
    });
  }


  onFormSaved(user: User) {
    console.log("Received event for user", user);
    this.userIsSaving$ = true;
    user.id = this.userId$;
    this.userService.updateUser(user).subscribe({
      next: response => {
        this.toastr.success("User updated successfully");
        this.userIsSaving$ = false;
      },
      error: err => {
        let errorResponse: ErrorResponse = err.error;
        this.toastr.error(errorResponse.message, "Could not save user");
        this.userIsSaving$ = false;
      }
    });
  }
}
