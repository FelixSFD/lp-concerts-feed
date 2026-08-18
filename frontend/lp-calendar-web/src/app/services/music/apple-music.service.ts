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

  /**
   * Creates a playlist in the current user's Apple Music library.
   * MusicKit handles obtaining and storing the user's music token after
   * authorization; the catalog song IDs can then be used directly as tracks.
   */
  public async createPlaylist(name: string, songIds: string[], description?: string): Promise<void> {
    await this.init();

    if (!this.music!.isAuthorized) {
      await this.music!.authorize();
    }

    const response = await fetch('https://api.music.apple.com/v1/me/library/playlists', {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${this.music!.developerToken}`,
        'Music-User-Token': this.music!.musicUserToken,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        attributes: {
          name,
          ...(description ? {description} : {})
        },
        relationships: {
          tracks: {
            data: songIds.map(id => ({
              id,
              type: 'songs'
            }))
          }
        }
      })
    });

    if (!response.ok) {
      let detail = `Apple Music returned HTTP ${response.status}.`;
      try {
        const errorResponse = await response.json() as { errors?: Array<{ detail?: string }> };
        detail = errorResponse.errors?.[0]?.detail ?? detail;
      } catch {
        // Keep the HTTP status when Apple Music does not return JSON.
      }
      throw new Error(detail);
    }
  }
}
