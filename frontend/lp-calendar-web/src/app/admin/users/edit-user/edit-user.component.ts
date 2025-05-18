import {Component, inject, OnInit} from '@angular/core';
import {User} from '../../../data/users/user';
import {environment} from '../../../../environments/environment';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {ToastrService} from 'ngx-toastr';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {UsersService} from '../../../services/users.service';
import {UserFormComponent} from '../user-form/user-form.component';


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

  constructor(private route: ActivatedRoute, private userService: UsersService, private toastr: ToastrService) {

  }


  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.userId$ = params['id'];
    })
  }


  onFormSaved(user: User) {
    console.log("Received event for user", user);
  }
}
