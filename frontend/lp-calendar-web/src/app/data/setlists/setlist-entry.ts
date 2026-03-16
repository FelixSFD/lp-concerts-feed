import {SetlistEntryDto} from '../../modules/lpshows-api';

export class SetlistEntry {
  id!: string;
  songNumber!: number;
  title!: string;
  extraNotes!: string | null;


  static fromDto(dto: SetlistEntryDto): SetlistEntry {
    let title = dto.title;
    let entry: SetlistEntry = {
      id: dto.id!,
      songNumber: dto.songNumber ?? 0,
      title: title ?? dto.playedSong?.title ?? dto.playedSongVariant?.variantName ?? "",
      extraNotes: dto.extraNotes ?? null
    };

    // TODO: Mashups

    return entry;
  }
}
