import { inject, Service } from '@angular/core';
import { UserConcertStatsResponseDto, UsersApi, UserSongStatsResponseDto } from '../modules/lpshows-api/v3';
import { environment } from '../../environments/environment';
import { addAuthentication } from '../auth/auth.config';
import { firstValueFrom } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Service()
export class UserStatsService {
  private usersApi = inject(UsersApi);
  private http = inject(HttpClient);

  constructor() {
    this.usersApi.configuration.basePath = environment.apiBaseUrl;
    addAuthentication(this.usersApi);
  }

  /**
   * Returns stats about concerts a user has attended
   * @param userId ID of the user
   */
  async getConcertStats(userId: string): Promise<UserConcertStatsResponseDto> {
    return firstValueFrom(
      this.usersApi.getUserConcertStats(userId)
    );
  }

  /**
   * Returns stats about songs a user has seen live
   * @param userId ID of the user
   */
  async getSongStats(userId: string): Promise<UserSongStatsResponseDto> {
    return firstValueFrom(
      this.usersApi.getUserSongStats(userId)
    );
  }

  /**
   * Returns a shareable image of a user's concert stats
   * @param userId ID of the user
   */
  async getUserConcertStatsSharePic(userId: string): Promise<File> {
    let response = await firstValueFrom(
      this.usersApi.getUserConcertStatsSharePic(userId)
    );

    let url = response.url;
    console.debug("Created share-pic at URL: ", url);
    let blob = await firstValueFrom(this.http.get(url, { responseType: "blob" }));

    return new File([blob], "stats.png", {
      type: blob.type,
    });
  }
}
