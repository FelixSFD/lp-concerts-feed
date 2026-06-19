import {ConcertStatusValueDto} from '../modules/lpshows-api';

export class ConcertStatus {
  value!: ConcertStatusValueDto;
  label!: string;

  static allValues: ConcertStatus[] = [
    {
      value: ConcertStatusValueDto.Planned,
      label: 'Planned',
    },
    {
      value: ConcertStatusValueDto.Running,
      label: 'Live right now',
    },
    {
      value: ConcertStatusValueDto.Past,
      label: 'Past',
    },
    {
      value: ConcertStatusValueDto.Cancelled,
      label: 'Cancelled',
    }
  ];
}
