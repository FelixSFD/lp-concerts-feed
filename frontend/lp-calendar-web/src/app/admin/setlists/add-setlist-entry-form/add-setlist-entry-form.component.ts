import {Component, EventEmitter, inject, OnInit, Output} from '@angular/core';
import {FormBuilder, FormControl, ReactiveFormsModule, Validators} from "@angular/forms";
import {SongsService} from '../../../services/songs.service';
import {ConcertDto, ErrorResponseDto, SetlistActDto, SongDto, SongVariantDto} from '../../../modules/lpshows-api';
import {ToastrService} from 'ngx-toastr';
import {NgClass} from '@angular/common';

@Component({
  selector: 'app-add-setlist-entry-form',
  imports: [
    ReactiveFormsModule,
    NgClass
  ],
  templateUrl: './add-setlist-entry-form.component.html',
  styleUrl: './add-setlist-entry-form.component.css',
})
export class AddSetlistEntryFormComponent implements OnInit {
  private toastr = inject(ToastrService);
  private formBuilder = inject(FormBuilder);
  private songService = inject(SongsService);

  setlistEntryForm = this.formBuilder.group({
    entryType: new FormControl(AddSetlistEntryFormContent.entryTypeSong, [Validators.required]),
    sortNumber: new FormControl(0, [Validators.min(1), Validators.max(10000), Validators.required]),
    songNumber: new FormControl(0, [Validators.max(99)]),
    titleOverride: new FormControl('', [Validators.maxLength(31)]),
    extraNotes: new FormControl('', [Validators.maxLength(127)]),
    selectedActNumber: new FormControl(0, []),
    actNumber: new FormControl('', []),
    actTitle: new FormControl('', [Validators.maxLength(31)]),
    selectedSongId: new FormControl(0, []),
    songTitle: new FormControl('', [Validators.maxLength(31)]),
    songIsrc: new FormControl('', []),
    selectedSongVariantId: new FormControl(0, []),
    songVariantName: new FormControl('', []),
    songVariantDescription: new FormControl('', []),
    wasPlayedFromRecording: new FormControl(false, []),
    wasRotationSong: new FormControl(false, []),
    wasWorldPremiere: new FormControl(false, []),
  });

  selectedEntryType$: string = AddSetlistEntryFormContent.entryTypeSong;

  availableSongs$: SongDto[] = [];
  availableActs$: SetlistActDto[] = [];

  variantsOfSelectedSong$: SongVariantDto[] = [];

  showAddActFields: boolean = false;
  showAddSongFields: boolean = false;
  showAddNewVariantFields: boolean = false;

  ngOnInit(): void {
    this.setlistEntryForm.controls.selectedSongVariantId.disable();

    this.setlistEntryForm.controls.entryType.valueChanges.subscribe(value => {
      this.selectedEntryType$ = value ?? AddSetlistEntryFormContent.entryTypeSong;
    });

    this.songService
      .getAllSongs(true)
      .subscribe({
        next: data => {
          this.availableSongs$ = data;
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not load songs");
        }
      });
  }


  onActSelectionChanged() {
    let actNumber = Number(this.setlistEntryForm.value.selectedActNumber?.valueOf());
    console.debug("Act selected: ", actNumber);

    this.showAddActFields = actNumber == -1;
  }


  onSongSelectionChanged() {
    let songId = Number(this.setlistEntryForm.value.selectedSongId?.valueOf());
    console.debug("Song selected: ", songId);

    if (songId > 0) {
      this.loadVariantsOfSelectedSong(songId);
      this.setlistEntryForm.controls.selectedSongVariantId.enable();
    } else {
      this.setlistEntryForm.controls.selectedSongVariantId.disable();
    }

    this.showAddSongFields = songId == -1;
  }


  onSongVariantSelectionChanged() {
    let songVariantId = Number(this.setlistEntryForm.value.selectedSongVariantId?.valueOf());
    console.debug("Variant selected: ", songVariantId);

    this.showAddNewVariantFields = songVariantId == -1;
  }


  private loadVariantsOfSelectedSong(songId: number) {
    console.debug("Load Variants of selectedSong");
    this.variantsOfSelectedSong$ = [];

    this.songService.getVariantsOfSong(songId)
      .subscribe(variants => {
        this.variantsOfSelectedSong$ = variants;
      });
  }


  public readValuesFromForm(): AddSetlistEntryFormContent {
    let entryType = this.selectedEntryType$;
    let content: AddSetlistEntryFormContent = {
      entryType: entryType,
      sortNumber: this.setlistEntryForm.value.sortNumber?.valueOf(),
      songNumber: this.setlistEntryForm.value.songNumber?.valueOf(),
      titleOverride: this.setlistEntryForm.value.titleOverride?.valueOf(),
      extraNotes: this.setlistEntryForm.value.extraNotes?.valueOf(),
      wasRotationSong: this.setlistEntryForm.value.wasRotationSong?.valueOf() ?? false,
      wasPlayedFromRecording: this.setlistEntryForm.value.wasPlayedFromRecording?.valueOf() ?? false,
      wasWorldPremiere: this.setlistEntryForm.value.wasWorldPremiere?.valueOf() ?? false,
      selectedActNumber: this.setlistEntryForm.value.selectedActNumber?.valueOf(),
      // these fields are not present in every case
      actNumber: undefined,
      actTitle: undefined,
      selectedSongId: undefined,
      selectedSongVariantId: undefined,
      songIsrc: undefined,
      songTitle: undefined,
      songVariantDescription: undefined,
      songVariantName: undefined,
    }

    if (content.selectedActNumber ?? 0 < 0) {
      content.actNumber = Number(this.setlistEntryForm.value.actNumber?.valueOf());
      content.actTitle = this.setlistEntryForm.value.actTitle?.valueOf() ?? null;
    }

    if (entryType == AddSetlistEntryFormContent.entryTypeSong) {
      console.debug("Adding fields about a song...");

      content.selectedSongId = this.setlistEntryForm.value.selectedSongId?.valueOf();
      content.songTitle = this.setlistEntryForm.value.songTitle?.valueOf();
      content.songIsrc = this.setlistEntryForm.value.songIsrc?.valueOf();

      let songVariantId = this.setlistEntryForm.value.selectedSongVariantId?.valueOf() ?? 0;
      if (songVariantId != 0) {
        console.debug("The entry is a song variant!");
        content.entryType = AddSetlistEntryFormContent.entryTypeSongVariant;

        content.selectedSongVariantId = songVariantId;
        content.songVariantName = this.setlistEntryForm.value.songVariantName?.valueOf();
        content.songVariantDescription = this.setlistEntryForm.value.songVariantDescription?.valueOf();
      }
    }

    return content;
  }


  public AddSetlistEntryFormContent = AddSetlistEntryFormContent;
}


export class AddSetlistEntryFormContent {
  static entryTypeSong = "song";
  static entryTypeSongVariant = "songVariant";
  static entryTypeSongMashup = "songMashup";

  /**
   * Type of the entry.
   */
  entryType: string | undefined;

  /**
   * Number that is only used for sorting
   */
  sortNumber: number | undefined;

  /**
   * Number of the song in the setlist.
   */
  songNumber: number | undefined;

  /**
   * optional title that would override the title of the song or mashup in this entry
   */
  titleOverride: string | undefined | null;

  /**
   * Additional notes about this entry
   */
  extraNotes: string | undefined | null;

  /**
   * ID of the act that was selected. null if this entry does not contain a song. "-1" if a new act will be added
   */
  selectedActNumber: number | undefined | null;

  /**
   * Number of the act within the setlist. null if this entry is not part of an Act
   */
  actNumber: number | undefined;

  /**
   * Optional title of the act within the setlist. null if this entry is not part of an Act
   */
  actTitle: string | undefined | null;

  /**
   * ID of the song that was selected. null if this entry does not contain a song. "-1" if a new song will be added
   */
  selectedSongId: number | undefined | null;

  /**
   * If the selectedSongId is "-1", this field contains the title of the new song
   */
  songTitle: string | undefined | null;

  /**
   * If the selectedSongId is "-1", this field contains the ISRC of the new song
   */
  songIsrc: string | undefined | null;

  /**
   * ID of the song variant that was selected. null if this entry does not contain a song. "-1" if a new song will be added
   */
  selectedSongVariantId: number | undefined | null;

  /**
   * If the selectedSongVariantId is "-1", this field contains the name of the new variant
   */
  songVariantName: string | undefined | null;

  /**
   * If the selectedSongVariantId is "-1", this field contains the description of the new variant
   */
  songVariantDescription: string | undefined | null;

  /**
   * true if the song was played from a recording
   */
  wasPlayedFromRecording: boolean = false;

  /**
   * true if this song changes between different sets on the same tour
   */
  wasRotationSong: boolean = false;

  /**
   * true if this was the world premiere of a new song
   */
  wasWorldPremiere: boolean = false;
}
