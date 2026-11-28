import { Component, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  ConcertDetailsShortDto,
  ProblemDetailsDto,
  PublicUserDto, SongStatsWithRarityDto,
  UserConcertStatsResponseDto,
  UserSongStatsResponseDto
} from '../../../../modules/lpshows-api/v3';
import Artwork = MusicKit.Artwork;
import { Card } from 'primeng/card';
import { UserStatsService } from '../../../../services/user-stats.service';
import { ConcertMapComponent, ConcertMapPin } from '../../concert-map/concert-map.component';
import { defaultShowTypeId } from '../../../../app.config';
import { Message } from 'primeng/message';
import { UsersService } from '../../../../services/users.service';
import { Button, ButtonDirective, ButtonIcon, ButtonLabel } from 'primeng/button';
import { DataView } from 'primeng/dataview';
import { AppleMusicArtworkComponent } from '../../music/apple-music-artwork/apple-music-artwork.component';
import { AppleMusicService } from '../../../../services/music/apple-music.service';
import { NgOptimizedImage, NgTemplateOutlet } from '@angular/common';
import {
  ScrollArea,
  ScrollAreaContent,
  ScrollAreaHandle,
  ScrollAreaScrollbar,
  ScrollAreaViewport
} from 'primeng/scrollarea';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { AuthService } from '../../../../auth/auth.service';
import { MatomoTracker } from 'ngx-matomo-client';
import { Skeleton } from 'primeng/skeleton';
import { Dialog } from 'primeng/dialog';
import { ProgressSpinner } from 'primeng/progressspinner';
import { MessageService } from 'primeng/api';

@Component({
  imports: [
    Card,
    ConcertMapComponent,
    Message,
    ButtonDirective,
    ButtonIcon,
    ButtonLabel,
    RouterLink,
    DataView,
    AppleMusicArtworkComponent,
    NgTemplateOutlet,
    ScrollArea,
    ScrollAreaContent,
    ScrollAreaViewport,
    ScrollAreaScrollbar,
    ScrollAreaHandle,
    Tag,
    Tooltip,
    Skeleton,
    Dialog,
    ProgressSpinner,
  ],
  selector: 'app-user-profile-page',
  styleUrl: './user-profile-page.component.css',
  templateUrl: './user-profile-page.component.html',
})
export class UserProfilePageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly usersService = inject(UsersService);
  private readonly userStatsService = inject(UserStatsService);
  private readonly authService = inject(AuthService);
  private readonly appleMusicService = inject(AppleMusicService);
  private readonly tracker = inject(MatomoTracker);
  private readonly messageService = inject(MessageService);

  protected username = signal<string>("");
  protected isOwnProfile = signal<boolean>(false);
  protected resolverError = signal<ProblemDetailsDto | null>(null);

  protected songStats = signal<UserSongStatsResponseDto | null>(null);
  protected rareSongCount = signal<number>(0);
  protected concertStats = signal<UserConcertStatsResponseDto | null>(null);

  protected concertPins = signal<ConcertMapPin[]>([]);

  protected showShareButton = signal<boolean>(false);
  protected showShareImageDialog = signal<boolean>(false);
  protected shareImageUrl = signal<string | null>(null);

  protected isLoadingConcertStats = signal<boolean>(false);
  protected isLoadingSongStats = signal<boolean>(false);

  protected isSharing = signal<boolean>(false);

  // The file object that will be used for sharing inside the dialog
  private shareImageFile: File | null = null;

  // map that stores the artwork for an Apple Music Song ID
  songArtworks$ = signal<Map<string, Artwork>>(new Map<string, Artwork>());

  private user = signal<PublicUserDto | null>(null);
  private userUpdatedEffect = effect(() => {
    let user = this.user();
    if (user == null) {
      return;
    }

    console.debug("User updated:", user);
    this.reloadSongStats(user.id).then();
    this.reloadConcertStats(user.id).then();
    this.username.set(user.username);
  });
  private isOwnProfileEffect = effect(() => {
    let currentUserId = this.usersService.currentUser()?.id;
    let currentProfilePageId = this.user()?.id;
    this.isOwnProfile.set(currentUserId == currentProfilePageId);
  });

  private songStatsUpdatedEffect = effect(() => {
    console.debug("Song stats updated");
    this.rareSongCount.set((this.songStats()?.songs ?? []).filter(s => s.rarity?.isRare).length);
  });

  private isLoadingThumbnails: boolean = false;

  constructor() {
    this.route.data.subscribe(data => {
      console.debug("Resolved data:", data);

      let user = data['user'] as PublicUserDto | null;
      if (user == null || !user.id) {
        this.resolverError.set(data['user'] as ProblemDetailsDto);
        this.user.set(null);
        return;
      }

      this.user.set(user);
    });

    this.authService.canManageSetlists.subscribe((canManage) => {
      this.showShareButton.set(canManage);
    });
  }

  async onShareStatsClicked() {
    console.debug("Share stats clicked");
    this.isSharing.set(true);
    this.showShareImageDialog.set(true);

    try {
      let response = await this.userStatsService.getUserConcertStatsSharePic(this.user()?.id!);
      console.debug("response:", response);

      const reader = new FileReader();
      reader.onload = (e) => {
        this.shareImageUrl.set(e.target?.result as string);
        this.shareImageFile = response;
      }
      reader.readAsDataURL(new Blob([await response.bytes()]));
    } catch (e) {
      console.error("Failed to share profile stats:", e);
    } finally {
      this.isSharing.set(false);
    }
  }

  async onShareImageClicked() {
    if (this.shareImageFile == null) {
      console.warn("shareImageFile not found");
      this.messageService.add({severity: 'error', summary: 'Failed to share image', detail: "Image to share not found"});
      return;
    }

    let shareData = {
      files: [this.shareImageFile],
    };

    if (navigator.share) {
      await navigator.share(shareData);
      this.tracker.trackEvent('share', 'user_stats', this.user()?.username);
    } else {
      console.warn("navigator.canShare() returned false");
      this.messageService.add({severity: 'error', detail: 'Failed to share image'});
    }
  }

  async onCopyImageClicked() {
    if (this.shareImageFile == null) {
      console.warn("shareImageFile not found");
      this.messageService.add({severity: 'error', summary: 'Failed to copy image', detail: "Image to copy not found"});
      return;
    }

    if (navigator.clipboard) {
      // copy file to clipboard
      try {
        await navigator.clipboard.write([
          new ClipboardItem({
            [this.shareImageFile.type]: this.shareImageFile
          })
        ]);
        console.debug("Copied image to clipboard");
        this.messageService.add({severity: 'success', detail:'Image copied to clipboard'});
      } catch (e) {
        console.error("Failed to copy image to clipboard:", e);
        this.messageService.add({severity: 'error', detail: 'Failed to copy image to clipboard'});
      }
    }
  }

  async reloadSongStats(userId: string) {
    try {
      this.isLoadingSongStats.set(true);
      let stats = await this.userStatsService.getSongStats(userId);
      console.debug("User song stats:", stats);
      this.songStats.set(stats);
      await this.loadThumbnails(stats.songs ?? []);
    } catch (e) {
      console.error("Failed to load user song stats:", e);
    } finally {
      this.isLoadingSongStats.set(false);
    }
  }

  async reloadConcertStats(userId: string) {
    try {
      this.isLoadingConcertStats.set(true);
      let stats = await this.userStatsService.getConcertStats(userId);
      let sortedConcerts = stats.concerts?.sort((a, b) => b.concertDate.localeCompare(a.concertDate)) ?? [];
      stats.concerts = sortedConcerts;
      console.debug("User concert stats:", stats);
      this.concertStats.set(stats);
      this.concertPins.set(sortedConcerts.map(c => this.makePinForConcert(c)) ?? []);
    } catch (e) {
      console.error("Failed to load user concert stats:", e);
    } finally {
      this.isLoadingConcertStats.set(false);
    }
  }

  private makePinForConcert(concert: ConcertDetailsShortDto): ConcertMapPin {
    let markerColor = "#c969e0";
    if (concert.concertType.id != defaultShowTypeId) {
      markerColor = "#eb4b4b";
    }
    return {
      label: `${concert.cityName}, ${concert.countryName}`,
      subtitle: `${concert.venueName}`,
      color: markerColor,
      latitude: concert.venueLatitude ?? 0,
      longitude: concert.venueLongitude ?? 0,
      alwaysShowSubtitle: true,
    }
  }

  private async loadThumbnails(songs: SongStatsWithRarityDto[]) {
    if (this.isLoadingThumbnails) {
      return;
    }

    this.isLoadingThumbnails = true;

    try {
      const appleMusicIds = songs
        .filter(entry => entry.appleMusicId != null)
        .map(entry => entry.appleMusicId!);

      const foundAppleMusicSongs = appleMusicIds && appleMusicIds.length > 0 ? await this.appleMusicService.getSongsById(appleMusicIds) : [];
      const artworks = new Map<string, Artwork>(this.songArtworks$());
      for (const song of songs ?? []) {
        if (song.appleMusicId == null) {
          continue;
        }
        const foundSong = foundAppleMusicSongs.find(ams => ams.id == song.appleMusicId);
        if (foundSong?.attributes?.artwork) {
          artworks.set(song.appleMusicId, foundSong.attributes.artwork);
        }
      }
      this.songArtworks$.set(artworks);
    } catch (err) {
      console.error("Could not load thumbnails", err);
    } finally {
      this.isLoadingThumbnails = false;
    }
  }
}
