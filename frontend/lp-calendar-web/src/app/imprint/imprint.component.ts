import { Component } from '@angular/core';
import {environment} from '../../environments/environment';

@Component({
  selector: 'app-imprint',
  imports: [],
  templateUrl: './imprint.component.html',
  styleUrl: './imprint.component.css'
})
export class ImprintComponent {

  protected readonly environment = environment;
}
