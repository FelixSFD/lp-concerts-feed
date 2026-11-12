import { Component, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  ConcertDetailsShortDto,
  ProblemDetailsDto,
  PublicUserDto,
  UserConcertStatsResponseDto,
  UserSongStatsResponseDto, UserSongStatsResponseSongsInnerDto
} from '../../../../modules/lpshows-api/v3';
import Artwork = MusicKit.Artwork;
import { Card } from 'primeng/card';
import { UserStatsService } from '../../../../services/user-stats.service';
import { ConcertMapComponent, ConcertMapPin } from '../../concert-map/concert-map.component';
import { defaultShowTypeId } from '../../../../app.config';
import { Message } from 'primeng/message';
import { UsersService } from '../../../../services/users.service';
import { ButtonDirective, ButtonIcon, ButtonLabel } from 'primeng/button';
import { DataView } from 'primeng/dataview';
import { AppleMusicArtworkComponent } from '../../music/apple-music-artwork/apple-music-artwork.component';
import { AppleMusicService } from '../../../../services/music/apple-music.service';
import { NgTemplateOutlet } from '@angular/common';
import {
  ScrollArea,
  ScrollAreaContent,
  ScrollAreaHandle,
  ScrollAreaScrollbar,
  ScrollAreaViewport
} from 'primeng/scrollarea';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';

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
  ],
  selector: 'app-user-profile-page',
  styleUrl: './user-profile-page.component.css',
  templateUrl: './user-profile-page.component.html',
})
export class UserProfilePageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly usersService = inject(UsersService);
  private readonly userStatsService = inject(UserStatsService);
  private readonly appleMusicService = inject(AppleMusicService);
  protected username = signal<string>("");
  protected isOwnProfile = signal<boolean>(false);
  protected resolverError = signal<ProblemDetailsDto | null>(null);

  protected songStats = signal<UserSongStatsResponseDto | null>(null);
  protected concertStats = signal<UserConcertStatsResponseDto | null>(null);

  protected concertPins = signal<ConcertMapPin[]>([]);

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

  /*private songStatsUpdatedEffect = effect(() => {
    this.loadThumbnails(this.songStats()?.songs ?? []).then();
    console.debug("Song stats updated");
  });*/

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
  }

  async reloadSongStats(userId: string) {
    try {
      let stats = await this.userStatsService.getSongStats(userId);
      console.debug("User song stats:", stats);
      this.songStats.set(stats);
      await this.loadThumbnails(stats.songs ?? []);
    } catch (e) {
      console.error("Failed to load user song stats:", e);
    }
  }

  async reloadConcertStats(userId: string) {
    try {
      let stats = await this.userStatsService.getConcertStats(userId);
      let sortedConcerts = stats.concerts?.sort((a, b) => b.concertDate.localeCompare(a.concertDate)) ?? [];
      stats.concerts = sortedConcerts;
      console.debug("User concert stats:", stats);
      this.concertStats.set(stats);
      this.concertPins.set(sortedConcerts.map(c => this.makePinForConcert(c)) ?? []);
    } catch (e) {
      console.error("Failed to load user concert stats:", e);
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

  private async loadThumbnails(songs: UserSongStatsResponseSongsInnerDto[]) {
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
