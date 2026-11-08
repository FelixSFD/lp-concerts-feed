import { inject, Service } from '@angular/core';
import { UserConcertStatsResponseDto, UsersApi, UserSongStatsResponseDto } from '../modules/lpshows-api/v3';
import { environment } from '../../environments/environment';
import { addAuthentication } from '../auth/auth.config';
import { firstValueFrom } from 'rxjs';

@Service()
export class UserStatsService {
  private usersApi = inject(UsersApi);

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
}
