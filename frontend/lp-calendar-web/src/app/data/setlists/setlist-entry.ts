import {SetlistEntryDto} from '../../modules/lpshows-api';

export class SetlistEntry {
  id!: string;
  songNumber!: number;
  title!: string;
  extraNotes!: string | null;


  static fromDto(dto: SetlistEntryDto): SetlistEntry {
    let entry: SetlistEntry = {
      id: dto.id!,
      songNumber: dto.songNumber ?? 0,
      title: "", // will be overwritten later in this function
      extraNotes: dto.extraNotes ?? null
    };

    if (dto.playedSong) {
      entry.title = dto.playedSong.title ?? "";
    }

    if (dto.playedSongVariant) {
      entry.title = dto.playedSongVariant.variantName ?? "";
    }

    // TODO: Mashups

    return entry;
  }
}
