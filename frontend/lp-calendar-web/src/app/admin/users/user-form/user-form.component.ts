import {Component, EventEmitter, inject, Input, OnChanges, OnInit, Output, SimpleChanges} from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {ToastrService} from 'ngx-toastr';
import {Concert} from '../../../data/concert';
import {User} from '../../../data/users/user';
import {UsersService} from '../../../services/users.service';
import {NgClass, NgIf} from '@angular/common';

@Component({
  selector: 'app-user-form',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgIf,
    NgClass
  ],
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.css'
})
export class UserFormComponent implements OnInit, OnChanges {
  private formBuilder = inject(FormBuilder);
  private toastrService = inject(ToastrService);
  private userService = inject(UsersService);

  userForm = this.formBuilder.group({
    email: new FormControl('', [Validators.required, Validators.email]),
    username: new FormControl('', [Validators.min(3), Validators.required]),
  });


  @Input({ alias: "user-id" })
  userId: string | null = null;

  @Input({ alias: "is-saving" })
  isSaving$: boolean = false;

  @Output('saveClicked')
  saveClicked = new EventEmitter<User>();

  user$ : User | null = null;


  ngOnInit(): void {
    if (this.userId != null) {
      this.loadUser(this.userId);
    }
  }


  ngOnChanges(changes: SimpleChanges) {
    if (changes.hasOwnProperty('isSaving$')) {
      let change = changes['isSaving$'];
      if (change.currentValue == true) {
        this.userForm.disable();
      } else {
        this.userForm.enable();
      }
    }
  }


  private loadUser(id: string) {
    this.userForm.disable();

    this.userService.getUserById(id).subscribe(user => {
      this.user$ = user;
      this.fillFormWithUser(user);

      this.userForm.enable();
    });
  }


  private fillFormWithUser(user: User) {
    this.userForm.controls.username.setValue(user.username ?? "");
    this.userForm.controls.email.setValue(user.email ?? "");
  }


  onSaveClicked() {
    let createdUser = this.readUserFromForm();
    console.log("Emitting event for user");
    this.saveClicked.emit(createdUser);
  }


  private readUserFromForm() {
    let newUser = new User();
    newUser.username = this.userForm.controls.username.value ?? "";
    newUser.email = this.userForm.controls.email.value ?? "";

    return newUser;
  }
}
