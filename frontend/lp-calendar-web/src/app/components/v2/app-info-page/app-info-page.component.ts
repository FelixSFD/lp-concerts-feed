import { Component } from '@angular/core';
import {Card} from 'primeng/card';
import {NgOptimizedImage} from '@angular/common';

@Component({
  selector: 'app-app-info-page',
  imports: [
    Card,
    NgOptimizedImage
  ],
  templateUrl: './app-info-page.component.html',
  styleUrl: './app-info-page.component.css'
})
export class AppInfoPageComponent {

}
