import { Component } from '@angular/core';
import {environment} from '../../../../environments/environment';
import {Card} from 'primeng/card';

@Component({
  selector: 'app-imprint-page-page',
  imports: [
    Card
  ],
  templateUrl: './imprint-page.component.html',
  styleUrl: './imprint-page.component.css'
})
export class ImprintPageComponent {

  protected readonly environment = environment;
}
