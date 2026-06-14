import {Component, inject, Input} from '@angular/core';
import {FormBuilder, FormControl, ReactiveFormsModule, Validators} from '@angular/forms';
import {SetlistEntrySongExtraDto, SongDto} from '../../../modules/lpshows-api';
import TypeEnum = SetlistEntrySongExtraDto.TypeEnum;
import {SelectButton} from 'primeng/selectbutton';
import {Select} from 'primeng/select';
import {FloatLabel} from 'primeng/floatlabel';
import {InputText} from 'primeng/inputtext';

@Component({
  selector: 'app-setlist-entry-song-extra-form',
  imports: [
    ReactiveFormsModule,
    SelectButton,
    Select,
    FloatLabel,
    InputText
  ],
  templateUrl: './setlist-entry-song-extra-form.component.html',
  styleUrl: './setlist-entry-song-extra-form.component.css',
})
export class SetlistEntrySongExtraFormComponent {
  private formBuilder = inject(FormBuilder);

  @Input("setlist-id")
  setlistId: number | undefined;

  @Input("available-songs")
  availableSongs$: SongDto[] = [];

  extraForm = this.formBuilder.group({
    extraType: new FormControl(SetlistEntrySongExtraFormContent.extraTypeExtraVerse, [Validators.required]),
    selectedSong: new FormControl<SongDto | null>(null, []),
    description: new FormControl('', [Validators.maxLength(127)]),
  });


  public readValuesFromForm(): SetlistEntrySongExtraFormContent {
    return {
      type: this.extraForm.value.extraType?.valueOf() as TypeEnum,
      songId: this.extraForm.value.selectedSong?.id,
      description: this.extraForm.value.description?.valueOf() ?? null
    };
  }


  protected readonly SetlistEntrySongExtraFormContent = SetlistEntrySongExtraFormContent;
}


export class SetlistEntrySongExtraFormContent {
  static extraTypeExtendedBridge = "ExtendedBridge";
  static extraTypeExtraVerse = "ExtraVerse";

  static possibleTypes: string[] = [SetlistEntrySongExtraFormContent.extraTypeExtraVerse, SetlistEntrySongExtraFormContent.extraTypeExtendedBridge];

  type: SetlistEntrySongExtraDto.TypeEnum | undefined;
  songId: number | undefined | null = null;
  description: string | null = null;
}
