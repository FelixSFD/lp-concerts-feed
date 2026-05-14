import {SetlistEntryDto, SetlistEntrySongExtraDto} from '../../modules/lpshows-api';

export class SetlistEntry {
  id!: string;
  songNumber!: number;
  actNumber!: number | null;
  title!: string;
  albumTitle: string | null = null;
  extraNotes!: string | null;
  appleMusicId!: string | null;
  linkinpediaUrl!: string | null;
  isPlayedFromRecording: boolean = false;
  isWorldPremiere: boolean = false;
  isRotationSong: boolean = false;
  isLivePremiere: boolean = false;
  songExtras: SetlistEntrySongExtraDto[] = [];


  static fromDto(dto: SetlistEntryDto): SetlistEntry {
    let title = dto.title;
    let entry: SetlistEntry = {
      id: dto.id!,
      songNumber: dto.songNumber ?? 0,
      actNumber: dto.actNumber ?? null,
      title: title ?? dto.playedSong?.title ?? dto.playedSongVariant?.variantName ?? "",
      albumTitle: dto.playedSong?.album?.title ?? null,
      extraNotes: dto.extraNotes ?? null,
      appleMusicId: dto.appleMusicId ?? null,
      linkinpediaUrl: dto.linkinpediaUrl ?? null,
      isPlayedFromRecording: dto.isPlayedFromRecording ?? false,
      isWorldPremiere: dto.isWorldPremiere ?? false,
      isRotationSong: dto.isRotationSong ?? false,
      isLivePremiere: dto.isLivePremiere ?? false,
      songExtras: dto.songExtras ?? [],
    };

    // TODO: Mashups

    return entry;
  }
}
