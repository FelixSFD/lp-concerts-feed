import { ChangeDetectionStrategy, Component, inject, Input, OnInit } from '@angular/core';
import {SetlistsService} from '../../../../services/setlists.service';
import {ErrorResponseDto} from '../../../../modules/lpshows-api';
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
import { ConfirmationService, MessageService } from 'primeng/api';
import {SetlistAct} from '../../../../data/setlists/setlist-act';
import {SetlistEntry} from '../../../../data/setlists/setlist-entry';
import { ConfirmDialog } from 'primeng/confirmdialog';

@Component({
  selector: 'app-setlist',
  imports: [
    SetlistEntryIconsComponent,
    SetlistAlbumChartComponent,
    AppleMusicArtworkComponent,
    SetlistEntrySongExtraListComponent,
    Tooltip,
    Button,
    ConfirmDialog
  ],
  templateUrl: './setlist.component.html',
  styleUrl: './setlist.component.css',
  changeDetection: ChangeDetectionStrategy.Eager
})
export class SetlistComponent implements OnInit {
  private readonly tracker = inject(MatomoTracker);
  private readonly scroller = inject(ViewportScroller);
  private readonly appleMusicService = inject(AppleMusicService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);

  @Input({ required: false })
  setlistId: number | undefined;

  @Input({ required: false })
  setlist: Setlist | undefined;

  // map that stores the artwork for an Apple Music Song ID
  songArtworks$ = new Map<string, Artwork>();

  setlistTitle$: string = "Setlist";

  isExpanded$ = false;
  isCreatingPlaylist$ = false;

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

    const appleMusicIds = this.setlist?.entries.filter(entry => entry.appleMusicId != null).map(entry => entry.appleMusicId!);
    const foundSongs = appleMusicIds ? await this.appleMusicService.getSongsById(appleMusicIds ?? []) : [];
    for (const entry of this.setlist?.entries ?? []) {
      if (entry.appleMusicId == null) {
        continue;
      }
      const song = foundSongs.find(song => song.id == entry.appleMusicId);
      if (song?.attributes?.artwork) {
        this.songArtworks$.set(entry.appleMusicId, song.attributes.artwork);
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

  async onCreateAppleMusicPlaylistClicked() {
    const songIds = this.setlist?.entries
      .map(entry => entry.appleMusicId)
      .filter((id): id is string => id != null && id.length > 0) ?? [];

    if (songIds.length === 0 || this.isCreatingPlaylist$) {
      return;
    }

    this.isCreatingPlaylist$ = true;
    const playlistName = `LINKIN PARK - ${this.setlist?.concertTitle ?? this.setlist?.setName}`;

    this.confirmationService.confirm({
      message: `Do you want to create a playlist named "${playlistName}" in your Apple Music library?`,
      header: 'Create a new playlist in your library?',
      icon: 'pi pi-info-circle',
      rejectLabel: 'Cancel',
      rejectButtonProps: {
        label: 'Cancel',
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: 'Add playlist',
        icon: 'pi pi-apple',
        severity: 'primary'
      },

      accept: () => {
        this.createPlaylist(playlistName, songIds);
        this.isCreatingPlaylist$ = false;
      },
      reject: () => {
        this.isCreatingPlaylist$ = false;
      }
    });
  }

  private async createPlaylist(playlistName: string, songIds: string[]) {
    try {
      await this.appleMusicService.createPlaylist(
        playlistName,
        songIds,
        `Setlist from ${this.setlist?.concertTitle ?? 'concert'}`
      );
      this.messageService.add({
        severity: 'success',
        summary: 'Playlist created',
        detail: `“${playlistName}” was added to your Apple Music library.`
      });
    } catch (error) {
      console.error('Could not create Apple Music playlist', error);
      this.messageService.add({
        severity: 'error',
        summary: 'Could not create playlist',
        detail: error instanceof Error ? error.message : 'Please try again.'
      });
    }
  }

  hasAppleMusicSongs(): boolean {
    return this.setlist?.entries.some(entry => entry.appleMusicId != null && entry.appleMusicId.length > 0) ?? false;
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
