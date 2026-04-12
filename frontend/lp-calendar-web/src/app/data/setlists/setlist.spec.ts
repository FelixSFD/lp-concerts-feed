import { Setlist } from './setlist';
import {SetlistDto} from '../../modules/lpshows-api';
import {Guid} from 'guid-typescript';

describe('Setlist', () => {
  it('should create an instance', () => {
    expect(new Setlist()).toBeTruthy();
  });


  it('should create a setlist with entries and acts', () => {
    let setlistId = 1337;
    let setlistDto: SetlistDto = {
      id: setlistId,
      concertId: "concert1234",
      linkinpediaUrl: "https://lplive.net",
      acts: [
        {
          actNumber: 1,
          title: "Inception Intro A",
          setlistId: setlistId
        },
        {
          actNumber: 2,
          title: "Creation Intro A",
          setlistId: setlistId
        },
        {
          actNumber: 3,
          title: "Break/Collapse Transition",
          setlistId: setlistId
        },
        {
          actNumber: 5,
          title: "Resolution Intro A",
          setlistId: setlistId
        }
      ],
      entries: [
        {
          id: Guid.create().toString(),
          title: "Waiting Room",
          songNumber: 0,
          sortNumber: 0,
          extraNotes: "Pre-show; Fugazi song",
          isPlayedFromRecording: true
        },
        {
          id: Guid.create().toString(),
          title: "Somewhere I Belong",
          actNumber: 1,
          songNumber: 1,
          sortNumber: 1,
          playedSong: {
            id: 1,
            title: "Somewhere I Belong",
            isrc: "1234"
          },
          extraNotes: "Short intro",
        },
        {
          id: Guid.create().toString(),
          title: "Crawling",
          actNumber: 1,
          songNumber: 2,
          sortNumber: 2,
          playedSong: {
            id: 2,
            title: "Crawling",
            isrc: "45634"
          },
        },
        {
          id: Guid.create().toString(),
          title: "The Catalyst",
          actNumber: 2,
          songNumber: 7,
          sortNumber: 7,
          playedSong: {
            id: 3,
            title: "The Catalyst",
            isrc: "47567345"
          },
          extraNotes: "shortened",
        },
        {
          id: Guid.create().toString(),
          title: "Waiting For The End",
          actNumber: 2,
          songNumber: 9,
          sortNumber: 9,
          playedSong: {
            id: 4,
            title: "Waiting For The End",
            isrc: "7876321"
          },
          extraNotes: "2024 Intro",
        },
        {
          id: Guid.create().toString(),
          title: "Lost",
          actNumber: 2,
          songNumber: 16,
          sortNumber: 16,
          playedSongVariant: {
            id: 1,
            variantName: "Piano Version",
            songId: 5,
            description: "Shortened. Only first verse and chorus"
          }
        },
        {
          id: Guid.create().toString(),
          title: "Heavy Is The Crown",
          actNumber: 5,
          songNumber: 16,
          sortNumber: 16,
          playedSong: {
            id: 99,
            title: "Heavy Is The Crown",
            isrc: "563242"
          },
          isWorldPremiere: true
        }
      ]
    }

    let setlist = Setlist.fromDto(setlistDto);
    expect(setlist).toBeDefined();
  });
});
