import {Component, Input, OnInit} from '@angular/core';
import {Setlist} from '../../../../data/setlists/setlist';
import {UIChart} from 'primeng/chart';

@Component({
  selector: 'app-setlist-album-chart',
  imports: [
    UIChart
  ],
  templateUrl: './setlist-album-chart.component.html',
  styleUrl: './setlist-album-chart.component.css',
})
export class SetlistAlbumChartComponent implements OnInit {
  @Input({ required: true })
  setlist!: Setlist;

  setlistTitle$: string = "Setlist";

  data: any;
  options: any;

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

    const documentStyle = getComputedStyle(document.documentElement);
    const textColor = documentStyle.getPropertyValue('--text-color');
    const baseColors = ["green", "red", "blue", "violet", "slate", "yellow", "emerald", "fuchsia", "stone", "indigo"];
    const backgroundColorNum = 300;
    const hoverColorNum = 500;

    this.data = {
      labels: labels,
      datasets: [{
        label: 'Songs',
        data: values,
        backgroundColor: baseColors.map(color => documentStyle.getPropertyValue(`--p-${color}-${backgroundColorNum}`)),
        hoverBackgroundColor: baseColors.map(color => documentStyle.getPropertyValue(`--p-${color}-${hoverColorNum}`)),
        hoverOffset: 8
      }],
    };

    this.options = this.options = {
      plugins: {
        legend: {
          labels: {
            usePointStyle: true,
            color: textColor
          }
        }
      },
      aspectRatio: 2.5
    };
  }
}
