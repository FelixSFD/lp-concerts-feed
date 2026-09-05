import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of } from 'rxjs';
import { SimpleChange } from '@angular/core';
import { SelectTourLegComponent } from './select-tour-leg.component';
import { ToursService } from '../../../../../services/tours.service';
import { TourDto } from '../../../../../modules/lpshows-api/v3';

describe('SelectTourLegComponent', () => {
  let component: SelectTourLegComponent;
  let fixture: ComponentFixture<SelectTourLegComponent>;
  let toursService: jasmine.SpyObj<ToursService>;

  const mockTour1: TourDto = {
    id: 'tour-1',
    name: 'From Zero World Tour',
    legs: [
      { id: 'leg-1', name: 'Europe Leg 1', tourId: 'tour-1' },
      { id: 'leg-2', name: 'North America Leg 1', tourId: 'tour-1' },
    ],
  };

  const mockTour2: TourDto = {
    id: 'tour-2',
    name: 'Hunting Party Tour',
    legs: [
      { id: 'leg-3', name: 'South America Leg 1', tourId: 'tour-2' },
    ],
  };

  beforeEach(async () => {
    const serviceSpy = jasmine.createSpyObj('ToursService', ['getTour']);
    serviceSpy.getTour.and.callFake((id: string) => {
      if (id === 'tour-1') {
        return of(mockTour1);
      }
      if (id === 'tour-2') {
        return of(mockTour2);
      }
      return of({ id, name: 'Unknown Tour', legs: [] });
    });

    await TestBed.configureTestingModule({
      imports: [SelectTourLegComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ToursService, useValue: serviceSpy },
      ],
    }).compileComponents();

    toursService = TestBed.inject(ToursService) as jasmine.SpyObj<ToursService>;
    fixture = TestBed.createComponent(SelectTourLegComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should not load legs when no tour is provided', () => {
    expect(toursService.getTour).not.toHaveBeenCalled();
    expect(component.tourLegs).toEqual([]);
  });

  it('should load tour legs when tour input is provided', () => {
    component.tour = mockTour1;
    component.ngOnChanges({
      tour: new SimpleChange(null, mockTour1, false),
    });

    expect(toursService.getTour).toHaveBeenCalledWith('tour-1');
    expect(component.tourLegs).toEqual(mockTour1.legs!);
  });

  it('should reload tour legs when tour changes and clear value if not in new legs', () => {
    component.tour = mockTour1;
    component.ngOnChanges({
      tour: new SimpleChange(null, mockTour1, false),
    });
    component.writeValue('leg-1');

    const onChangeSpy = jasmine.createSpy('onChange');
    component.registerOnChange(onChangeSpy);

    component.tour = mockTour2;
    component.ngOnChanges({
      tour: new SimpleChange(mockTour1, mockTour2, false),
    });

    expect(toursService.getTour).toHaveBeenCalledWith('tour-2');
    expect(component.tourLegs).toEqual(mockTour2.legs!);
    expect(component.value).toBeNull();
    expect(onChangeSpy).toHaveBeenCalledWith(null);
  });

  it('should clear tour legs and value when tour is set to null', () => {
    component.tour = mockTour1;
    component.ngOnChanges({
      tour: new SimpleChange(null, mockTour1, false),
    });
    component.writeValue('leg-1');

    const onChangeSpy = jasmine.createSpy('onChange');
    component.registerOnChange(onChangeSpy);

    component.tour = null;
    component.ngOnChanges({
      tour: new SimpleChange(mockTour1, null, false),
    });

    expect(component.tourLegs).toEqual([]);
    expect(component.value).toBeNull();
    expect(onChangeSpy).toHaveBeenCalledWith(null);
  });

  it('should implement ControlValueAccessor writeValue', () => {
    component.writeValue('leg-1');
    expect(component.value).toBe('leg-1');

    component.writeValue(null);
    expect(component.value).toBeNull();
  });

  it('should propagate changes through onChange and onTouched', () => {
    const onChangeSpy = jasmine.createSpy('onChange');
    const onTouchedSpy = jasmine.createSpy('onTouched');
    const eventEmitterSpy = spyOn(component.legChange, 'emit');

    component.registerOnChange(onChangeSpy);
    component.registerOnTouched(onTouchedSpy);

    component.onValueChange('leg-2');

    expect(component.value).toBe('leg-2');
    expect(onChangeSpy).toHaveBeenCalledWith('leg-2');
    expect(onTouchedSpy).toHaveBeenCalled();
    expect(eventEmitterSpy).toHaveBeenCalledWith('leg-2');
  });

  it('should handle setDisabledState', () => {
    component.setDisabledState(true);
    expect(component.disabled).toBeTrue();

    component.setDisabledState(false);
    expect(component.disabled).toBeFalse();
  });
});
