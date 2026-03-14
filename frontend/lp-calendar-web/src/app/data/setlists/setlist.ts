import {SetlistEntry} from './setlist-entry';
import {SetlistDto} from '../../modules/lpshows-api';
import {SetlistAct} from './setlist-act';

export class Setlist {
  id!: number;
  concertId: string | undefined;
  title!: string;
  entries: SetlistEntry[] = [];
  acts: SetlistAct[] = [];


  static fromDto(dto: SetlistDto): Setlist {
    return {
      id: dto.id!,
      title: "Setlist",
      concertId: dto.concertId,
      entries: dto.entries?.map(entry => SetlistEntry.fromDto(entry)) ?? [],
      acts: dto.acts?.map(entry => SetlistAct.fromDto(entry)) ?? []
    };
  }
}
