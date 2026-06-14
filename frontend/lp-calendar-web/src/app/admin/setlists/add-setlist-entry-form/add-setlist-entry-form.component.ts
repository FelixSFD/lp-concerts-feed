import {Component, inject, Input, OnInit, TemplateRef, viewChild} from '@angular/core';
import {FormBuilder, FormControl, ReactiveFormsModule, Validators} from "@angular/forms";
import {SongsService} from '../../../services/songs.service';
import {
  AddSongExtraToSetlistEntryRequestDto,
  ErrorResponseDto,
  RawSetlistEntryDto,
  SetlistActDto, SetlistEntrySongExtraDto,
  SongDto, SongMashupDto,
  SongVariantDto
} from '../../../modules/lpshows-api';
import {ToastrService} from 'ngx-toastr';
import {NgClass, NgTemplateOutlet} from '@angular/common';
import {SetlistsService} from '../../../services/setlists.service';
import {nullIfEmpty} from '../../../helper/string-helper'
import {
  SetlistEntrySongExtraFormComponent
} from '../setlist-entry-song-extra-form/setlist-entry-song-extra-form.component';
import {NgbModal, NgbModalRef, NgbTooltip} from '@ng-bootstrap/ng-bootstrap';
import {InputNumber} from 'primeng/inputnumber';
import {FloatLabel} from 'primeng/floatlabel';
import {Select} from 'primeng/select';
import {Button, ButtonSeverity} from 'primeng/button';
import {Dialog} from 'primeng/dialog';
import {Divider} from 'primeng/divider';
import {InputText} from 'primeng/inputtext';
import {SelectButton} from 'primeng/selectbutton';

@Component({
  selector: 'app-add-setlist-entry-form',
  imports: [
    ReactiveFormsModule,
    NgClass,
    SetlistEntrySongExtraFormComponent,
    NgbTooltip,
    InputNumber,
    FloatLabel,
    Select,
    NgTemplateOutlet,
    Button,
    Dialog,
    InputText,
    SelectButton
  ],
  templateUrl: './add-setlist-entry-form.component.html',
  styleUrl: './add-setlist-entry-form.component.css',
})
export class AddSetlistEntryFormComponent implements OnInit {
  private modalService = inject(NgbModal);
  private toastr = inject(ToastrService);
  private formBuilder = inject(FormBuilder);
  private songService = inject(SongsService);
  private setlistsService = inject(SetlistsService);

  private static songVariantNormalVersion: SongVariantDto = {
    variantName: "Normal version"
  };

  @Input("setlist-id")
  setlistId: number | undefined;

  setlistEntryForm = this.formBuilder.group({
    entryType: new FormControl(AddSetlistEntryFormContent.entryTypeSong, [Validators.required]),
    sortNumber: new FormControl(0, [Validators.min(1), Validators.max(10000), Validators.required]),
    songNumber: new FormControl(0, [Validators.max(99)]),
    titleOverride: new FormControl('', [Validators.maxLength(63)]),
    extraNotes: new FormControl('', [Validators.maxLength(255)]),
    selectedActNumber: new FormControl(0, []),
    actNumber: new FormControl<number>(1, []),
    actTitle: new FormControl('', [Validators.maxLength(31)]),
    selectedAct: new FormControl<SetlistActDto | null>(null, []),
    selectedSong: new FormControl<SongDto | null>(null, []),
    selectedSongVariant: new FormControl<SongVariantDto>(AddSetlistEntryFormComponent.songVariantNormalVersion, []),
    selectedSongMashup: new FormControl<SongMashupDto | null>(null, []),
    wasPlayedFromRecording: new FormControl(false, []),
    wasRotationSong: new FormControl(false, []),
    wasWorldPremiere: new FormControl(false, []),
    wasLivePremiere: new FormControl(false, []),
  });

  addActForm = this.formBuilder.group({
    actNumber: new FormControl<number>(1, [Validators.required]),
    actTitle: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(31)]),
  });

  addSongForm = this.formBuilder.group({
    songTitle: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(31)]),
    songIsrc: new FormControl('', []),
  });

  selectedEntryType$: string = AddSetlistEntryFormContent.entryTypeSong;

  availableSongMashups$: SongMashupDto[] = [];
  availableSongs$: SongDto[] = [];
  availableActs$: SetlistActDto[] = [];

  variantsOfSelectedSong$: SongVariantDto[] = [AddSetlistEntryFormComponent.songVariantNormalVersion];

  currentSongExtras$: SetlistEntrySongExtraDto[] = [];

  showAddActDialog: boolean = false;
  showAddSongDialog: boolean = false;
  showAddSongFields: boolean = false;

  isAddingSongExtra$: boolean = false;
  isDeletingSongExtra$: boolean = false;

  songExtraToDelete$: SetlistEntrySongExtraDto | null = null;

  // if open, the modal is referenced here
  addExtraModal: NgbModalRef | undefined;

  // if open, the modal is referenced here
  deleteExtraConfirmationModal: NgbModalRef | undefined;

  private addSongExtraFormComponent = viewChild(SetlistEntrySongExtraFormComponent);

  // To work around race conditions (entry loading before the select-fields), the entry can be stored here
  private storedEntry: RawSetlistEntryDto | null = null;

  ngOnInit(): void {
    this.setlistEntryForm.controls.selectedSongVariant.disable();

    this.setlistEntryForm.controls.entryType.valueChanges.subscribe(value => {
      this.selectedEntryType$ = value ?? AddSetlistEntryFormContent.entryTypeSong;
    });

    this.setlistEntryForm.controls.selectedSong.valueChanges.subscribe(value => {
      this.onSongSelectionChanged(value);
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
            if (songId > 0) {
              this.setlistEntryForm.controls.selectedSong.setValue(this.storedEntry.playedSong ?? null);
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
            this.setlistEntryForm.controls.selectedSongMashup.setValue(this.storedEntry.playedSongMashup ?? null);
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
          data.sort((a, b) => (a.actNumber ?? 0) < (b.actNumber ?? 0) ? -1 : 1);
          this.availableActs$ = data;
          console.debug("Loaded acts: ", this.availableActs$);
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not load acts");
        }
      })
  }


  openModal(content: TemplateRef<any>) {
    return this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title', scrollable: true });
  }


  onAddSongExtraClicked(content: TemplateRef<any>) {
    this.addExtraModal = this.openModal(content);
  }

  dismissAddExtraModal() {
    this.addExtraModal?.dismiss();
  }

  onAddSongExtraConfirmed() {
    if (this.setlistId == null || this.storedEntry?.id == null) {
      this.toastr.error("Setlist Entry could not be identified!");
      return;
    }

    this.isAddingSongExtra$ = true;

    let formContent = this.addSongExtraFormComponent()?.readValuesFromForm();
    console.debug("Read extra from form: ", formContent);

    let addExtraRequest: AddSongExtraToSetlistEntryRequestDto = {
      type: formContent?.type,
      songId: Number(formContent?.songId ?? 0),
      description: formContent?.description,
    };

    this.setlistsService.addSongExtraToEntry(addExtraRequest, this.setlistId!, this.storedEntry!.id)
      .subscribe({
        next: data => {
          console.debug("Response after adding extra: ", data);
          if (this.storedEntry != null) {
            this.storedEntry.songExtras = data.songExtras ?? [];
          }

          this.currentSongExtras$ = data.songExtras ?? [];

          this.dismissAddExtraModal();
          this.isAddingSongExtra$ = false;
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not add extra to this entry");
          this.isAddingSongExtra$ = false;
        }
      });
  }


  onDeleteSongExtraClicked(content: TemplateRef<any>, extraId: string) {
    this.songExtraToDelete$ = this.currentSongExtras$.find(e => e.id == extraId) ?? null;

    if (this.songExtraToDelete$ == null) {
      return;
    }

    this.deleteExtraConfirmationModal = this.openModal(content);
  }


  confirmDeleteSongExtraConfirmationModal() {
    this.isDeletingSongExtra$ = true;
    console.debug("Delete song extra", this.songExtraToDelete$);

    let deleteId = this.songExtraToDelete$?.id ?? null;
    if (deleteId == null) {
      console.warn("ID of extra not found", this.songExtraToDelete$);
      return;
    }

    this.setlistsService.removeSongExtraFromEntry(deleteId, this.setlistId ?? 0, this.storedEntry?.id ?? "")
      .subscribe({
        next: _ => {
          if (this.storedEntry != null) {
            this.storedEntry.songExtras = this.storedEntry.songExtras?.filter(e => e.id != deleteId);
          }

          this.currentSongExtras$ = this.currentSongExtras$?.filter(e => e.id != deleteId);

          this.dismissDeleteSongExtraConfirmationModal();
          this.isDeletingSongExtra$ = false;
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not delete extra from this entry");
          this.isDeletingSongExtra$ = false;

          this.deleteExtraConfirmationModal?.dismiss();
        }
      })
  }


  dismissDeleteSongExtraConfirmationModal() {
    this.deleteExtraConfirmationModal?.dismiss();
  }


  onSongSelectionChanged(song: SongDto | null) {
    let songId = song?.id ?? 0;
    console.debug("Song selected: ", songId);

    if (songId > 0) {
      this.loadVariantsOfSelectedSong(songId);
      this.setlistEntryForm.controls.selectedSongVariant.enable();
    } else {
      this.setlistEntryForm.controls.selectedSongVariant.disable();
    }

    this.showAddSongFields = songId == -1;
  }

  onActAdd() {
    let newAct: SetlistActDto = {
      actNumber: Number(this.addActForm.value.actNumber),
      title: this.addActForm.value.actTitle,
    }

    console.debug("Add new Act", newAct);
    this.availableActs$.push(newAct);
    this.availableActs$.sort((a, b) => (a.actNumber ?? 0) < (b.actNumber ?? 0) ? -1 : 1);
    this.setlistEntryForm.controls.selectedAct.setValue(newAct);
    this.showAddActDialog = false;
    this.addActForm.reset();
  }


  openAddActDialog() {
    this.addActForm.controls.actNumber.setValue((this.availableActs$[this.availableActs$.length - 1]?.actNumber ?? 0) + 1);
    this.showAddActDialog = true;
  }


  openAddSongDialog() {
    this.showAddSongDialog = true;
  }

  onSongAdd() {
    let newSong: SongDto = {
      title: this.addSongForm.value.songTitle,
      isrc: this.addSongForm.value.songIsrc,
    }

    console.debug("Add new song", newSong);
    this.availableSongs$.push(newSong);
    this.availableSongs$.sort((a, b) => (a.title ?? "") < (b.title ?? "") ? -1 : 1);
    this.setlistEntryForm.controls.selectedSong.setValue(newSong);
    this.showAddSongDialog = false;
    this.addSongForm.reset();
  }


  public setSongNumber(songNumber: number) {
    this.setlistEntryForm.controls.songNumber.setValue(songNumber);
  }


  private loadVariantsOfSelectedSong(songId: number) {
    console.debug("Load Variants of selectedSong");
    this.setlistEntryForm.controls.selectedSongVariant.disable();
    this.variantsOfSelectedSong$ = [AddSetlistEntryFormComponent.songVariantNormalVersion];

    this.songService.getVariantsOfSong(songId)
      .subscribe(variants => {
        console.debug("Loaded Variants of selectedSong", variants);
        this.variantsOfSelectedSong$ = [AddSetlistEntryFormComponent.songVariantNormalVersion, ...variants];

        if (this.storedEntry) {
          let songVariantId = this.storedEntry.playedSongVariant?.id ?? 0;
          let foundVariant = this.variantsOfSelectedSong$.find(e => e.id == songVariantId);
          this.setlistEntryForm.controls.selectedSongVariant.setValue(foundVariant ?? AddSetlistEntryFormComponent.songVariantNormalVersion);
        }

        this.setlistEntryForm.controls.selectedSongVariant.enable();
      });
  }


  public loadEntry(setlistId: number, entryId: string) {
    this.setlistEntryForm.disable();

    this.setlistsService.getSetlistEntry(setlistId, entryId, false)
      .subscribe({
        next: entry => {
          this.storedEntry = entry;

          console.debug("Loaded entry: ", this.storedEntry);

          console.debug("Available songs: ", this.availableSongs$);

          this.setlistEntryForm.controls.sortNumber.setValue(entry.sortNumber ?? null);
          this.setlistEntryForm.controls.songNumber.setValue(entry.songNumber ?? null);
          this.setlistEntryForm.controls.titleOverride.setValue(entry.titleOverride ?? null);
          this.setlistEntryForm.controls.extraNotes.setValue(entry.extraNotes ?? null);
          this.setlistEntryForm.controls.selectedActNumber.setValue(entry.actNumber ?? 0);
          this.setlistEntryForm.controls.selectedSong.setValue(entry.playedSong ?? this.availableSongs$.find(s => s.id == (entry.playedSongVariant?.songId ?? 0)) ?? null);
          this.setlistEntryForm.controls.selectedSongVariant.setValue(entry.playedSongVariant ?? null);
          this.setlistEntryForm.controls.selectedSongMashup.setValue(entry.playedSongMashup ?? null);
          this.setlistEntryForm.controls.wasWorldPremiere.setValue(entry.isWorldPremiere ?? false);
          this.setlistEntryForm.controls.wasPlayedFromRecording.setValue(entry.isPlayedFromRecording ?? false);
          this.setlistEntryForm.controls.wasRotationSong.setValue(entry.isRotationSong ?? false);
          this.setlistEntryForm.controls.wasLivePremiere.setValue(entry.isLivePremiere ?? false);

          if (entry.playedSong != null) {
            this.setlistEntryForm.controls.entryType.setValue(AddSetlistEntryFormContent.entryTypeSong);
          } else if (entry.playedSongVariant != null) {
            this.setlistEntryForm.controls.entryType.setValue(AddSetlistEntryFormContent.entryTypeSong);
          } else if (entry.playedSongMashup != null) {
            this.setlistEntryForm.controls.entryType.setValue(AddSetlistEntryFormContent.entryTypeSongMashup);
          } else {
            console.debug("This is a free-text entry");
            this.setlistEntryForm.controls.entryType.setValue(AddSetlistEntryFormContent.entryTypeFreeText);
          }

          this.currentSongExtras$ = this.storedEntry.songExtras ?? [];

          this.setlistEntryForm.enable();
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could not load entry");
        }
      });
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
    let wasLivePremiere = this.setlistEntryForm.value.wasLivePremiere?.valueOf() ?? false;
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
      wasLivePremiere: wasLivePremiere,
      selectedActNumber: selectedActNumber,
      // these fields are not present in every case
      actNumber: this.setlistEntryForm.value.selectedAct?.actNumber,
      actTitle: this.setlistEntryForm.value.selectedAct?.title,
      selectedSongId: undefined,
      selectedSongVariantId: undefined,
      songIsrc: undefined,
      songTitle: undefined,
      songVariantDescription: undefined,
      songVariantName: undefined,
      selectedSongMashupId: undefined,
    }

    if (entryType == AddSetlistEntryFormContent.entryTypeSong) {
      console.debug("Adding fields about a song...");

      let song = this.setlistEntryForm.value.selectedSong;
      content.selectedSongId = song?.id;
      content.songTitle = nullIfEmpty(song?.title ?? null);
      content.songIsrc = nullIfEmpty(song?.isrc ?? null);

      let songVariantId = this.setlistEntryForm.value.selectedSongVariant?.id ?? 0;
      if (songVariantId != 0) {
        console.debug("The entry is a song variant!");
        content.entryType = AddSetlistEntryFormContent.entryTypeSongVariant;

        content.selectedSongVariantId = songVariantId;
        content.songVariantName = nullIfEmpty(this.setlistEntryForm.value.selectedSongVariant?.variantName);
        content.songVariantDescription = nullIfEmpty(this.setlistEntryForm.value.selectedSongVariant?.description);
      }
    } else if (entryType == AddSetlistEntryFormContent.entryTypeSongMashup) {
      console.debug("This entry is a song mashup...");
      content.selectedSongMashupId = this.setlistEntryForm.value.selectedSongMashup?.id;
    }

    return content;
  }


  public AddSetlistEntryFormContent = AddSetlistEntryFormContent;
  protected readonly SetlistEntryTypeSelectorItem = SetlistEntryTypeSelectorItem;
}


export class AddSetlistEntryFormContent {
  static entryTypeFreeText = "freeText";
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

  /**
   * true if this was the live premiere of a new song
   */
  wasLivePremiere: boolean = false;
}

/**
 * A type of entry the user can select
 */
export class SetlistEntryTypeSelectorItem {
  static possibleTypes: SetlistEntryTypeSelectorItem[] = [
    {
      value: AddSetlistEntryFormContent.entryTypeSong,
      label: "Song",
      severity: "primary",
    },
    {
      value: AddSetlistEntryFormContent.entryTypeSongMashup,
      label: "Mashup/Medley",
      severity: "info",
    },
    {
      value: AddSetlistEntryFormContent.entryTypeFreeText,
      label: "Custom",
      severity: "secondary",
    },
  ];

  value: string;
  label: string;
  severity: ButtonSeverity;

  constructor(label: string, value: string, severity: ButtonSeverity) {
    this.value = value;
    this.label = label;
    this.severity = severity;
  }
}
