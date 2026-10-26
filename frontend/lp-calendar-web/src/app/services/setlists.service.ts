import { inject, Injectable } from '@angular/core';
import {
  AddCustomEntryToSetlistRequestDto,
  AddSongExtraToSetlistEntryRequestDto,
  AddSongMashupToSetlistRequestDto,
  AddSongToSetlistRequestDto,
  AddSongVariantToSetlistRequestDto,
  CreateSetlistRequestDto,
  CreateSetlistResponseDto,
  ImportSetlistPreviewDto,
  ImportSetlistRequestDto,
  ReorderSetlistEntriesRequestDto,
  ReorderSetlistEntriesResponseDto,
  RawSetlistEntryDto,
  SetlistActDto,
  SetlistDto,
  SetlistHeaderDto,
  UpdateSetlistEntryRequestDto,
  UpdateSetlistHeaderRequestDto, SetlistsApi, AddSongToSetlistResponseDto, AddSongVariantToSetlistResponseDto,
  AddSongMashupToSetlistResponseDto, AddSongExtraToSetlistEntryResponseDto, ActParametersDto, SetlistEntryParametersDto,
  SongMashupParametersDto, SongVariantParametersDto, SongParametersDto, SetlistEntryDto,
} from '../modules/lpshows-api/v3';
import { firstValueFrom, map, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { getRequestIdParameter } from '../helper/cache-parameter-helper';
import {
  AddSetlistEntryFormContent
} from '../components/v2/admin/setlists/add-setlist-entry-form/add-setlist-entry-form.component';
import { addAuthentication } from '../auth/auth.config';

/**
 * Service for interacting with setlists
 */
@Injectable({
  providedIn: 'root',
})
export class SetlistsService {
  private readonly setlistsApi = inject(SetlistsApi);

  constructor() {
    this.setlistsApi.configuration.basePath = environment.apiBaseUrl;
    addAuthentication(this.setlistsApi);
  }

  /**
   * Adds a custom entry to a setlist
   * @param setlistId The ID of the setlist (number, not string)
   * @param addCustomEntryToSetlistRequestDto The request data
   */
  addCustomEntryToSetlist(
    setlistId: number,
    addCustomEntryToSetlistRequestDto: AddCustomEntryToSetlistRequestDto
  ): Promise<any> {
    return firstValueFrom(this.setlistsApi.addCustomEntryToSetlist(setlistId, addCustomEntryToSetlistRequestDto));
  }

  /**
   * Adds an extra to a song in a setlist entry
   * @param setlistId The ID of the setlist (number)
   * @param entryId The ID of the setlist entry (string)
   * @param addSongExtraToSetlistEntryRequestDto The request data
   */
  addSongExtraToSetlistEntry(
    setlistId: number,
    entryId: string,
    addSongExtraToSetlistEntryRequestDto: AddSongExtraToSetlistEntryRequestDto
  ): Promise<AddSongExtraToSetlistEntryResponseDto> {
    return firstValueFrom(this.setlistsApi.addSongExtraToSetlistEntry(setlistId, entryId, addSongExtraToSetlistEntryRequestDto));
  }

  /**
   * Adds a song mashup to a setlist
   * @param setlistId The ID of the setlist (number)
   * @param addSongMashupToSetlistRequestDto The request data
   */
  addSongMashupToSetlist(
    setlistId: number,
    addSongMashupToSetlistRequestDto: AddSongMashupToSetlistRequestDto
  ): Promise<AddSongMashupToSetlistResponseDto> {
    return firstValueFrom(this.setlistsApi.addSongMashupToSetlist(setlistId, addSongMashupToSetlistRequestDto));
  }

  /**
   * Adds a song to a setlist
   * @param setlistId The ID of the setlist (number)
   * @param addSongToSetlistRequestDto The request data
   */
  addSongToSetlist(
    setlistId: number,
    addSongToSetlistRequestDto: AddSongToSetlistRequestDto
  ): Promise<AddSongToSetlistResponseDto> {
    return firstValueFrom(this.setlistsApi.addSongToSetlist(setlistId, addSongToSetlistRequestDto));
  }

  /**
   * Adds a song variant to a setlist
   * @param setlistId The ID of the setlist (number)
   * @param addSongVariantToSetlistRequestDto The request data
   */
  addSongVariantToSetlist(
    setlistId: number,
    addSongVariantToSetlistRequestDto: AddSongVariantToSetlistRequestDto
  ): Promise<AddSongVariantToSetlistResponseDto> {
    return firstValueFrom(this.setlistsApi.addSongVariantToSetlist(setlistId, addSongVariantToSetlistRequestDto));
  }

  /**
   * Creates a new setlist
   * @param createSetlistRequestDto The request data
   */
  createSetlist(createSetlistRequestDto: CreateSetlistRequestDto): Promise<CreateSetlistResponseDto> {
    return firstValueFrom(this.setlistsApi.createSetlist(createSetlistRequestDto));
  }

  /**
   * Deletes a setlist
   * @param setlistId The ID of the setlist to delete
   */
  deleteSetlist(setlistId: number): Promise<any> {
    return firstValueFrom(this.setlistsApi.deleteSetlist(setlistId));
  }

  /**
   * Deletes an entry from a setlist
   * @param setlistId The ID of the setlist
   * @param entryId The ID of the entry to delete
   */
  deleteSetlistEntry(setlistId: number, entryId: string): Promise<any> {
    return firstValueFrom(this.setlistsApi.deleteSetlistEntry(setlistId, entryId));
  }

  /**
   * Removes an extra from a song in a setlist entry
   * @param setlistId The ID of the setlist
   * @param entryId The ID of the setlist entry
   * @param songExtraId The ID of the extra to remove
   */
  deleteSongExtraFromSetlistEntry(
    setlistId: number,
    entryId: string,
    songExtraId: string
  ): Promise<any> {
    return firstValueFrom(this.setlistsApi.deleteSongExtraFromSetlistEntry(setlistId, entryId, songExtraId));
  }

  /**
   * Returns the acts of a setlist
   * @param setlistId The ID of the setlist (number)
   */
  getActsInSetlist(setlistId: number): Promise<SetlistActDto[]> {
    return firstValueFrom(this.setlistsApi.getActsInSetlist(setlistId));
  }

  /**
   * Returns a complete setlist with all entries and acts
   * @param setlistId The ID of the setlist (number)
   * @param cached true if cached values are allowed
   */
  getCompleteSetlist(setlistId: number, cached: boolean = true): Promise<SetlistDto> {
    return firstValueFrom(this.setlistsApi.getCompleteSetlist(setlistId, getRequestIdParameter(cached)));
  }

  /**
   * Returns a specific entry in a setlist
   * @param setlistId The ID of the setlist (number)
   * @param entryId The ID of the entry
   */
  getSetlistEntry(setlistId: number, entryId: string): Promise<RawSetlistEntryDto> {
    return firstValueFrom(this.setlistsApi.getSetlistEntry(setlistId, entryId));
  }

  /**
   * Returns headers of all setlists
   */
  getSetlistHeaders(): Promise<SetlistHeaderDto[]> {
    return firstValueFrom(this.setlistsApi.getSetlistHeaders());
  }

  /**
   * Returns all setlists for a concert
   * @param concertId The ID of the concert
   */
  getSetlistsForConcert(concertId: string): Promise<SetlistDto[]> {
    return firstValueFrom(this.setlistsApi.getSetlistsForConcert(concertId));
  }

  /**
   * Imports a setlist from Linkinpedia for a concert
   * @param concertId The ID of the concert
   * @param importSetlistRequestDto The request data
   */
  importSetlistFromLinkinpedia(
    concertId: string,
    importSetlistRequestDto: ImportSetlistRequestDto
  ): Promise<ImportSetlistPreviewDto> {
    return firstValueFrom(this.setlistsApi.importSetlistFromLinkinpedia(concertId, importSetlistRequestDto));
  }

  /**
   * Reorders the entries in a setlist
   * @param setlistId The ID of the setlist (number)
   * @param reorderSetlistEntriesRequestDto The request data
   */
  reorderSetlistEntries(
    setlistId: number,
    reorderSetlistEntriesRequestDto: ReorderSetlistEntriesRequestDto
  ): Promise<ReorderSetlistEntriesResponseDto> {
    return firstValueFrom(this.setlistsApi.reorderSetlistEntries(setlistId, reorderSetlistEntriesRequestDto));
  }

  /**
   * Updates the header information of a setlist
   * @param setlistId The ID of the setlist (number)
   * @param updateSetlistHeaderRequestDto The request data
   */
  updateSetlistHeader(
    setlistId: number,
    updateSetlistHeaderRequestDto: UpdateSetlistHeaderRequestDto
  ): Promise<any> {
    return firstValueFrom(this.setlistsApi.updateSetlistHeader(setlistId, updateSetlistHeaderRequestDto));
  }

  /**
   * Applies a new order for the entries of a setlist
   * @param setlistId ID of the setlist
   * @param orderedEntryIds IDs of all entries in the new order
   */
  public applyNewEntryOrder(setlistId: number, orderedEntryIds: string[]) : Promise<SetlistEntryDto[]> {
    return firstValueFrom(this.setlistsApi.reorderSetlistEntries(setlistId, { entryIds: orderedEntryIds }).pipe(
      map(response => response.reorderedEntries ?? [])
    ));
  }

  /**
   * Updates a setlist entry
   * @param setlistId ID of the setlist
   * @param entryId ID of the entry
   * @param formContent new Data of the setlist entry
   */
  public updateSetlistEntry(formContent: AddSetlistEntryFormContent, setlistId: number, entryId: string) : Promise<any> {
    let request: UpdateSetlistEntryRequestDto = {
      entryParameters: this.getEntryParametersFromFormContent(formContent),
      actParameters: this.getActParametersFromFormContent(formContent, setlistId),
      songParameters: this.getSongParametersFromFormContent(formContent),
      songVariantParameters: this.getSongVariantParametersFromFormContent(formContent),
      songMashupParameters: this.getSongMashupParametersFromFormContent(formContent),
    };

    return firstValueFrom(this.setlistsApi.updateSetlistEntry(setlistId, entryId, request));
  }

  private getEntryParametersFromFormContent(content: AddSetlistEntryFormContent): SetlistEntryParametersDto {
    return {
      songNumber: content.songNumber,
      sortNumber: content.sortNumber ?? ((content.songNumber ?? 1) * 10),
      titleOverride: content.titleOverride,
      extraNotes: content.extraNotes,
      isPlayedFromRecording: content.wasPlayedFromRecording,
      isRotationSong: content.wasRotationSong,
      isWorldPremiere: content.wasWorldPremiere,
      isLivePremiere: content.wasLivePremiere
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
      id: songId,
      title: content.songTitle ?? undefined,
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
      id: songVariantId ?? undefined,
      variantName: content.songVariantName ?? undefined,
      description: content.songVariantDescription,
    };
  }

  private getSongMashupParametersFromFormContent(content: AddSetlistEntryFormContent): SongMashupParametersDto | undefined {
    if (content.selectedSongMashupId == null) {
      return undefined;
    }

    let mashupId = content.selectedSongMashupId == -1 ? 0 : Number(content.selectedSongMashupId);
    return {
      id: mashupId
    };
  }

  public async addSetlistEntry(content: AddSetlistEntryFormContent, setlistId: number): Promise<any> {
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

      return this.setlistsApi.addSongToSetlist(setlistId, addSongRequest);
    } else if (entryType == AddSetlistEntryFormContent.entryTypeSongVariant) {
      let addSongVariantRequest: AddSongVariantToSetlistRequestDto = {
        entryParameters: entryParameters,
        songVariantParameters: this.getSongVariantParametersFromFormContent(content),
        actParameters: actParameters
      };

      return this.setlistsApi.addSongVariantToSetlist(setlistId, addSongVariantRequest);
    } else if (entryType == AddSetlistEntryFormContent.entryTypeSongMashup) {
      let addSongMashupRequest: AddSongMashupToSetlistRequestDto = {
        entryParameters: entryParameters,
        songMashupParameters: this.getSongMashupParametersFromFormContent(content),
        actParameters: actParameters
      };
      console.debug("addSongMashupRequest: ", addSongMashupRequest);

      return this.setlistsApi.addSongMashupToSetlist(setlistId, addSongMashupRequest);
    } else if (entryType == AddSetlistEntryFormContent.entryTypeFreeText) {
      let addEntryRequest: AddCustomEntryToSetlistRequestDto = {
        entryParameters: entryParameters,
        actParameters: actParameters
      };
      return this.setlistsApi.addCustomEntryToSetlist(setlistId, addEntryRequest);
    } else {
      return throwError(() => ({
        error: { message: `The entry type '${entryType}' is not implemented` }
      }));
    }
  }
}
