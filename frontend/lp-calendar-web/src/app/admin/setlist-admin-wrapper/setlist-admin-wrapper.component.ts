import { Component } from '@angular/core';
import {RouterLink, RouterLinkActive, RouterOutlet} from '@angular/router';

@Component({
  selector: 'app-setlist-admin-wrapper',
  imports: [
    RouterLink,
    RouterLinkActive,
    RouterOutlet
  ],
  templateUrl: './setlist-admin-wrapper.component.html',
  styleUrl: './setlist-admin-wrapper.component.css',
})
export class SetlistAdminWrapperComponent {

}
