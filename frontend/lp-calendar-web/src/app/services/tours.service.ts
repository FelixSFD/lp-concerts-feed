import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { addAuthentication } from '../auth/auth.config';
import { AddTourLegRequestDto, CreateTourRequestDto, TourDto, TourLegDto, ToursApi } from '../modules/lpshows-api/v3';

/**
 * Service to manage tours and tour legs
 */
@Injectable({
  providedIn: 'root'
})
export class ToursService {
  constructor(private toursApi: ToursApi) {
    toursApi.configuration.basePath = environment.apiBaseUrl;
    addAuthentication(toursApi);
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
}
