import {Component, Input, OnInit} from '@angular/core';
import {Setlist} from '../../data/setlists/setlist';
import {Chart} from 'chart.js/auto';

@Component({
  selector: 'app-setlist-album-chart',
  imports: [],
  templateUrl: './setlist-album-chart.component.html',
  styleUrl: './setlist-album-chart.component.css',
})
export class SetlistAlbumChartComponent implements OnInit {
  @Input({ required: true })
  setlist!: Setlist;

  setlistTitle$: string = "Setlist";

  public chart: any;

  constructor() {
  }

  ngOnInit() {
    this.setlistTitle$ = this.setlist?.concertId ?? "Setlist";
    this.makePieChart();
  }


  private makePieChart() {
    console.debug("Make PieChart");
    let albumNames = this.setlist?.entries.map((entry => entry.albumTitle ?? "Other"));
    let albumStats = new Map<string, number>();
    for (const albumName of albumNames ?? []) {
      let currentSongCount = albumStats.get(albumName);
      if (currentSongCount) {
        currentSongCount = currentSongCount + 1;
      } else {
        currentSongCount = 1;
      }

      albumStats.set(albumName, currentSongCount);
    }

    let labels: string[] = Array.from(albumStats.keys());
    let values: number[] = Array.from(albumStats.values());
    console.debug(values);

    this.chart = new Chart("albumsChart", {
      type: 'doughnut',
      data: {
        labels: labels,
        datasets: [{
          label: 'Songs',
          data: values,
          backgroundColor: [
            'red',
            'pink',
            'green',
            'yellow',
            'orange',
            'blue',
            'purple',
          ],
          hoverOffset: 4
        }],
      },
      options: {
        aspectRatio:2.5
      }
    });
  }
}
