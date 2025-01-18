import { Component } from '@angular/core';
import {RouterLink} from '@angular/router';
import {environment} from '../../environments/environment';

@Component({
  selector: 'app-home',
  imports: [
    RouterLink
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {

  protected readonly environment = environment;
}
