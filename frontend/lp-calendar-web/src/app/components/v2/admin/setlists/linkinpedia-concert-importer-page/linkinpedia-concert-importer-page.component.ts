import { Component, inject, OnInit, signal, viewChild } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgTemplateOutlet } from '@angular/common';
import { SetlistsService } from '../../../../../services/setlists.service';
import {
  ActParametersDto,
  AddSongMashupToSetlistRequestDto,
  AddSongToSetlistRequestDto,
  AddSongVariantToSetlistRequestDto,
  CreateSetlistRequestDto,
  CreateSongMashupRequestDto,
  CreateSongRequestDto,
  ImportSetlistActPreviewDto,
  ImportSetlistEntryPreviewDto,
  ImportSetlistPreviewDto,
  ImportSetlistRequestDto,
  ProblemDetailsDto,
  SetlistEntryParametersDto,
  SongDto,
  SongMashupDto,
  SongVariantDto
} from '../../../../../modules/lpshows-api/v3';
import { SongFormComponent } from '../song-form/song-form.component';
import { SongsService } from '../../../../../services/songs.service';
import { MashupFormComponent } from '../mashup-form/mashup-form.component';
import { ActivatedRoute, Router } from '@angular/router';
import { DateTime } from 'luxon';
import { ConcertsService } from '../../../../../services/concerts.service';
import { Card } from 'primeng/card';
import { Button } from 'primeng/button';
import { FloatLabel } from 'primeng/floatlabel';
import { InputGroup } from 'primeng/inputgroup';
import { InputGroupAddon } from 'primeng/inputgroupaddon';
import { InputText } from 'primeng/inputtext';
import { Divider } from 'primeng/divider';
import { TableModule } from 'primeng/table';
import { Tooltip } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { Dialog } from 'primeng/dialog';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-linkinpedia-concert-importer-page',
  imports: [
    ReactiveFormsModule,
    SongFormComponent,
    MashupFormComponent,
    NgTemplateOutlet,
    Card,
    Button,
    FloatLabel,
    InputGroup,
    InputGroupAddon,
    InputText,
    Divider,
    TableModule,
    Tooltip,
    Dialog
  ],
  templateUrl: './linkinpedia-concert-importer-page.component.html',
  styleUrl: './linkinpedia-concert-importer-page.component.css',
})
export class LinkinpediaConcertImporterPageComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private readonly messageService = inject(MessageService);
  private readonly setlistsService = inject(SetlistsService);
  private readonly songsService = inject(SongsService);
  private readonly concertsService = inject(ConcertsService);

  // true if the page is currently reading the source information
  isReadingSource$ = signal(false);

  // true if the page is currently saving the new setlist
  isCreatingSetlist$ = signal(false);

  isShowingAddSongDialog$ = signal(false);
  isShowingAddMashupDialog$ = signal(false);

  allEntriesValid$ = signal(false);

  sourceDataForm = this.formBuilder.group({
    concertId: new FormControl('', [Validators.required]),
    concertTitle: new FormControl('', [Validators.required, Validators.min(5)]),
    setName: new FormControl('', []),
    linkinpediaUrl: new FormControl('https://linkinpedia.com/wiki/Live:20240905', [Validators.required, Validators.pattern(/^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)/)]),
  });

  generatedSetlist$ = signal<ImportSetlistPreviewDto | null>(null);
  private originallyGeneratedSetlist: ImportSetlistPreviewDto | null = null;

  // properties for the add song modal
  isAddingSong$ = signal(false);
  private newSongFormComponent = viewChild(SongFormComponent);

  // properties for the add mashup modal
  isAddingMashup$ = signal(false);
  private newMashupFormComponent = viewChild(MashupFormComponent);


  ngOnInit() {
    this.sourceDataForm.controls.concertId.valueChanges.subscribe(async id => {
      console.debug("Changed id", id);
      if (id != null) {
        try {
          const concert = await this.concertsService.getDetailsById(id);
          const startDate = DateTime.fromISO(concert.postedStartTime!, { zone: concert.venue?.timeZoneId });
          const year = startDate.year.toString();
          const month = startDate.month < 10 ? '0' + startDate.month : startDate.month;
          const day = startDate.day < 10 ? '0' + startDate.day : startDate.day;
          const location = concert.venue?.city ? `${concert.venue.city.name}, ${concert.venue.city.country.name}` : (concert.venue?.currentName ?? '');
          this.sourceDataForm.controls.concertTitle.setValue(`${location} - ${year}-${month}-${day}`);
          this.sourceDataForm.controls.linkinpediaUrl.setValue(`https://linkinpedia.com/wiki/Live:${year}${month}${day}`);
        } catch (err: any) {
          console.warn("Could not load concert details:", err);
          const errorResponse: ProblemDetailsDto = err?.error ?? err;
          this.messageService.add({
            severity: "error",
            summary: "Could not load concert details!",
            detail: errorResponse?.detail ?? errorResponse?.title ?? "Failed to fetch concert"
          });
        }
      }
    });

    this.route.params.subscribe(params => {
      const concertId = params['id'];
      if (concertId != null && concertId.length > 0) {
        this.sourceDataForm.controls.concertId.setValue(params['id'], { emitEvent: true });
        this.sourceDataForm.controls.concertId.disable();
      }
    });
  }


  async onLoadSourceClicked() {
    await this.startImport();
  }


  private async startImport() {
    const url = this.sourceDataForm.value.linkinpediaUrl?.valueOf();
    const concertId = this.sourceDataForm.getRawValue().concertId?.valueOf() || "123";
    if (url) {
      this.isReadingSource$.set(true);
      try {
        const request: ImportSetlistRequestDto = {
          concertId: concertId,
          linkinpediaUrl: url
        };
        const data = await this.setlistsService.importSetlistFromLinkinpedia(concertId, request);
        this.generatedSetlist$.set(data);
        this.originallyGeneratedSetlist = JSON.parse(JSON.stringify(data));
        this.validateEntries();

        this.messageService.add({
          severity: "success",
          summary: "Successfully read setlist from Linkinpedia",
        });
      } catch (err: any) {
        const errorResponse: ProblemDetailsDto = err?.error ?? err;
        console.warn("Failed to fetch import data:", err);

        this.messageService.add({
          severity: "error",
          summary: "Could not get import data!",
          detail: errorResponse?.detail ?? errorResponse?.title ?? errorResponse?.['message'] ?? "Could not get import data!",
        });
      } finally {
        this.isReadingSource$.set(false);
      }
    }
  }


  private validateEntries() {
    const entries = this.generatedSetlist$()?.entries ?? [];
    this.allEntriesValid$.set(entries.length > 0 && entries.every(entry => entry.foundSongId != null || entry.foundSongVariantId != null || entry.foundMashupId != null));
  }


  openCreateSongBtnClicked(entryPreview: ImportSetlistEntryPreviewDto) {
    const songPrefill: SongDto = {
      title: entryPreview.title,
    };

    this.isShowingAddSongDialog$.set(true);
    this.newSongFormComponent()?.fillFormWith(songPrefill);
  }


  async onAddSongConfirm() {
    this.isAddingSong$.set(true);

    // find the song information
    const newSongValues = this.newSongFormComponent()?.readFromForm();
    if (!newSongValues) {
      this.messageService.add({
        severity: "error",
        summary: "Failed to read data from form",
      });
      this.isAddingSong$.set(false);
      return;
    }

    const createRequest: CreateSongRequestDto = {
      title: newSongValues.title,
      albumId: Number(newSongValues.albumId),
      isrc: newSongValues.isrc,
      appleMusicId: newSongValues.appleMusicId,
      linkinpediaUrl: newSongValues.linkinpediaUrl,
    };

    console.debug('Creating new song...', createRequest);

    try {
      await firstValueFrom(this.songsService.createSong(createRequest));
      this.onLoadSourceClicked();
      this.dismissAddSongModal();
    } catch (err: any) {
      const errorResponse: ProblemDetailsDto = err?.error ?? err;
      console.warn("Failed to create song:", err);

      this.messageService.add({
        severity: "error",
        summary: "Could not create new song!",
        detail: errorResponse?.detail ?? errorResponse?.title ?? errorResponse?.['message'] ?? "Could not create new song!",
      });
    } finally {
      this.isAddingSong$.set(false);
    }
  }


  dismissAddSongModal() {
    this.isShowingAddSongDialog$.set(false);
  }


  openCreateMashupBtnClicked(entryPreview: ImportSetlistEntryPreviewDto) {
    const mashupPrefill: SongMashupDto = {
      title: entryPreview.title,
    };

    this.isShowingAddMashupDialog$.set(true);
    this.newMashupFormComponent()?.fillFormWith(mashupPrefill);
  }


  async onAddMashupConfirm() {
    this.isAddingMashup$.set(true);

    // find the song information
    const newMashupValues = this.newMashupFormComponent()?.readFromForm();
    if (!newMashupValues) {
      this.messageService.add({
        severity: "error",
        summary: "Failed to read data from form",
      });
      this.isAddingMashup$.set(false);
      return;
    }

    const createRequest: CreateSongMashupRequestDto = {
      title: newMashupValues.title,
      linkinpediaUrl: newMashupValues.linkinpediaUrl,
      songIds: newMashupValues.songs.map(s => s.id!),
    };

    console.debug('Creating new song mashup...', createRequest);

    try {
      await firstValueFrom(this.songsService.createMashup(createRequest));
      this.onLoadSourceClicked();
      this.dismissAddMashupModal();
    } catch (err: any) {
      const errorResponse: ProblemDetailsDto = err?.error ?? err;
      console.warn("Failed to create song mashup:", err);

      this.messageService.add({
        severity: "error",
        summary: "Could not create new song mashup!",
        detail: errorResponse?.detail ?? errorResponse?.title ?? errorResponse?.['message'] ?? "Could not create new song mashup!",
      });
    } finally {
      this.isAddingMashup$.set(false);
    }
  }


  dismissAddMashupModal() {
    this.isShowingAddMashupDialog$.set(false);
  }


  setVariantBtnClicked(entryPreview: ImportSetlistEntryPreviewDto, variant: SongVariantDto) {
    console.log("use variant: ", variant);
    entryPreview.title += " (" + variant.variantName + ")";
    entryPreview.foundSongVariantId = variant.id;
    this.validateEntries();
  }

  setNormalVersionBtnClicked(entryPreview: ImportSetlistEntryPreviewDto) {
    entryPreview.title = this.originallyGeneratedSetlist?.entries?.find((e) => e.songNumber == entryPreview.songNumber)?.title ?? "failed to reload title";
    entryPreview.foundSongVariantId = null;
    this.validateEntries();
  }


  changeToPlainTextEntryBtnClicked(entryPreview: ImportSetlistEntryPreviewDto) {
    entryPreview.foundSongId = -1;
    this.validateEntries();
  }


  openConcertDetailsClicked() {
    const concertId = this.sourceDataForm.getRawValue().concertId?.valueOf();
    if (concertId?.length == 0) {
      return;
    }

    window.open("/concerts/" + concertId, "_blank");
  }


  async onCreateSetlistClicked() {
    const concertId = this.sourceDataForm.getRawValue().concertId?.valueOf();
    if (!concertId) {
      this.messageService.add({
        severity: "error",
        summary: "Concert ID not set!",
      });
      return;
    }

    const concertTitle = this.sourceDataForm.value.concertTitle?.valueOf();
    if (!concertTitle) {
      this.messageService.add({
        severity: "error",
        summary: "Concert Title not set!",
      });
      return;
    }

    const linkinpediaUrl = this.sourceDataForm.value.linkinpediaUrl?.valueOf();
    if (!linkinpediaUrl) {
      this.messageService.add({
        severity: "error",
        summary: "Linkinpedia URL not set!",
      });
      return;
    }

    const setName = this.sourceDataForm.value.setName?.valueOf() ?? null;
    const currentSetlist = this.generatedSetlist$();

    const createRequest: CreateSetlistRequestDto = {
      concertId: concertId,
      concertTitle: concertTitle,
      setName: setName,
      linkinpediaUrl: linkinpediaUrl,
      addSongs: currentSetlist?.entries?.filter(e => e.foundSongId && (e.foundSongVariantId ?? 0) <= 0).map(e => {
        const currentAct = currentSetlist?.acts?.filter(a => a.actNumber == e.actNumber).at(0) ?? null;

        // it's either a new or an existing song
        if (Number(e.foundSongId) >= 0) {
          console.debug("Song ID: ", e.foundSongId);
          const dto: AddSongToSetlistRequestDto = {
            songParameters: {
              id: e.foundSongId ?? undefined
            },
            actParameters: this.getCreateActParameters(e, currentAct),
            entryParameters: this.getCreateEntryParameters(e)
          };
          return dto;
        } else {
          // not an actual song. Just a plain-text entry
          const dto: AddSongToSetlistRequestDto = {
            songParameters: {},
            actParameters: this.getCreateActParameters(e, currentAct),
            entryParameters: this.getCreateEntryParameters(e)
          };
          dto.entryParameters!.titleOverride = e.title ?? "Unknown Song";

          console.debug("Entry is a plain-text entry, not a real song", dto);

          return dto;
        }
      }) ?? [],
      addSongVariants: currentSetlist?.entries?.filter(e => e.foundSongVariantId).map(e => {
        const currentAct = currentSetlist?.acts?.filter(a => a.actNumber == e.actNumber).at(0) ?? null;
        const dto: AddSongVariantToSetlistRequestDto = {
          songVariantParameters: {
            songId: e.foundSongId ?? undefined,
            id: e.foundSongVariantId ?? undefined,
          },
          actParameters: this.getCreateActParameters(e, currentAct),
          entryParameters: this.getCreateEntryParameters(e)
        };
        return dto;
      }) ?? [],
      addSongMashups: currentSetlist?.entries?.filter(e => e.foundMashupId).map(e => {
        const currentAct = currentSetlist?.acts?.filter(a => a.actNumber == e.actNumber).at(0) ?? null;
        const dto: AddSongMashupToSetlistRequestDto = {
          songMashupParameters: {
            id: e.foundMashupId ?? undefined
          },
          actParameters: this.getCreateActParameters(e, currentAct),
          entryParameters: this.getCreateEntryParameters(e)
        };
        return dto;
      }) ?? [],
    };

    console.debug('Creating new set list...', createRequest);
    this.isCreatingSetlist$.set(true);

    try {
      const data = await this.setlistsService.createSetlist(createRequest);
      try {
        await this.router.navigate(["/admin", "setlists", data.id]);
      } catch (reason: any) {
        this.messageService.add({
          severity: "error",
          summary: "Failed to navigate to created setlist!",
          detail: reason?.message ?? reason,
        });
      }
    } catch (err: any) {
      const errorResponse: ProblemDetailsDto = err?.error ?? err;
      console.warn("Failed create setlist:", err);

      this.messageService.add({
        severity: "error",
        summary: "Could not create setlist!",
        detail: errorResponse?.detail ?? errorResponse?.title ?? errorResponse?.['message'] ?? "Could not create setlist!",
      });
    } finally {
      this.isCreatingSetlist$.set(false);
    }
  }


  private getCreateEntryParameters(previewEntry: ImportSetlistEntryPreviewDto) : SetlistEntryParametersDto {
    return {
      songNumber: previewEntry.songNumber,
      sortNumber: (previewEntry.songNumber ?? 1) * 10,
      extraNotes: previewEntry.extraNotes,
    };
  }


  private getCreateActParameters(previewEntry: ImportSetlistEntryPreviewDto, previewAct: ImportSetlistActPreviewDto | null) : ActParametersDto | undefined {
    if (!previewAct) {
      return undefined;
    }

    return {
      actNumber: previewEntry.actNumber ?? previewAct.actNumber,
      title: previewAct.name,
    };
  }
}
