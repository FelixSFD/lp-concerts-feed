import {Component, Input, OnInit} from '@angular/core';
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {SetlistsService} from '../../services/setlists.service';
import {ToastrService} from 'ngx-toastr';
import {ErrorResponseDto} from '../../modules/lpshows-api';
import {Setlist} from '../../data/setlists/setlist';
import {SetlistEntry} from '../../data/setlists/setlist-entry';
import { SetlistActWithEntries } from "../../data/setlists/setlist-act";
import {SetlistEntryIconsComponent} from '../../admin/setlists/setlist-entry-icons/setlist-entry-icons.component';
import {Chart} from 'chart.js/auto';

@Component({
  selector: 'app-setlist',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    SetlistEntryIconsComponent
  ],
  templateUrl: './setlist.component.html',
  styleUrl: './setlist.component.css',
})
export class SetlistComponent implements OnInit {
  @Input({ required: false })
  setlistId: number | undefined;

  @Input({ required: false })
  setlist: Setlist | undefined;

  setlistTitle$: string = "Setlist";

  public chart: any;

  constructor(private setlistService: SetlistsService, private toastr: ToastrService) {
  }

  private didLoadSetlist() {
    console.debug("Found setlist", this.setlist);
    this.setlistTitle$ = this.setlist?.concertId ?? "Setlist";
    this.makePieChart();
  }

  ngOnInit() {
    if (this.setlist == undefined && this.setlistId !== undefined) {
      this.setlistService.getSetlist(this.setlistId).subscribe({
        next: data => {
          this.setlist = Setlist.fromDto(data);
          this.didLoadSetlist();
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not load setlist");
        }
      })
    } else {
      this.makePieChart();
    }
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

    this.chart = new Chart("MyChart", {
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

  isAct(
    item: SetlistEntry | SetlistActWithEntries
  ): item is SetlistActWithEntries {
    return (item as SetlistActWithEntries).entries !== undefined;
  }
}
