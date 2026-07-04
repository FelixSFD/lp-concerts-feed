import {Component, Input, OnInit} from '@angular/core';
import Artwork = MusicKit.Artwork;
import {NgOptimizedImage} from '@angular/common';

@Component({
  selector: 'app-apple-music-artwork',
  imports: [
    NgOptimizedImage
  ],
  templateUrl: './apple-music-artwork.component.html',
  styleUrl: './apple-music-artwork.component.css',
})
export class AppleMusicArtworkComponent implements OnInit {
  // URL template
  @Input("artwork")
  artwork!: Artwork;

  @Input("width")
  width: number = 50;

  // exact URL to the file
  url: string | null = null;


  ngOnInit() {
    // noinspection JSSuspiciousNameCombination
    let height = this.width;
    this.url = MusicKit.formatArtworkURL(this.artwork, height, this.width);
  }
}
