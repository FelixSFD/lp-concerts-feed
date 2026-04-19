import {Component, EventEmitter, inject, Input, OnChanges, OnInit, Output, SimpleChanges} from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import { NgClass } from '@angular/common';
import {UserDto, UserNotificationSettingsDto} from '../../../modules/lpshows-api';
import ConcertRemindersStatusEnum = UserNotificationSettingsDto.ConcertRemindersStatusEnum;
import MainStageTimeUpdatesStatusEnum = UserNotificationSettingsDto.MainStageTimeUpdatesStatusEnum;

@Component({
  selector: 'app-user-form',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgClass
],
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.css'
})
export class UserFormComponent implements OnInit, OnChanges {
  private formBuilder = inject(FormBuilder);

  userForm = this.formBuilder.group({
    email: new FormControl('', [Validators.required, Validators.email]),
    username: new FormControl('', [Validators.min(3), Validators.required]),
  });


  @Input({ alias: "is-saving" })
  isSaving$: boolean = false;

  @Input({ alias: "user" })
  user$ : UserDto | null = null;

  @Output('saveClicked')
  saveClicked = new EventEmitter<UserDto>();

  @Output('saveNotificationsClicked')
  saveNotificationsClicked = new EventEmitter<UserNotificationSettingsDto>();


  ngOnInit(): void {
    this.userForm.disable();

    if (this.user$ != null) {
      this.fillFormWithUser(this.user$);
      this.userForm.enable();
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

    if (changes.hasOwnProperty('user$')) {
      let change = changes['user$'];
      this.fillFormWithUser(change.currentValue);
    }
  }


  private fillFormWithUser(user: UserDto | null) {
    if (user != null) {
      this.userForm.enable();
    } else {
      this.userForm.disable();
    }
    this.userForm.controls.username.setValue(user?.username ?? "");
    this.userForm.controls.email.setValue(user?.email ?? "");
  }


  onSaveClicked() {
    let createdUser = this.readUserFromForm();
    console.debug("Emitting event for user");
    this.saveClicked.emit(createdUser);
  }


  private readUserFromForm() {
    let newUser: UserDto = {};
    newUser.username = this.userForm.controls.username.value ?? "";
    newUser.email = this.userForm.controls.email.value ?? "";

    return newUser;
  }
}
