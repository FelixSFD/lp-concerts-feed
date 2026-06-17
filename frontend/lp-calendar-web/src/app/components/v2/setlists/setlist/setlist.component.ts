import {Component, inject, Input, OnInit} from '@angular/core';
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {SetlistsService} from '../../../../services/setlists.service';
import {ErrorResponseDto} from '../../../../modules/lpshows-api';
import {Setlist} from '../../../../data/setlists/setlist';
import {SetlistEntryIconsComponent} from '../setlist-entry-icons/setlist-entry-icons.component';
import {SetlistAlbumChartComponent} from '../setlist-album-chart/setlist-album-chart.component';
import {MatomoTracker} from 'ngx-matomo-client';
import {ViewportScroller} from '@angular/common';
import {AppleMusicService} from '../../../../services/music/apple-music.service';
import Artwork = MusicKit.Artwork;
import {AppleMusicArtworkComponent} from '../../../music/apple-music-artwork/apple-music-artwork.component';
import {
  SetlistEntrySongExtraListComponent
} from '../setlist-entry-song-extra-list/setlist-entry-song-extra-list.component';
import {TableModule} from 'primeng/table';
import {Tooltip} from 'primeng/tooltip';
import {Button} from 'primeng/button';
import {MessageService} from 'primeng/api';
import {SetlistAct} from '../../../../data/setlists/setlist-act';
import {SetlistEntry} from '../../../../data/setlists/setlist-entry';

@Component({
  selector: 'app-setlist',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    SetlistEntryIconsComponent,
    SetlistAlbumChartComponent,
    AppleMusicArtworkComponent,
    SetlistEntrySongExtraListComponent,
    TableModule,
    Tooltip,
    Button
  ],
  templateUrl: './setlist.component.html',
  styleUrl: './setlist.component.css',
})
export class SetlistComponent implements OnInit {
  private readonly tracker = inject(MatomoTracker);
  private readonly scroller = inject(ViewportScroller);
  private readonly appleMusicService = inject(AppleMusicService);
  private readonly messageService = inject(MessageService);

  @Input({ required: false })
  setlistId: number | undefined;

  @Input({ required: false })
  setlist: Setlist | undefined;

  setlistEntriesArtwork$: (Artwork | null)[] = [];

  // map that stores the artwork for an Apple Music Song ID
  songArtworks$ = new Map<string, Artwork>();

  setlistTitle$: string = "Setlist";

  isExpanded$ = false;

  private isLoadingThumbnails = false;

  constructor(private setlistService: SetlistsService) {
    // init apple music
    this.appleMusicService.init().then(async () => {
      await this.loadThumbnails();
    });
  }

  private async didLoadSetlist() {
    console.debug("Found setlist", this.setlist);
    this.setlistTitle$ = this.setlist?.concertId ?? "Setlist";
    await this.loadThumbnails();
  }


  private async loadThumbnails() {
    if (this.isLoadingThumbnails) {
      return;
    }

    this.isLoadingThumbnails = true;

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

    this.isLoadingThumbnails = false;
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
          this.messageService.add({severity: "error", summary: "Could not load setlist", detail: errorResponse.message});
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

  getActForEntry(setlist: Setlist | undefined | null, firstEntry: SetlistEntry): SetlistAct | null {
    return setlist?.acts?.find(a => a.actNumber == firstEntry.actNumber) ?? null;
  }

  protected readonly Number = Number;
}
