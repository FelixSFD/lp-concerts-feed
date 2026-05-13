import {Component, inject, Input} from '@angular/core';
import {FormBuilder, FormControl, ReactiveFormsModule, Validators} from '@angular/forms';
import {SongsService} from '../../../services/songs.service';
import {SetlistsService} from '../../../services/setlists.service';
import {NgClass} from '@angular/common';
import {SetlistEntrySongExtraDto, SongDto} from '../../../modules/lpshows-api';
import TypeEnum = SetlistEntrySongExtraDto.TypeEnum;

@Component({
  selector: 'app-setlist-entry-song-extra-form',
  imports: [
    ReactiveFormsModule,
    NgClass
  ],
  templateUrl: './setlist-entry-song-extra-form.component.html',
  styleUrl: './setlist-entry-song-extra-form.component.css',
})
export class SetlistEntrySongExtraFormComponent {
  private formBuilder = inject(FormBuilder);
  private songService = inject(SongsService);
  private setlistsService = inject(SetlistsService);

  @Input("setlist-id")
  setlistId: number | undefined;

  @Input("available-songs")
  availableSongs$: SongDto[] = [];

  extraForm = this.formBuilder.group({
    extraType: new FormControl(SetlistEntrySongExtraFormContent.extraTypeExtraVerse, [Validators.required]),
    selectedSongId: new FormControl(-1, []),
    description: new FormControl('', [Validators.maxLength(127)]),
  });


  public readValuesFromForm(): SetlistEntrySongExtraFormContent {
    return {
      type: this.extraForm.value.extraType?.valueOf() as TypeEnum,
      songId: this.extraForm.value.selectedSongId?.valueOf(),
      description: this.extraForm.value.description?.valueOf() ?? null
    };
  }


  protected readonly SetlistEntrySongExtraFormContent = SetlistEntrySongExtraFormContent;
}


export class SetlistEntrySongExtraFormContent {
  static extraTypeExtendedBridge = "extendedBridge";
  static extraTypeExtraVerse = "extraVerse";

  type: SetlistEntrySongExtraDto.TypeEnum | undefined;
  songId: number | undefined | null = null;
  description: string | null = null;
}
