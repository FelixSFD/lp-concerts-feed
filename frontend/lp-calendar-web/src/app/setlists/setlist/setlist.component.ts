import {Component, inject, Input, OnInit} from '@angular/core';
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {SetlistsService} from '../../services/setlists.service';
import {ToastrService} from 'ngx-toastr';
import {ErrorResponseDto} from '../../modules/lpshows-api';
import {Setlist} from '../../data/setlists/setlist';
import {SetlistEntry} from '../../data/setlists/setlist-entry';
import { SetlistActWithEntries } from "../../data/setlists/setlist-act";
import {SetlistEntryIconsComponent} from '../../admin/setlists/setlist-entry-icons/setlist-entry-icons.component';
import {SetlistAlbumChartComponent} from '../setlist-album-chart/setlist-album-chart.component';
import {MatomoTracker} from 'ngx-matomo-client';

@Component({
  selector: 'app-setlist',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    SetlistEntryIconsComponent,
    SetlistAlbumChartComponent
  ],
  templateUrl: './setlist.component.html',
  styleUrl: './setlist.component.css',
})
export class SetlistComponent implements OnInit {
  private readonly tracker = inject(MatomoTracker);

  @Input({ required: false })
  setlistId: number | undefined;

  @Input({ required: false })
  setlist: Setlist | undefined;

  setlistTitle$: string = "Setlist";

  isExpanded$ = false;

  constructor(private setlistService: SetlistsService, private toastr: ToastrService) {
  }

  private didLoadSetlist() {
    console.debug("Found setlist", this.setlist);
    this.setlistTitle$ = this.setlist?.concertId ?? "Setlist";
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
    }
  }

  onToggleExpendedClicked() {
    this.isExpanded$ = !this.isExpanded$;

    if (this.isExpanded$) {
      this.tracker.trackEvent("setlist", "expand_view", this.setlistTitle$);
    }
  }

  isAct(
    item: SetlistEntry | SetlistActWithEntries
  ): item is SetlistActWithEntries {
    return (item as SetlistActWithEntries).entries !== undefined;
  }
}
