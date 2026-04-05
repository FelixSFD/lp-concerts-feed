import Songs = MusicKit.Songs;

export class AppleMusicSong {
  id!: string;
  title!: string;
  albumName: string | undefined | null;
  artistName!: string;


  static fromMusicKit(songs: Songs): AppleMusicSong {
    return {
      id: songs.id,
      title: songs.attributes?.name ?? "Unknown Title",
      albumName: songs.attributes?.albumName,
      artistName: songs.attributes?.artistName ?? "Unknown Artist"
    };
  }
}
