import {Component, inject, OnInit} from '@angular/core';
import {MenuItem} from 'primeng/api';
import {Router} from '@angular/router';
import {Menubar} from 'primeng/menubar';
import {NgOptimizedImage} from '@angular/common';

@Component({
  selector: 'app-main-menu',
  imports: [
    Menubar,
    NgOptimizedImage
  ],
  templateUrl: './main-menu.component.html',
  styleUrl: './main-menu.component.css',
})
export class MainMenuComponent implements OnInit {
  private router = inject(Router);
  items: MenuItem[] | undefined;

  ngOnInit() {
    this.items = [
      {
        label: 'Home',
        icon: 'pi pi-home',
        routerLink: '/home'
      },
      {
        label: 'Concerts',
        icon: 'pi pi-calendar',
        routerLink: '/concerts'
      },
      {
        label: 'Map',
        icon: 'pi pi-map',
        routerLink: '/map'
      },
      {
        label: 'App',
        icon: 'pi pi-mobile',
        routerLink: '/app'
      },
      {
        label: 'About',
        icon: 'pi pi-info-circle',
        routerLink: '/about'
      },
    ];
  }
}
