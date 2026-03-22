import {inject, Injectable} from '@angular/core';
import {AuthService} from '../auth/auth.service';
import {HttpClient} from '@angular/common/http';
import {
  CreateSetlistRequestDto,
  CreateSetlistResponseDto, SetlistDto,
  SetlistsService as SetlistsApiClient,
  ConcertsService as ConcertsApiClient, UpdateSetlistHeaderRequestDto, AddSongToSetlistRequestDto,
  SetlistEntryParametersDto, AddSongVariantToSetlistRequestDto, SetlistEntryDto, ActParametersDto, RawSetlistEntryDto,
  UpdateSetlistEntryRequestDto, SongParametersDto, SongVariantParametersDto, SetlistActDto,
  AddSongMashupToSetlistRequestDto, SongMashupParametersDto,
} from '../modules/lpshows-api';
import {map, Observable} from 'rxjs';
import {Guid} from 'guid-typescript';
import {AddSetlistEntryFormContent} from '../admin/setlists/add-setlist-entry-form/add-setlist-entry-form.component';

@Injectable({
  providedIn: 'root',
})
export class SetlistsService {
  private readonly authService = inject(AuthService);

  constructor(private httpClient: HttpClient, private setlistsApiClient: SetlistsApiClient, private concertsApiClient: ConcertsApiClient) { }


  /**
   * Creates a new setlist
   * @param request Data of the setlist
   */
  public createSetlist(request: CreateSetlistRequestDto) : Observable<CreateSetlistResponseDto> {
    return this.setlistsApiClient.createSetlist(request);
  }

  /**
   * Updates a setlist
   * @param setlistId ID of the setlist
   * @param request Data of the setlist
   */
  public updateSetlistHeader(setlistId: number, request: UpdateSetlistHeaderRequestDto) : Observable<any> {
    return this.setlistsApiClient.updateSetlistHeader(setlistId, request);
  }

  /**
   * Updates a setlist entry
   * @param setlistId ID of the setlist
   * @param entryId ID of the entry
   * @param formContent new Data of the setlist entry
   */
  public updateSetlistEntry(formContent: AddSetlistEntryFormContent, setlistId: number, entryId: string) : Observable<any> {
    let request: UpdateSetlistEntryRequestDto = {
      entryParameters: this.getEntryParametersFromFormContent(formContent),
      actParameters: this.getActParametersFromFormContent(formContent, setlistId),
      songParameters: this.getSongParametersFromFormContent(formContent),
      songVariantParameters: this.getSongVariantParametersFromFormContent(formContent),
    };
    return this.setlistsApiClient.updateSetlistEntry(setlistId, entryId, request);
  }

  /**
   * Deletes a setlists by its ID
   * @param setlistId ID of the setlist to delete
   */
  public deleteSetlist(setlistId: number) : Observable<any> {
    return this.setlistsApiClient.deleteSetlist(setlistId);
  }

  /**
   * Deletes a setlist entry by its ID
   * @param setlistId ID of the setlist
   * @param entryId ID of the entry in the setlist
   */
  public deleteSetlistEntry(setlistId: number, entryId: string) : Observable<any> {
    return this.setlistsApiClient.deleteSetlistEntry(setlistId, entryId);
  }


  private getEntryParametersFromFormContent(content: AddSetlistEntryFormContent): SetlistEntryParametersDto {
    return {
      songNumber: content.songNumber,
      sortNumber: content.sortNumber ?? content.songNumber,
      titleOverride: content.titleOverride,
      extraNotes: content.extraNotes,
      isPlayedFromRecording: content.wasPlayedFromRecording,
      isRotationSong: content.wasRotationSong,
      isWorldPremiere: content.wasWorldPremiere
    };
  }

  private getActParametersFromFormContent(content: AddSetlistEntryFormContent, setlistId: number): ActParametersDto | undefined {
    let usedActNumber = Number(content.selectedActNumber ?? 0) >= 0 ? Number(content.selectedActNumber) : Number(content.actNumber);

    if (usedActNumber == 0) {
      return undefined;
    }

    return {
      setlistId: Number(setlistId),
      actNumber: usedActNumber ?? 0,
      title: content.actTitle ?? null,
    };
  }

  private getSongParametersFromFormContent(content: AddSetlistEntryFormContent): SongParametersDto | undefined {
    if (content.selectedSongId == null) {
      return undefined;
    }

    let songId = content.selectedSongId == -1 ? 0 : Number(content.selectedSongId);
    return {
      songId: songId,
      songTitle: content.songTitle,
      isrc: content.songIsrc
    };
  }

  private getSongVariantParametersFromFormContent(content: AddSetlistEntryFormContent): SongVariantParametersDto | undefined {
    if (content.selectedSongVariantId == null) {
      return undefined;
    }

    let songId = content.selectedSongId == -1 ? 0 : Number(content.selectedSongId);
    let songVariantId = content.selectedSongVariantId == -1 ? 0 : Number(content.selectedSongVariantId);
    return {
      songId: songId,
      songVariantId: songVariantId,
      variantName: content.songVariantName,
      description: content.songVariantDescription,
    };
  }

  private getSongMashupParametersFromFormContent(content: AddSetlistEntryFormContent): SongMashupParametersDto | undefined {
    if (content.selectedSongMashupId == null) {
      return undefined;
    }

    let mashupId = content.selectedSongMashupId == -1 ? 0 : Number(content.selectedSongMashupId);
    return {
      songMashupId: mashupId
    };
  }

  public addSetlistEntry(content: AddSetlistEntryFormContent, setlistId: number): Observable<any> {
    let entryType = content.entryType;
    console.debug("Entry type: ", entryType);

    let entryParameters: SetlistEntryParametersDto = this.getEntryParametersFromFormContent(content);
    let actParameters: ActParametersDto | undefined = this.getActParametersFromFormContent(content, setlistId);

    if (entryType == AddSetlistEntryFormContent.entryTypeSong) {
      let addSongRequest: AddSongToSetlistRequestDto = {
        entryParameters: entryParameters,
        songParameters: this.getSongParametersFromFormContent(content),
        actParameters: actParameters
      };

      return this.setlistsApiClient.addSongToSetlist(setlistId, addSongRequest);
    } else if (entryType == AddSetlistEntryFormContent.entryTypeSongVariant) {
      let addSongVariantRequest: AddSongVariantToSetlistRequestDto = {
        entryParameters: entryParameters,
        songVariantParameters: this.getSongVariantParametersFromFormContent(content),
        actParameters: actParameters
      };

      return this.setlistsApiClient.addSongVariantToSetlist(setlistId, addSongVariantRequest);
    } else if (entryType == AddSetlistEntryFormContent.entryTypeSongMashup) {
      let addSongMashupRequest: AddSongMashupToSetlistRequestDto = {
        entryParameters: entryParameters,
        songMashupParameters: this.getSongMashupParametersFromFormContent(content),
        actParameters: actParameters
      };
      console.debug("addSongMashupRequest: ", addSongMashupRequest);

      return this.setlistsApiClient.addSongMashupToSetlist(setlistId, addSongMashupRequest);
    } else {
      throw new Error(`The entry type '${entryType}' is not implemented`);
    }
  }

  /**
   * Returns a setlist by its ID
   * @param setlistId ID of the setlist
   * @param cached true if the setlist can be read from cache
   */
  public getSetlist(setlistId: number, cached: boolean = true) : Observable<SetlistDto> {
    if (cached) {
      return this.setlistsApiClient.getCompleteSetlist(setlistId);
    } else {
      return this.setlistsApiClient.getCompleteSetlist(setlistId, Guid.create().toString(), "body", false);
    }
  }


  /**
   * Returns a setlist entry by its ID
   * @param setlistId ID of the setlist
   * @param entryId ID of the entry
   */
  public getSetlistEntry(setlistId: number, entryId: string) : Observable<RawSetlistEntryDto> {
    return this.setlistsApiClient.getSetlistEntry(setlistId, entryId);
  }

  /**
   * Returns all setlists for a given concert
   * @param concertId ID of the concert
   */
  public getSetlistsForConcert(concertId: string) : Observable<SetlistDto[]> {
    return this.concertsApiClient.getSetlistsForConcert(concertId);
  }

  /**
   * Returns all setlists
   */
  public getSetlists(cached: boolean) : Observable<SetlistDto[]> {
    if (cached) {
      return this.setlistsApiClient.getSetlists();
    } else {
      return this.setlistsApiClient.getSetlists(Guid.create().toString(), "body", false);
    }
  }


  /**
   * Returns all acts in a setlist
   */
  public getSetlistActs(setlistId: number, cached: boolean) : Observable<SetlistActDto[]> {
    if (cached) {
      return this.setlistsApiClient.getActsWithinSetlist(setlistId);
    } else {
      return this.setlistsApiClient.getActsWithinSetlist(setlistId, Guid.create().toString(), "body", false);
    }
  }


  /**
   * Applies a new order for the entries of a setlist
   * @param setlistId ID of the setlist
   * @param orderedEntryIds IDs of all entries in the new order
   */
  public applyNewEntryOrder(setlistId: number, orderedEntryIds: string[]) : Observable<SetlistEntryDto[]> {
    return this.setlistsApiClient.reorderSetlistEntries(setlistId, { entryIds: orderedEntryIds }).pipe(
      map(response => response.reorderedEntries ?? [])
    );
  }
}
