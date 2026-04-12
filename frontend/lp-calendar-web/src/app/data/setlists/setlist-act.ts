import {SetlistActDto} from '../../modules/lpshows-api';
import {SetlistEntry} from './setlist-entry';

export class SetlistAct {
  setlistId!: number;
  actNumber!: number;
  title!: string;


  static fromDto(dto: SetlistActDto): SetlistAct {
    return {
      setlistId: dto.setlistId!,
      actNumber: dto.actNumber!,
      title: dto.title ?? `Act ${dto.actNumber}`,
    };
  }
}

export class SetlistActWithEntries extends SetlistAct {
  entries: SetlistEntry[] = [];

  static withEntries(act: SetlistAct, entries: SetlistEntry[]): SetlistActWithEntries {
    return {
      setlistId: act.setlistId!,
      actNumber: act.actNumber!,
      title: act.title ?? `Act ${act.actNumber}`,
      entries: entries
    };
  }
}
