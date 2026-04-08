import {Injectable} from '@angular/core';
import {firstValueFrom} from 'rxjs';
import {AppleMusicService as AppleMusicApiClient} from '../../modules/lpshows-api/api/apple-music.service';
import {AppleMusicSong} from '../../data/music/apple/apple-music-song';
import Songs = MusicKit.Songs;

@Injectable({
  providedIn: 'root',
})
export class AppleMusicService {
  private music: MusicKit.MusicKitInstance | null = null;
  private initPromise: Promise<void> | null = null;

  constructor(private apiClient: AppleMusicApiClient) { }

  async init(): Promise<void> {
    if (this.initPromise) {
      return this.initPromise; // already initializing
    }

    this.initPromise = this.initialize();
    return this.initPromise;
  }

  private async initialize(): Promise<void> {
    console.debug("initialize MusicKit");

    const devToken = await this.getDeveloperToken();

    await MusicKit.configure({
      developerToken: devToken,
      app: {
        name: 'LPshows.live',
        build: '1.0.0'
      }
    });

    this.music = MusicKit.getInstance();
    console.debug("Initialized MusicKit successfully");
  }


  private async getDeveloperToken(): Promise<string> {
    console.debug("GetDeveloperToken");

    return await firstValueFrom(
      this.apiClient.getAppleMusicDeveloperToken()
    );
  }

  public async getSongsForIsrc(isrc: string) : Promise<AppleMusicSong[]> {
    console.debug("GetSongsForIsrc", isrc);
    await this.init();
    const storefront = this.music!.api.storefrontId ?? "us";

    const response = await this.music!.api.music(
      `/v1/catalog/${storefront}/songs`,
      {
        filter: {
          isrc: isrc
        }
      }
    ) as any;

    console.debug("GetSongsForIsrc result", response);

    let songs = response.data.data as Songs[];
    return songs.map(AppleMusicSong.fromMusicKit);
  }


  public async getSongsById(ids: string[]) : Promise<Songs[]> {
    console.debug("getSongsById", ids);
    await this.init();
    const storefront = this.music!.api.storefrontId ?? "us";

    const response = await this.music!.api.music(
      `/v1/catalog/${storefront}/songs`,
      {
        ids: ids
      }
    ) as any;

    console.debug("getSongsById result", response);

    return response.data.data as Songs[];
  }
}
