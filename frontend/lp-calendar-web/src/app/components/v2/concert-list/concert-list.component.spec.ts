import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter, Router } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { ConcertListComponent } from './concert-list.component';
import { ConcertsService } from '../../../services/concerts.service';
import { ToursService } from '../../../services/tours.service';
import { AuthService } from '../../../auth/auth.service';
import { ConcertDetailsDto, ConcertListResponseDto, ConcertStatusValueDto } from '../../../modules/lpshows-api/v3';

describe('ConcertListComponent', () => {
  let component: ConcertListComponent;
  let fixture: ComponentFixture<ConcertListComponent>;
  let mockConcertsService: jasmine.SpyObj<ConcertsService>;
  let mockToursService: jasmine.SpyObj<ToursService>;
  let mockAuthService: Partial<AuthService>;

  const sampleConcert: ConcertDetailsDto = {
    id: 'concert-1',
    status: ConcertStatusValueDto.Planned,
    postedStartTime: '2026-11-01T19:00:00Z',
    mainStageTime: '2026-11-01T21:00:00Z',
    venue: {
      id: 1,
      currentName: 'Barclays Arena',
      countryCode: 'DE',
      venueNames: [],
      timeZoneId: 'Europe/Berlin',
      city: {
        id: 1,
        name: 'Hamburg',
        countryCode: 'DE',
        nativeName: 'Hamburg',
        country: {
          name: 'Germany',
          nativeName: 'Deutschland',
          isoCode: 'DE',
        },
      },
    },
    tour: {
      id: '1',
      name: 'From Zero World Tour',
    },
    concertType: {
      id: 1,
      name: 'Headline',
    },
    lpuEarlyEntryConfirmed: true,
  };

  const sampleResponse: ConcertListResponseDto = {
    concerts: [sampleConcert],
    metadata: {
      offset: 0,
      totalElements: 1,
    },
  };

  beforeEach(async () => {
    mockConcertsService = jasmine.createSpyObj('ConcertsService', ['getFilteredConcerts']);
    mockConcertsService.getFilteredConcerts.and.returnValue(Promise.resolve(sampleResponse));

    mockToursService = jasmine.createSpyObj('ToursService', ['deleteConcert']);
    mockToursService.deleteConcert.and.returnValue(Promise.resolve({}));

    mockAuthService = {
      canAddConcerts: of(true),
      canUpdateConcerts: of(true),
      canDeleteConcerts: of(true),
      canManageSetlists: of(true),
    };

    await TestBed.configureTestingModule({
      imports: [ConcertListComponent],
      providers: [
        ConfirmationService,
        MessageService,
        provideRouter([]),
        { provide: ConcertsService, useValue: mockConcertsService },
        { provide: ToursService, useValue: mockToursService },
        { provide: AuthService, useValue: mockAuthService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ConcertListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load initial concerts on init', async () => {
    await component.reloadConcertList(true);
    expect(mockConcertsService.getFilteredConcerts).toHaveBeenCalled();
    expect(component.concerts$().length).toBe(1);
    expect(component.concerts$()[0]?.id).toBe('concert-1');
    expect(component.hasMore$()).toBeFalse();
  });

  it('should handle loadMore when more items are available', async () => {
    const multiPageResponse: ConcertListResponseDto = {
      concerts: [sampleConcert],
      metadata: {
        offset: 0,
        totalElements: 2,
      },
    };
    mockConcertsService.getFilteredConcerts.and.returnValue(Promise.resolve(multiPageResponse));
    await component.reloadConcertList(true);
    expect(component.hasMore$()).toBeTrue();

    const secondConcert: ConcertDetailsDto = {
      ...sampleConcert,
      id: 'concert-2',
    };
    mockConcertsService.getFilteredConcerts.and.returnValue(Promise.resolve({
      concerts: [secondConcert],
      metadata: {
        offset: 1,
        totalElements: 2,
      },
    }));

    await component.loadMore();
    expect(component.concerts$().length).toBe(2);
    expect(component.concerts$()[1]?.id).toBe('concert-2');
    expect(component.hasMore$()).toBeFalse();
  });

  it('should format badges data correctly', () => {
    const badges = component.getBadgesData(sampleConcert);
    expect(badges.concertStatus).toBe(ConcertStatusValueDto.Planned);
    expect(badges.showType).toBe('Headline');
    expect(badges.lpuEarlyEntryConfirmed).toBeTrue();
  });

  it('should toggle date sort order between desc and asc, reload list, and update URL queryParams', async () => {
    const router = TestBed.inject(Router);
    spyOn(router, 'navigate').and.returnValue(Promise.resolve(true));

    mockConcertsService.getFilteredConcerts.calls.reset();
    expect(component.sortOrder$()).toBe('desc');

    component.toggleDateSort();

    expect(component.sortOrder$()).toBe('asc');
    expect(component.currentFilter.orderBy).toEqual(['date']);
    expect(mockConcertsService.getFilteredConcerts).toHaveBeenCalledWith(
      jasmine.objectContaining({
        orderBy: ['date'],
      }),
      20,
      0,
      true
    );
    expect(router.navigate).toHaveBeenCalledWith([], {
      relativeTo: jasmine.anything(),
      queryParams: { sortOrder: 'asc' },
      queryParamsHandling: 'merge',
    });

    mockConcertsService.getFilteredConcerts.calls.reset();
    component.toggleDateSort();

    expect(component.sortOrder$()).toBe('desc');
    expect(component.currentFilter.orderBy).toEqual(['-date']);
    expect(mockConcertsService.getFilteredConcerts).toHaveBeenCalledWith(
      jasmine.objectContaining({
        orderBy: ['-date'],
      }),
      20,
      0,
      true
    );
    expect(router.navigate).toHaveBeenCalledWith([], {
      relativeTo: jasmine.anything(),
      queryParams: { sortOrder: 'desc' },
      queryParamsHandling: 'merge',
    });
  });

  it('should initialize sortOrder from activatedRoute queryParams in ngOnInit', () => {
    const route = TestBed.inject(ActivatedRoute);
    (route.snapshot as any).queryParams = { sortOrder: 'asc' };

    component.ngOnInit();

    expect(component.sortOrder$()).toBe('asc');
    expect(component.currentFilter.orderBy).toEqual(['date']);
    expect(component.defaultFilter.orderBy).toEqual(['date']);
  });

  it('should retain current date sort order when onFilterChanged is called', async () => {
    component.sortOrder$.set('asc');
    mockConcertsService.getFilteredConcerts.calls.reset();

    component.onFilterChanged({
      onlyFuture: false,
      tour: null,
      dateFrom: null,
      dateTo: null,
    });

    expect(component.currentFilter.orderBy).toEqual(['date']);
    expect(mockConcertsService.getFilteredConcerts).toHaveBeenCalledWith(
      jasmine.objectContaining({
        orderBy: ['date'],
      }),
      20,
      0,
      true
    );
  });
});
