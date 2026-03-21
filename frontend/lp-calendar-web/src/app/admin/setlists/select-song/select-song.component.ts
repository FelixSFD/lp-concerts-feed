import {Component, EventEmitter, Input, Output} from '@angular/core';
import {SongDto} from '../../../modules/lpshows-api';
import {FormsModule} from '@angular/forms';

@Component({
  selector: 'app-select-song',
  imports: [
    FormsModule
  ],
  templateUrl: './select-song.component.html',
  styleUrl: './select-song.component.css',
})
export class SelectSongComponent {
  @Input("available-songs")
  availableSongs$: SongDto[] = [];

  @Output('songSelected')
  songSelected = new EventEmitter<SongDto>();

  onSongSelectionChanged(id: string) {
    let selectedSong = this.availableSongs$.find(s => s.id === Number(id));
    this.songSelected.emit(selectedSong ?? undefined);
  }
}
