import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TourMapPageComponent } from './tour-map-page.component';
import { MessageService } from 'primeng/api';
import { ConcertsService } from '../../../services/concerts.service';
import { ConcertListResponseDto } from '../../../modules/lpshows-api/v3';

describe('TourMapPageComponent', () => {
  let component: TourMapPageComponent;
  let fixture: ComponentFixture<TourMapPageComponent>;
  let concertsServiceMock: jasmine.SpyObj<ConcertsService>;

  beforeEach(async () => {
    concertsServiceMock = jasmine.createSpyObj('ConcertsService', ['getFilteredConcerts']);
    concertsServiceMock.getFilteredConcerts.and.resolveTo({
      metadata: { offset: 0, totalElements: 0 },
      concerts: []
    });

    await TestBed.configureTestingModule({
      imports: [TourMapPageComponent],
      providers: [
        { provide: MessageService, useValue: jasmine.createSpyObj('MessageService', ['add']) },
        { provide: ConcertsService, useValue: concertsServiceMock },
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TourMapPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
    expect(component.downloadStatus()).toBe(0);
  });

  it('should paginate through all concerts and update downloadStatus', async () => {
    const page1: ConcertListResponseDto = {
      metadata: { offset: 0, totalElements: 150 },
      concerts: Array(100).fill({
        venue: { latitude: 10, longitude: 20, city: { name: 'City', country: { name: 'Country' } } },
        concertType: { id: 1 }
      }) as any
    };
    const page2: ConcertListResponseDto = {
      metadata: { offset: 100, totalElements: 150 },
      concerts: Array(50).fill({
        venue: { latitude: 10, longitude: 20, city: { name: 'City', country: { name: 'Country' } } },
        concertType: { id: 1 }
      }) as any
    };

    concertsServiceMock.getFilteredConcerts.and.returnValues(
      Promise.resolve(page1),
      Promise.resolve(page2)
    );

    await component['reloadPins']();

    expect(concertsServiceMock.getFilteredConcerts).toHaveBeenCalledTimes(2);
    expect(concertsServiceMock.getFilteredConcerts).toHaveBeenCalledWith(component.currentFilter, 100, 0);
    expect(concertsServiceMock.getFilteredConcerts).toHaveBeenCalledWith(component.currentFilter, 100, 100);
    expect(component.downloadStatus()).toBe(1);
  });
});
