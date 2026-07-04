import {Component, inject, Injectable, OnInit} from '@angular/core';
import {routes} from '../../../app.routes';
import {MenuItem} from 'primeng/api';
import {Breadcrumb} from 'primeng/breadcrumb';
import {ActivatedRoute, NavigationEnd, Router} from '@angular/router';
import {filter} from 'rxjs';

@Component({
  selector: 'app-auto-breadcrumbs',
  imports: [
    Breadcrumb
  ],
  templateUrl: './auto-breadcrumbs.component.html',
  styleUrl: './auto-breadcrumbs.component.css',
})
export class AutoBreadcrumbsComponent implements OnInit {
  breadcrumbService = inject(BreadcrumbService);

  // Breadcrumbs
  home: MenuItem | undefined;


  ngOnInit() {
    if (!this.home) {
      this.home = {
        icon: 'pi pi-home',
        routerLink: '/',
      };
    }
  }
}


@Injectable({ providedIn: 'root' })
export class BreadcrumbService {
  breadcrumbs: MenuItem[] = [];

  constructor(private router: Router, private activatedRoute: ActivatedRoute) {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event) => {
        let allBc = this.buildBreadcrumb(this.activatedRoute);
        console.log("All Breadcrumbs: ", allBc);
        this.breadcrumbs = allBc;
      });
  }

  buildBreadcrumb(currentAR: ActivatedRoute, url: string = '', breadcrumbs: MenuItem[] = []): MenuItem[] {
    let nextUrlPart = currentAR.snapshot.url.map(segment => segment.path).filter(p => p.length > 0).join("/");
    if (nextUrlPart.startsWith('/')) {
      nextUrlPart = nextUrlPart.substring(0, nextUrlPart.length - 1);
    }
    console.debug("nextUrlPart: ", nextUrlPart);

    if (nextUrlPart.length == 0) {
      console.debug("nextUrlPart was empty. Skipping this part.");
    } else {
      if (nextUrlPart != '/') {
        url = url + "/" + nextUrlPart;
      }

      console.debug("Building Breadcrumb", currentAR.snapshot, url);
      if (currentAR.snapshot.data['breadcrumb']) {
        breadcrumbs.push({
          label: currentAR.snapshot.data['breadcrumb'],
          routerLink: url
        });
      }
    }

    if (currentAR.firstChild !== null) {
      return this.buildBreadcrumb(currentAR.firstChild, url, breadcrumbs);
    }

    return breadcrumbs;
  }
}
