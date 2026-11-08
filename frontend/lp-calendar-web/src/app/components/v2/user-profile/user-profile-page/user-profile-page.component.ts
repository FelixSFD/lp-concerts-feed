import { Component, effect, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  ConcertDetailsShortDto,
  ProblemDetailsDto,
  PublicUserDto,
  UserConcertStatsResponseDto,
  UserSongStatsResponseDto
} from '../../../../modules/lpshows-api/v3';
import { Card } from 'primeng/card';
import { UserStatsService } from '../../../../services/user-stats.service';
import { ConcertMapComponent, ConcertMapPin } from '../../concert-map/concert-map.component';

@Component({
  imports: [
    Card,
    ConcertMapComponent,
  ],
  selector: 'app-user-profile-page',
  styleUrl: './user-profile-page.component.css',
  templateUrl: './user-profile-page.component.html',
})
export class UserProfilePageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly userStatsService = inject(UserStatsService);
  protected username = signal<string>("");
  protected resolverError = signal<ProblemDetailsDto | null>(null);

  protected songStats = signal<UserSongStatsResponseDto | null>(null);
  protected concertStats = signal<UserConcertStatsResponseDto | null>(null);

  protected concertPins = signal<ConcertMapPin[]>([]);

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
    let stats = await this.userStatsService.getSongStats(userId);
    console.debug("User song stats:", stats);
    this.songStats.set(stats);
  }

  async reloadConcertStats(userId: string) {
    let stats = await this.userStatsService.getConcertStats(userId);
    console.debug("User concert stats:", stats);
    this.concertStats.set(stats);
    this.concertPins.set(stats.concerts?.map(c => this.makePinForConcert(c)) ?? []);
  }

  private makePinForConcert(concert: ConcertDetailsShortDto): ConcertMapPin {
    return {
      label: `${concert.cityName}, ${concert.countryName}`,
      latitude: concert.venueLatitude ?? 0,
      longitude: concert.venueLongitude ?? 0,
    }
  }
}
