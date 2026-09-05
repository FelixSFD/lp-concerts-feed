import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { ConcertFormComponent } from './concert-form.component';
import { ConcertTypesService } from '../../../../../services/concert-types.service';
import { ToursService } from '../../../../../services/tours.service';
import { LocationsService } from '../../../../../services/locations.service';
import { ConcertDetailsDto, VenueDto } from '../../../../../modules/lpshows-api/v3';
import { ConcertStatusValueDto } from '../../../../../modules/lpshows-api';

describe('ConcertFormComponent', () => {
  let component: ConcertFormComponent;
  let fixture: ComponentFixture<ConcertFormComponent>;
  let concertTypesService: jasmine.SpyObj<ConcertTypesService>;
  let toursService: jasmine.SpyObj<ToursService>;
  let locationsService: jasmine.SpyObj<LocationsService>;

  const mockConcertDetails: ConcertDetailsDto = {
    id: 'concert-123',
    customTitle: 'Special Show in Berlin',
    concertType: {
      id: 1,
      name: 'Headline Show',
    },
    tour: {
      id: 'tour-123',
      name: 'From Zero World Tour',
    },
    tourLeg: {
      id: 'leg-123',
      name: 'Europe Leg',
      tourId: 'tour-123',
    },
    venue: {
      id: '10',
      currentName: 'Uber Arena',
      countryCode: 'DE',
      cityId: '1',
      timeZoneId: 'Europe/Berlin',
      venueNames: [],
      city: {
        id: '1',
        name: 'Berlin',
        countryCode: 'DE',
        nativeName: 'Berlin',
        country: {
          isoCode: 'DE',
          name: 'Germany',
          nativeName: 'Deutschland',
        },
      },
    },
  };

  beforeEach(async () => {
    const concertTypesSpy = jasmine.createSpyObj('ConcertTypesService', ['getConcertTypes']);
    concertTypesSpy.getConcertTypes.and.returnValue(
      of([
        { id: 1, name: 'Headline Show' },
        { id: 2, name: 'Festival' },
      ])
    );

    const toursSpy = jasmine.createSpyObj('ToursService', ['getTours', 'getTour']);
    toursSpy.getTours.and.returnValue(
      of([
        { id: 'tour-123', name: 'From Zero World Tour' },
      ])
    );
    toursSpy.getTour.and.returnValue(
      of({
        id: 'tour-123',
        name: 'From Zero World Tour',
        legs: [
          { id: 'leg-123', name: 'Europe Leg', tourId: 'tour-123' },
        ],
      })
    );

    const locationsSpy = jasmine.createSpyObj('LocationsService', [
      'getCountries',
      'getVenues',
      'getCities',
      'getCitiesIn',
    ]);
    locationsSpy.getCountries.and.returnValue(
      of([{ isoCode: 'DE', name: 'Germany', nativeName: 'Deutschland' }])
    );
    locationsSpy.getVenues.and.returnValue(
      of([
        { id: '10', currentName: 'Uber Arena', countryCode: 'DE', cityId: '1', timeZoneId: 'Europe/Berlin' },
      ])
    );
    locationsSpy.getCities.and.returnValue(
      of([
        { id: '1', name: 'Berlin', countryCode: 'DE', nativeName: 'Berlin', country: { isoCode: 'DE', name: 'Germany', nativeName: 'Deutschland' } },
      ])
    );
    locationsSpy.getCitiesIn.and.returnValue(
      of([
        { id: '1', name: 'Berlin', countryCode: 'DE', nativeName: 'Berlin', country: { isoCode: 'DE', name: 'Germany', nativeName: 'Deutschland' } },
      ])
    );

    await TestBed.configureTestingModule({
      imports: [ConcertFormComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        MessageService,
        { provide: ConcertTypesService, useValue: concertTypesSpy },
        { provide: ToursService, useValue: toursSpy },
        { provide: LocationsService, useValue: locationsSpy },
      ],
    }).compileComponents();

    concertTypesService = TestBed.inject(ConcertTypesService) as jasmine.SpyObj<ConcertTypesService>;
    toursService = TestBed.inject(ToursService) as jasmine.SpyObj<ToursService>;
    locationsService = TestBed.inject(LocationsService) as jasmine.SpyObj<LocationsService>;
    fixture = TestBed.createComponent(ConcertFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with invalid status before required fields are selected', () => {
    expect(component.concertForm.valid).toBeFalse();
    expect(component.concertForm.controls.concertTypeId.value).toBeNull();
    expect(component.concertForm.controls.tour.value).toBeNull();
    expect(component.concertForm.controls.tourLegId.value).toBeNull();
    expect(component.concertForm.controls.venue.value).toBeNull();
  });

  it('should be valid when all required fields are selected', () => {
    component.concertForm.controls.concertStatus.setValue(ConcertStatusValueDto.Planned);
    component.concertForm.controls.concertTypeId.setValue(1);
    component.concertForm.controls.tour.setValue({ id: 'tour-123', name: 'From Zero World Tour' });
    component.concertForm.controls.tourLegId.setValue('leg-123');
    component.concertForm.controls.venue.setValue(mockConcertDetails.venue as VenueDto);
    component.concertForm.controls.postedStartTime.setValue(new Date());
    component.concertForm.controls.customTitle.setValue('Test Title');
    expect(component.concertForm.valid).toBeTrue();
  });

  it('should fill form with concert details', () => {
    component.fillFormWith(mockConcertDetails);

    expect(component.concertForm.controls.customTitle.value).toBe('Special Show in Berlin');
    expect(component.concertForm.controls.concertTypeId.value).toBe(1);
    expect(component.concertForm.controls.tour.value).toEqual(mockConcertDetails.tour!);
    expect(component.concertForm.controls.tourLegId.value).toBe('leg-123');
    expect(component.concertForm.controls.venue.value).toEqual(mockConcertDetails.venue as VenueDto);
  });

  it('should read from form properly', () => {
    component.fillFormWith(mockConcertDetails);
    component.concertForm.controls.postedStartTime.setValue(new Date('2026-06-15T20:00:00Z'));
    const result = component.readFromForm();

    expect(result).not.toBeNull();
    expect(result?.customTitle).toBe('Special Show in Berlin');
    expect(result?.concertTypeId).toBe(1);
    expect(result?.tourId).toBe('tour-123');
    expect(result?.tourLegId).toBe('leg-123');
    expect(result?.venueId).toBe('10');
    expect(result?.timezone).toBe('Europe/Berlin');
  });

  it('should reset form properly', () => {
    component.fillFormWith(mockConcertDetails);
    component.reset();

    expect(component.concertForm.controls.customTitle.value).toBe('');
    expect(component.concertForm.controls.concertTypeId.value).toBeNull();
    expect(component.concertForm.controls.tour.value).toBeNull();
    expect(component.concertForm.controls.tourLegId.value).toBeNull();
    expect(component.concertForm.controls.venue.value).toBeNull();
  });

  it('should emit saveClicked when onSaveClicked is triggered with valid form', () => {
    spyOn(component.saveClicked, 'emit');
    component.fillFormWith(mockConcertDetails);
    component.concertForm.controls.postedStartTime.setValue(new Date('2026-06-15T20:00:00Z'));

    component.onSaveClicked();

    expect(component.saveClicked.emit).toHaveBeenCalled();
    const emitted = (component.saveClicked.emit as jasmine.Spy).calls.mostRecent().args[0];
    expect(emitted.customTitle).toBe('Special Show in Berlin');
    expect(emitted.concertTypeId).toBe(1);
    expect(emitted.tourId).toBe('tour-123');
    expect(emitted.tourLegId).toBe('leg-123');
    expect(emitted.venueId).toBe('10');
  });
});
