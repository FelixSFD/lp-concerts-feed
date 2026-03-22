import {Component, inject, Input, OnInit} from '@angular/core';
import {FormBuilder, FormControl, ReactiveFormsModule, Validators} from "@angular/forms";
import {SongsService} from '../../../services/songs.service';
import {
  ErrorResponseDto,
  RawSetlistEntryDto,
  SetlistActDto,
  SongDto, SongMashupDto,
  SongVariantDto
} from '../../../modules/lpshows-api';
import {ToastrService} from 'ngx-toastr';
import {NgClass} from '@angular/common';
import {SetlistsService} from '../../../services/setlists.service';
import {nullIfEmpty} from '../../../helper/string-helper'

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
  private setlistsService = inject(SetlistsService);

  @Input("setlist-id")
  setlistId: number | undefined;

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
    selectedSongMashupId: new FormControl(0, []),
    wasPlayedFromRecording: new FormControl(false, []),
    wasRotationSong: new FormControl(false, []),
    wasWorldPremiere: new FormControl(false, []),
  });

  selectedEntryType$: string = AddSetlistEntryFormContent.entryTypeSong;

  availableSongMashups$: SongMashupDto[] = [];
  availableSongs$: SongDto[] = [];
  availableActs$: SetlistActDto[] = [];

  variantsOfSelectedSong$: SongVariantDto[] = [];

  showAddActFields: boolean = false;
  showAddSongFields: boolean = false;
  showAddNewVariantFields: boolean = false;

  // To wort around race conditions (entry loading before the select-fields), the entry can be stored here
  private storedEntry: RawSetlistEntryDto | null = null;

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
          console.debug("Loaded songs: ", this.availableSongs$);

          if (this.storedEntry) {
            console.debug("Has a stored entry: ", this.storedEntry);
            let songId = this.storedEntry.playedSong?.id ?? 0;
            if (songId <= 0) {
              songId = this.storedEntry.playedSongVariant?.songId ?? 0;
            }

            if (songId > 0) {
              this.setlistEntryForm.controls.selectedSongId.setValue(songId);
              this.loadVariantsOfSelectedSong(songId);
            }
          }
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not load songs");
        }
      });

    this.songService
      .getAllMashups(true)
      .subscribe({
        next: data => {
          this.availableSongMashups$ = data;
          console.debug("Loaded mashups: ", this.availableSongMashups$);

          if (this.storedEntry) {
            console.debug("Has a stored entry: ", this.storedEntry);
            let mashupId = this.storedEntry.playedSongMashup?.id ?? 0;

            if (mashupId > 0) {
              this.setlistEntryForm.controls.selectedSongMashupId.setValue(mashupId);
            }
          }
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not load mashups");
        }
      });

    this.setlistsService
      .getSetlistActs(this.setlistId!, false)
      .subscribe({
        next: data => {
          this.availableActs$ = data;
          console.debug("Loaded acts: ", this.availableActs$);
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not load acts");
        }
      })
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
        console.debug("Loaded Variants of selectedSong", variants);
        this.variantsOfSelectedSong$ = variants;

        if (this.storedEntry) {
          let songVariantId = this.storedEntry.playedSongVariant?.id;
          this.setlistEntryForm.controls.selectedSongVariantId.setValue(songVariantId ?? 0);
        }
      });
  }


  public loadEntry(setlistId: number, entryId: string) {
    this.setlistEntryForm.disable();

    this.setlistsService.getSetlistEntry(setlistId, entryId, false)
      .subscribe({
        next: entry => {
          this.storedEntry = entry;

          console.debug("Available songs: ", this.availableSongs$);

          this.setlistEntryForm.controls.sortNumber.setValue(entry.sortNumber ?? null);
          this.setlistEntryForm.controls.songNumber.setValue(entry.songNumber ?? null);
          this.setlistEntryForm.controls.titleOverride.setValue(entry.titleOverride ?? null);
          this.setlistEntryForm.controls.extraNotes.setValue(entry.extraNotes ?? null);
          this.setlistEntryForm.controls.selectedActNumber.setValue(entry.actNumber ?? null);
          this.setlistEntryForm.controls.selectedSongId.setValue(entry.playedSong?.id ?? null);
          this.setlistEntryForm.controls.selectedSongVariantId.setValue(entry.playedSongVariant?.id ?? null);
          this.setlistEntryForm.controls.selectedSongMashupId.setValue(entry.playedSongMashup?.id ?? null);
          this.setlistEntryForm.controls.wasWorldPremiere.setValue(entry.isWorldPremiere ?? false);
          this.setlistEntryForm.controls.wasPlayedFromRecording.setValue(entry.isPlayedFromRecording ?? false);
          this.setlistEntryForm.controls.wasRotationSong.setValue(entry.isRotationSong ?? false);

          if (entry.playedSongMashup != null) {
            this.setlistEntryForm.controls.entryType.setValue(AddSetlistEntryFormContent.entryTypeSongMashup);
          }

          this.setlistEntryForm.enable();
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not load entry");
        }
      })
    //this.setlistEntryForm.controls.entryType.setValue(data.entryType ?? null);
  }


  public readValuesFromForm(): AddSetlistEntryFormContent {
    let entryType = this.selectedEntryType$;

    let sortNumber = this.setlistEntryForm.value.sortNumber?.valueOf();
    let songNumber = this.setlistEntryForm.value.songNumber?.valueOf();
    let titleOverride = this.setlistEntryForm.value.titleOverride?.valueOf() ?? null;
    let extraNotes = this.setlistEntryForm.value.extraNotes?.valueOf();
    let wasRotationSong = this.setlistEntryForm.value.wasRotationSong?.valueOf() ?? false;
    let wasPlayedFromRecording = this.setlistEntryForm.value.wasPlayedFromRecording?.valueOf() ?? false;
    let wasWorldPremiere = this.setlistEntryForm.value.wasWorldPremiere?.valueOf() ?? false;
    let selectedActNumber = Number(this.setlistEntryForm.value.selectedActNumber?.valueOf());

    let content: AddSetlistEntryFormContent = {
      entryType: entryType,
      sortNumber: sortNumber,
      songNumber: songNumber,
      titleOverride: nullIfEmpty(titleOverride),
      extraNotes: nullIfEmpty(extraNotes),
      wasRotationSong: wasRotationSong,
      wasPlayedFromRecording: wasPlayedFromRecording,
      wasWorldPremiere: wasWorldPremiere,
      selectedActNumber: selectedActNumber,
      // these fields are not present in every case
      actNumber: undefined,
      actTitle: undefined,
      selectedSongId: undefined,
      selectedSongVariantId: undefined,
      songIsrc: undefined,
      songTitle: undefined,
      songVariantDescription: undefined,
      songVariantName: undefined,
      selectedSongMashupId: undefined,
    }

    if (content.selectedActNumber ?? 0 < 0) {
      content.actNumber = Number(this.setlistEntryForm.value.actNumber?.valueOf());
      content.actTitle = nullIfEmpty(this.setlistEntryForm.value.actTitle?.valueOf() ?? null);
    }

    if (entryType == AddSetlistEntryFormContent.entryTypeSong) {
      console.debug("Adding fields about a song...");

      content.selectedSongId = this.setlistEntryForm.value.selectedSongId?.valueOf();
      content.songTitle = nullIfEmpty(this.setlistEntryForm.value.songTitle?.valueOf());
      content.songIsrc = nullIfEmpty(this.setlistEntryForm.value.songIsrc?.valueOf());

      let songVariantId = this.setlistEntryForm.value.selectedSongVariantId?.valueOf() ?? 0;
      if (songVariantId != 0) {
        console.debug("The entry is a song variant!");
        content.entryType = AddSetlistEntryFormContent.entryTypeSongVariant;

        content.selectedSongVariantId = songVariantId;
        content.songVariantName = nullIfEmpty(this.setlistEntryForm.value.songVariantName?.valueOf());
        content.songVariantDescription = nullIfEmpty(this.setlistEntryForm.value.songVariantDescription?.valueOf());
      }
    } else if (entryType == AddSetlistEntryFormContent.entryTypeSongMashup) {
      console.debug("This entry is a song mashup...");
      content.selectedSongMashupId = this.setlistEntryForm.value.selectedSongMashupId?.valueOf();
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
   * ID of the song mashup that was selected. null if this entry does not contain a song.
   */
  selectedSongMashupId: number | undefined | null;

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
