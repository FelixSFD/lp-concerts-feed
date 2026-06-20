import {Component, EventEmitter, Input, Output} from '@angular/core';
import {SongDto} from '../../../../../modules/lpshows-api';
import {FormsModule} from '@angular/forms';
import {Select} from 'primeng/select';
import {NgTemplateOutlet} from '@angular/common';

@Component({
  selector: 'app-select-song',
  imports: [
    FormsModule,
    Select,
    NgTemplateOutlet
  ],
  templateUrl: './select-song.component.html',
  styleUrl: './select-song.component.css',
})
export class SelectSongComponent {
  @Input("available-songs")
  availableSongs$: SongDto[] = [];

  @Output('songSelected')
  songSelectedEmitter = new EventEmitter<SongDto>();

  protected selectedSong:  | null = null;

  onSongSelectionChanged(song: SongDto) {
    this.songSelectedEmitter.emit(song ?? undefined);
  }

  protected readonly JSON = JSON;
}
