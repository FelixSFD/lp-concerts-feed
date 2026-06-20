import {Component, inject} from '@angular/core';
import {Router, RouterOutlet} from '@angular/router';
import {SelectButton} from 'primeng/selectbutton';
import {MenuItem} from 'primeng/api';
import {FormsModule} from '@angular/forms';
import {Divider} from 'primeng/divider';

@Component({
  selector: 'app-setlist-admin-wrapper',
  imports: [
    RouterOutlet,
    SelectButton,
    FormsModule,
    Divider
  ],
  templateUrl: './setlist-admin-wrapper.component.html',
  styleUrl: './setlist-admin-wrapper.component.css',
})
export class SetlistAdminWrapperComponent {
  private router = inject(Router);

  menuItems: MenuItem[] = [
    {
      label: "Setlists",
      routerLink: "/admin/setlists",
    },
    {
      label: "Albums",
      routerLink: "/admin/albums",
    },
    {
      label: "Songs",
      routerLink: "/admin/songs",
    },
    {
      label: "Mashups",
      routerLink: "/admin/mashups",
    }
  ];

  selectedArea = this.menuItems.find(
    s => s.routerLink === this.router.url
  );

  navigate(area: MenuItem) {
    this.router.navigateByUrl(area.routerLink).then().catch(error => {
      console.error("Failed to navigate!", area, error);
    });
  }
}
