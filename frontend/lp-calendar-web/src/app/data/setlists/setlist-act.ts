import {SetlistActDto, SetlistDto} from '../../modules/lpshows-api';

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
