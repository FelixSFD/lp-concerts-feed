import { Injectable } from '@angular/core';
import {firstValueFrom, Observable} from 'rxjs';
import {AppleMusicService as AppleMusicApiClient} from '../../modules/lpshows-api/api/apple-music.service';

@Injectable({
  providedIn: 'root',
})
export class AppleMusicService {
  private music: any;

  constructor(private apiClient: AppleMusicApiClient) { }

  async init() {
    let devToken = await this.getDeveloperToken();
    console.debug("Found developer token", devToken);
    await MusicKit.configure({
      developerToken: devToken,
      app: {
        name: 'LPshows.live',
        build: '1.0.0'
      }
    });

    this.music = MusicKit.getInstance();
  }


  private async getDeveloperToken(): Promise<string> {
    console.debug("GetDeveloperToken");

    return await firstValueFrom(
      this.apiClient.getAppleMusicDeveloperToken()
    );
  }
}
