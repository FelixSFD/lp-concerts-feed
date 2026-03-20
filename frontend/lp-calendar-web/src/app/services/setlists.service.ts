import {inject, Injectable} from '@angular/core';
import {AuthService} from '../auth/auth.service';
import {HttpClient} from '@angular/common/http';
import {
  CreateSetlistRequestDto,
  CreateSetlistResponseDto, SetlistDto,
  SetlistsService as SetlistsApiClient,
  ConcertsService as ConcertsApiClient, UpdateSetlistHeaderRequestDto, AddSongToSetlistRequestDto,
  SetlistEntryParametersDto, AddSongVariantToSetlistRequestDto, SetlistEntryDto, ActParametersDto, RawSetlistEntryDto,
} from '../modules/lpshows-api';
import {map, Observable} from 'rxjs';
import {Guid} from 'guid-typescript';
import {AddSetlistEntryFormContent} from '../admin/setlists/add-setlist-entry-form/add-setlist-entry-form.component';
import {SetlistEntry} from '../data/setlists/setlist-entry';

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
   * Updates a new setlist
   * @param setlistId ID of the setlist
   * @param request Data of the setlist
   */
  public updateSetlistHeader(setlistId: number, request: UpdateSetlistHeaderRequestDto) : Observable<any> {
    return this.setlistsApiClient.updateSetlistHeader(setlistId, request);
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

  public addSetlistEntry(content: AddSetlistEntryFormContent, setlistId: number): Observable<any> {
    let entryType = content.entryType;
    console.debug("Entry type: ", entryType);

    let entryParameters: SetlistEntryParametersDto = {
      songNumber: content.songNumber,
      sortNumber: content.sortNumber ?? content.songNumber,
      titleOverride: content.titleOverride,
      extraNotes: content.extraNotes,
      isPlayedFromRecording: content.wasPlayedFromRecording,
      isRotationSong: content.wasRotationSong,
      isWorldPremiere: content.wasWorldPremiere
    };

    let actParameters: ActParametersDto = {
      setlistId: Number(setlistId),
      actNumber: content.actNumber ?? 0,
      title: content.actTitle ?? null,
    };

    let songId = content.selectedSongId == -1 ? 0 : Number(content.selectedSongId);

    if (entryType == AddSetlistEntryFormContent.entryTypeSong) {
      let addSongRequest: AddSongToSetlistRequestDto = {
        entryParameters: entryParameters,
        songParameters: {
          songId: songId,
          songTitle: content.songTitle,
          isrc: content.songIsrc
        },
        actParameters: actParameters
      };

      return this.setlistsApiClient.addSongToSetlist(setlistId, addSongRequest);
    } else if (entryType == AddSetlistEntryFormContent.entryTypeSongVariant) {
      let songVariantId = content.selectedSongVariantId == -1 ? 0 : Number(content.selectedSongVariantId);
      let addSongVariantRequest: AddSongVariantToSetlistRequestDto = {
        entryParameters: entryParameters,
        songVariantParameters: {
          songId: songId,
          songVariantId: songVariantId,
          variantName: content.songVariantName,
          description: content.songVariantDescription,
        },
        actParameters: actParameters
      };

      return this.setlistsApiClient.addSongVariantToSetlist(setlistId, addSongVariantRequest);
    } else {
      throw new Error(`The entry type '${entryType}' is not implemented`);
    }
  }

  /**
   * Returns a setlist by its ID
   * @param setlistId ID of the setlist
   */
  public getSetlist(setlistId: number) : Observable<SetlistDto> {
    return this.setlistsApiClient.getCompleteSetlist(setlistId);
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
