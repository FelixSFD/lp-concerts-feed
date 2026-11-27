import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgOptimizedImage } from '@angular/common';
import { DateTime } from 'luxon';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-footer',
  imports: [RouterLink, NgOptimizedImage],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.css',
})
export class FooterComponent {
  protected readonly currentYear = DateTime.now().year;
  protected readonly environment = environment;

  /** Photos strip (public/photo-strip-2) */
  protected readonly stripPhotos = [
    'Linkin Park Paris JONDBARKER-1196.webp',
    'Linkin Park Paris JONDBARKER-300.webp',
    'Linkin Park Paris JONDBARKER-544.webp',
    'Linkin Park Paris JONDBARKER-894.webp',
    'Linkin Park Paris JONDBARKER-672.webp',
    'Linkin Park Paris JONDBARKER-734.webp',
    'Shinoda Amsterdam JonDBarker00552.webp',
    'Shinoda Amsterdam JonDBarker00054.webp',
    'Linkin Park Paris JONDBARKER-402.webp',
  ];
}
