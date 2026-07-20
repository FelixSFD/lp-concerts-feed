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

  data: any;
  options: any;
  totalSongs = 0;
  legendItems: { label: string; value: number; color: string }[] = [];

  ngOnInit() {
    this.makePieChart();
  }


  private makePieChart() {
    const albumStats = new Map<string, number>();
    for (const entry of this.setlist?.entries ?? []) {
      const album = entry.albumTitle ?? "Other";
      albumStats.set(album, (albumStats.get(album) ?? 0) + 1);
    }

    const labels: string[] = Array.from(albumStats.keys());
    const values: number[] = Array.from(albumStats.values());
    this.totalSongs = values.reduce((sum, v) => sum + v, 0);

    const documentStyle = getComputedStyle(document.documentElement);
    const palette = ["purple", "teal", "amber", "pink", "sky", "lime", "orange", "cyan", "rose", "indigo"];
    const backgroundColor: string[] = [];
    const hoverBackgroundColor: string[] = [];
    let hueIndex = 0;
    for (const label of labels) {
      if (label === "Other") {
        backgroundColor.push(documentStyle.getPropertyValue('--p-surface-400').trim());
        hoverBackgroundColor.push(documentStyle.getPropertyValue('--p-surface-500').trim());
      } else {
        const hue = palette[hueIndex % palette.length];
        hueIndex++;
        backgroundColor.push(documentStyle.getPropertyValue(`--p-${hue}-400`).trim());
        hoverBackgroundColor.push(documentStyle.getPropertyValue(`--p-${hue}-500`).trim());
      }
    }

    this.data = {
      labels: labels,
      datasets: [{
        label: 'Songs',
        data: values,
        backgroundColor: backgroundColor,
        hoverBackgroundColor: hoverBackgroundColor,
        borderWidth: 0,
        hoverOffset: 6
      }],
    };

    this.legendItems = labels.map((label, i) => ({ label: label, value: values[i], color: backgroundColor[i] }));

    this.options = {
      responsive: true,
      maintainAspectRatio: true,
      aspectRatio: 1,
      cutout: '66%',
      plugins: {
        legend: { display: false },
        tooltip: {
          enabled: true,
          position: 'nearest',
          padding: 10,
          backgroundColor: 'rgba(24, 24, 27, 0.92)',
          titleColor: '#ffffff',
          bodyColor: '#ffffff',
          callbacks: {
            label: (ctx: any) => `${ctx.parsed} song${ctx.parsed === 1 ? '' : 's'}`
          }
        }
      }
    };
  }
}
