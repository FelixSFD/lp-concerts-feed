import {Component, inject, Input, OnInit, signal} from '@angular/core';
import {SetlistsService} from '../../../../services/setlists.service';
import {ProblemDetailsDto} from '../../../../modules/lpshows-api/v3';
import {Setlist} from '../../../../data/setlists/setlist';
import {SetlistEntryIconsComponent} from '../setlist-entry-icons/setlist-entry-icons.component';
import {SetlistAlbumChartComponent} from '../setlist-album-chart/setlist-album-chart.component';
import {MatomoTracker} from 'ngx-matomo-client';
import {ViewportScroller} from '@angular/common';
import {AppleMusicService} from '../../../../services/music/apple-music.service';
import Artwork = MusicKit.Artwork;
import {AppleMusicArtworkComponent} from '../../music/apple-music-artwork/apple-music-artwork.component';
import {
  SetlistEntrySongExtraListComponent
} from '../setlist-entry-song-extra-list/setlist-entry-song-extra-list.component';
import {Tooltip} from 'primeng/tooltip';
import {Button} from 'primeng/button';
import {MessageService} from 'primeng/api';
import {SetlistAct} from '../../../../data/setlists/setlist-act';
import {SetlistEntry} from '../../../../data/setlists/setlist-entry';

@Component({
  selector: 'app-setlist',
  imports: [
    SetlistEntryIconsComponent,
    SetlistAlbumChartComponent,
    AppleMusicArtworkComponent,
    SetlistEntrySongExtraListComponent,
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
  private readonly setlistsService = inject(SetlistsService);

  @Input({ required: false })
  setlistId: number | undefined;

  @Input({ required: false })
  setlist: Setlist | undefined;

  // map that stores the artwork for an Apple Music Song ID
  songArtworks$ = signal<Map<string, Artwork>>(new Map<string, Artwork>());

  setlistTitle$ = signal<string>("Setlist");

  isExpanded$ = signal(false);

  private isLoadingThumbnails = false;

  private async didLoadSetlist() {
    console.debug("Found setlist", this.setlist);
    this.setlistTitle$.set(this.setlist?.concertId ?? "Setlist");
    await this.loadThumbnails();
  }

  private async loadThumbnails() {
    if (this.isLoadingThumbnails) {
      return;
    }

    this.isLoadingThumbnails = true;

    try {
      const appleMusicIds = this.setlist?.entries.filter(entry => entry.appleMusicId != null).map(entry => entry.appleMusicId!);
      const foundSongs = appleMusicIds && appleMusicIds.length > 0 ? await this.appleMusicService.getSongsById(appleMusicIds) : [];
      const artworks = new Map<string, Artwork>(this.songArtworks$());
      for (const entry of this.setlist?.entries ?? []) {
        if (entry.appleMusicId == null) {
          continue;
        }
        const song = foundSongs.find(song => song.id == entry.appleMusicId);
        if (song?.attributes?.artwork) {
          artworks.set(entry.appleMusicId, song.attributes.artwork);
        }
      }
      this.songArtworks$.set(artworks);
    } catch (err) {
      console.error("Could not load thumbnails", err);
    } finally {
      this.isLoadingThumbnails = false;
    }
  }

  async ngOnInit() {
    try {
      await this.appleMusicService.init();
      await this.loadThumbnails();
    } catch (err) {
      console.error("Could not initialize Apple Music", err);
    }

    if (this.setlist == undefined && this.setlistId !== undefined) {
      try {
        const data = await this.setlistsService.getCompleteSetlist(this.setlistId);
        this.setlist = Setlist.fromDto(data);
        await this.didLoadSetlist();
      } catch (err: any) {
        let errorResponse: ProblemDetailsDto = err?.error ?? err;
        this.messageService.add({
          severity: "error",
          summary: "Could not load setlist",
          detail: errorResponse?.detail ?? errorResponse?.title ?? errorResponse?.['message'] ?? "Could not load setlist"
        });
      }
    } else {
      await this.didLoadSetlist();
    }
  }

  onToggleExpendedClicked() {
    this.isExpanded$.update(expanded => !expanded);
    this.scroller.setOffset([0, 120]);
    this.scroller.scrollToAnchor(`setlist-container-${this.setlist?.id}`, {
      behavior: 'smooth'
    });

    if (this.isExpanded$()) {
      this.tracker.trackEvent("setlist", "expand_view", this.setlistTitle$());
    }
  }

  getActForEntry(setlist: Setlist | undefined | null, firstEntry: SetlistEntry): SetlistAct | null {
    return setlist?.acts?.find(a => a.actNumber == firstEntry.actNumber) ?? null;
  }

  isActStart(entries: SetlistEntry[], index: number): boolean {
    if (index <= 0) {
      return true;
    }
    return entries[index].actNumber !== entries[index - 1].actNumber;
  }

  protected readonly Number = Number;
}
