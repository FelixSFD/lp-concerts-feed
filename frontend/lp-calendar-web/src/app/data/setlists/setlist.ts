import {SetlistEntry} from './setlist-entry';
import {SetlistDto} from '../../modules/lpshows-api';
import {SetlistAct, SetlistActWithEntries} from './setlist-act';

export class Setlist {
  id!: number;
  concertId: string | undefined;
  title!: string;
  entries: SetlistEntry[] = [];
  acts: SetlistAct[] = [];

  /**
   * This array contains the entries structured by acts
   */
  structuredEntries: (SetlistEntry | SetlistActWithEntries)[] = [];


  static fromDto(dto: SetlistDto): Setlist {
    let setlist: Setlist = {
      id: dto.id!,
      title: "Setlist",
      concertId: dto.concertId,
      entries: dto.entries?.map(entry => SetlistEntry.fromDto(entry)) ?? [],
      acts: dto.acts?.map(entry => SetlistAct.fromDto(entry)) ?? [],
      structuredEntries: []
    };

    let currentAct: SetlistActWithEntries | null = null;
    for (let entry of dto.entries ?? []) {
      if (entry.actNumber ?? 0 > 0) {
        // this entry is part of an act. If currentAct is not set, it needs to be created
        if (currentAct == null) {
          // create new act
          let foundAct = setlist.acts.find(a => a.actNumber == entry.actNumber);
          if (foundAct) {
            currentAct = SetlistActWithEntries.withEntries(foundAct, []);

            // also make sure to add the act to the setlist entries
            setlist.structuredEntries.push(currentAct);
          }
        }

        // now that the act should be set, we can add the entry to the current act
        if (currentAct != null) {
          currentAct.entries.push(SetlistEntry.fromDto(entry));
        }
      } else {
        // this song is not part of any act. Just add it to the setlist
        setlist.structuredEntries.push(SetlistEntry.fromDto(entry));
      }
    }

    return setlist;
  }
}
