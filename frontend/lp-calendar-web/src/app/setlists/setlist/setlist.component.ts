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
import {ViewportScroller} from '@angular/common';
import {AppleMusicService} from '../../services/music/apple-music.service';
import Artwork = MusicKit.Artwork;
import {AppleMusicArtworkComponent} from '../../components/music/apple-music-artwork/apple-music-artwork.component';

@Component({
  selector: 'app-setlist',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    SetlistEntryIconsComponent,
    SetlistAlbumChartComponent,
    AppleMusicArtworkComponent
  ],
  templateUrl: './setlist.component.html',
  styleUrl: './setlist.component.css',
})
export class SetlistComponent implements OnInit {
  private readonly tracker = inject(MatomoTracker);
  private readonly scroller = inject(ViewportScroller);
  private readonly appleMusicService = inject(AppleMusicService);

  @Input({ required: false })
  setlistId: number | undefined;

  @Input({ required: false })
  setlist: Setlist | undefined;

  setlistEntriesArtwork$: (Artwork | null)[] = [];

  // map that stores the artwork for an Apple Music Song ID
  songArtworks$ = new Map<string, Artwork>();

  setlistTitle$: string = "Setlist";

  isExpanded$ = false;

  constructor(private setlistService: SetlistsService, private toastr: ToastrService) {
  }

  private async didLoadSetlist() {
    console.debug("Found setlist", this.setlist);
    this.setlistTitle$ = this.setlist?.concertId ?? "Setlist";

    let appleMusicIds = this.setlist?.entries.filter(entry => entry.appleMusicId != null).map(entry => entry.appleMusicId!);
    let foundSongs = appleMusicIds ? await this.appleMusicService.getSongsById(appleMusicIds ?? []) : [];
    this.setlistEntriesArtwork$ = [];
    for (let i = 0; i < (this.setlist?.entries.length ?? 0); i++) {
      let appleMusicId = this.setlist?.entries[i].appleMusicId ?? null;
      if (appleMusicId != null) {
        // entry exists and has an Apple Music ID
        // find the song in foundSongs to get the artwork
        let song = foundSongs.find(song => song.id == appleMusicId);
        console.debug("Found song", song);
        if (song?.attributes?.artwork) {
          this.songArtworks$.set(appleMusicId!, song?.attributes?.artwork!);
        } else {
          console.debug("Song has no artwork");
        }
      } else {
        // entry doesn't have an Apple Music ID
      }
    }
  }

  ngOnInit() {
    if (this.setlist == undefined && this.setlistId !== undefined) {
      this.setlistService.getSetlist(this.setlistId).subscribe({
        next: async data => {
          this.setlist = Setlist.fromDto(data);
          await this.didLoadSetlist();
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not load setlist");
        }
      })
    } else {
      this.didLoadSetlist().then();
    }
  }

  onToggleExpendedClicked() {
    this.isExpanded$ = !this.isExpanded$;
    this.scroller.setOffset([0, 120]);
    this.scroller.scrollToAnchor(`setlist-container-${this.setlist?.id}`, {
      behavior: 'smooth'
    });

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
