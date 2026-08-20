import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { addAuthentication } from '../auth/auth.config';
import {
  AddTourLegRequestDto, ConcertDetailsDto,
  ConcertsApi,
  CreateTourRequestDto, RawConcertDto,
  TourDto,
  TourLegDto,
  ToursApi
} from '../modules/lpshows-api/v3';
import { ConcertFilter } from '../data/concert-filter';
import { ConcertDto } from '../modules/lpshows-api';

/**
 * Service to manage tours and tour legs
 */
@Injectable({
  providedIn: 'root'
})
export class ToursService {
  constructor(private toursApi: ToursApi, private concertsApi: ConcertsApi) {
    toursApi.configuration.basePath = environment.apiBaseUrl;
    concertsApi.configuration.basePath = environment.apiBaseUrl;
    addAuthentication(toursApi);
    addAuthentication(concertsApi);
  }

  /**
   * Returns a list of all tours
   */
  getTours(): Observable<TourDto[]> {
    return this.toursApi.getTours();
  }

  /**
   * Returns information about a single tour
   * @param tourId ID of the tour
   */
  getTour(tourId: string): Observable<TourDto> {
    return this.toursApi.getTour(tourId);
  }

  /**
   * Creates a new tour
   * @param tour The tour creation request data
   */
  createTour(tour: CreateTourRequestDto): Observable<TourDto> {
    return this.toursApi.createTour(tour);
  }

  /**
   * Deletes a tour
   * @param tourId ID of the tour
   */
  deleteTour(tourId: string): Observable<any> {
    return this.toursApi.deleteTour(tourId);
  }

  /**
   * Returns information about a tour leg
   * @param tourId ID of the tour
   * @param legId ID of the tour leg
   */
  getTourLeg(tourId: string, legId: string): Observable<TourLegDto> {
    return this.toursApi.getTourLeg(tourId, legId);
  }

  /**
   * Adds a new leg to a tour
   * @param tourId ID of the tour
   * @param leg The tour leg creation request data
   */
  createTourLeg(tourId: string, leg: AddTourLegRequestDto): Observable<TourLegDto> {
    return this.toursApi.createTourLeg(tourId, leg);
  }

  /**
   * Deletes a leg from a tour
   * @param tourId ID of the tour
   * @param legId ID of the tour leg
   */
  deleteTourLeg(tourId: string, legId: string): Observable<any> {
    return this.toursApi.deleteTourLeg(tourId, legId);
  }

  getFilteredConcerts(filter: ConcertFilter, cached: boolean = true): Observable<ConcertDetailsDto[]> {
    // TODO: implement cache parameter
    return this.concertsApi.getConcerts();
  }

  getConcertById(concertId: string, cached: boolean = true): Observable<ConcertDetailsDto> {
    // TODO: implement cache parameter
    return this.concertsApi.getConcertById(concertId);
  }
}
