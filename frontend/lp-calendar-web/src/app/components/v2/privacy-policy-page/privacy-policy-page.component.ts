import { Component } from '@angular/core';
import {RouterLink} from '@angular/router';
import {Card} from 'primeng/card';

@Component({
  selector: 'app-privacy-policy-page-page',
  imports: [
    RouterLink,
    Card
  ],
  templateUrl: './privacy-policy-page.component.html',
  styleUrl: './privacy-policy-page.component.css'
})
export class PrivacyPolicyPageComponent {

}
