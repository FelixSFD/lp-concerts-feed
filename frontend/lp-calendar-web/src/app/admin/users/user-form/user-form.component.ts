import {Component, EventEmitter, inject, Input, OnChanges, OnInit, Output, SimpleChanges} from '@angular/core';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {NgClass, NgIf} from '@angular/common';
import {UserDto, UserNotificationSettingsDto} from '../../../modules/lpshows-api';

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

  userForm = this.formBuilder.group({
    email: new FormControl('', [Validators.required, Validators.email]),
    username: new FormControl('', [Validators.min(3), Validators.required]),
  });


  notificationsForm = this.formBuilder.group({
    receiveConcertReminders: new FormControl(false, []),
    receiveMainStageTimeUpdates: new FormControl(false, []),
  });


  @Input({ alias: "is-saving" })
  isSaving$: boolean = false;

  @Input({ alias: "is-saving-notifications" })
  isSavingNotifications$: boolean = false;

  @Input({ alias: "user" })
  user$ : UserDto | null = null;

  @Input({ alias: "notifications" })
  userNotificationSettings$ : UserNotificationSettingsDto | null = null;

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

    if (changes.hasOwnProperty('userNotificationSettings$')) {
      let change = changes['userNotificationSettings$'];
      this.fillFormWithNotificationSettings(change.currentValue);
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


  private fillFormWithNotificationSettings(settings: UserNotificationSettingsDto | null) {
    if (settings != null) {
      this.notificationsForm.enable();
    } else {
      this.notificationsForm.disable();
    }
    this.notificationsForm.controls.receiveConcertReminders.setValue(settings?.receiveConcertReminders ?? false);
    this.notificationsForm.controls.receiveMainStageTimeUpdates.setValue(settings?.receiveMainStageTimeUpdates ?? false);
  }


  onSaveClicked() {
    let createdUser = this.readUserFromForm();
    console.debug("Emitting event for user");
    this.saveClicked.emit(createdUser);
  }


  onSaveNotificationsClicked() {
    let settings = this.readNotificationSettingsFromForm();
    console.debug("Emitting event for notification settings");
    this.saveNotificationsClicked.emit(settings);
  }


  private readUserFromForm() {
    let newUser: UserDto = {};
    newUser.username = this.userForm.controls.username.value ?? "";
    newUser.email = this.userForm.controls.email.value ?? "";

    return newUser;
  }


  private readNotificationSettingsFromForm() {
    let newSettings: UserNotificationSettingsDto = {};
    newSettings.receiveConcertReminders = this.notificationsForm.controls.receiveConcertReminders.value ?? false;
    newSettings.receiveMainStageTimeUpdates = this.notificationsForm.controls.receiveMainStageTimeUpdates.value ?? false;

    return newSettings;
  }
}
