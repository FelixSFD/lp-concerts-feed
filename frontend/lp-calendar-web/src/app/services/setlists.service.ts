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
  AddSongMashupToSetlistResponseDto, AddSongExtraToSetlistEntryResponseDto,
} from '../modules/lpshows-api/v3';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';

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
   */
  getCompleteSetlist(setlistId: number): Promise<SetlistDto> {
    return firstValueFrom(this.setlistsApi.getCompleteSetlist(setlistId));
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
   * Updates a setlist entry
   * @param setlistId The ID of the setlist (number)
   * @param entryId The ID of the entry
   * @param updateSetlistEntryRequestDto The request data
   */
  updateSetlistEntry(
    setlistId: number,
    entryId: string,
    updateSetlistEntryRequestDto: UpdateSetlistEntryRequestDto
  ): Promise<any> {
    return firstValueFrom(this.setlistsApi.updateSetlistEntry(setlistId, entryId, updateSetlistEntryRequestDto));
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
}
