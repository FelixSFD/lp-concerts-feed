import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of } from 'rxjs';
import { SelectTourComponent } from './select-tour.component';
import { ToursService } from '../../../../../services/tours.service';
import { TourDto } from '../../../../../modules/lpshows-api/v3';

describe('SelectTourComponent', () => {
  let component: SelectTourComponent;
  let fixture: ComponentFixture<SelectTourComponent>;
  let toursService: jasmine.SpyObj<ToursService>;

  const mockTours: TourDto[] = [
    { id: 'tour-1', name: 'From Zero World Tour' },
    { id: 'tour-2', name: 'Hunting Party Tour' },
  ];

  beforeEach(async () => {
    const serviceSpy = jasmine.createSpyObj('ToursService', ['getTours']);
    serviceSpy.getTours.and.returnValue(of(mockTours));

    await TestBed.configureTestingModule({
      imports: [SelectTourComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ToursService, useValue: serviceSpy },
      ],
    }).compileComponents();

    toursService = TestBed.inject(ToursService) as jasmine.SpyObj<ToursService>;
    fixture = TestBed.createComponent(SelectTourComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load tours on init', () => {
    expect(toursService.getTours).toHaveBeenCalled();
    expect(component.tours).toEqual(mockTours);
  });

  it('should implement ControlValueAccessor writeValue', () => {
    component.writeValue('tour-1');
    expect(component.value).toBe('tour-1');

    component.writeValue(null);
    expect(component.value).toBeNull();
  });

  it('should propagate changes through onChange and onTouched', () => {
    const onChangeSpy = jasmine.createSpy('onChange');
    const onTouchedSpy = jasmine.createSpy('onTouched');
    const eventEmitterSpy = spyOn(component.tourChange, 'emit');

    component.registerOnChange(onChangeSpy);
    component.registerOnTouched(onTouchedSpy);

    component.onValueChange('tour-2');

    expect(component.value).toBe('tour-2');
    expect(onChangeSpy).toHaveBeenCalledWith('tour-2');
    expect(onTouchedSpy).toHaveBeenCalled();
    expect(eventEmitterSpy).toHaveBeenCalledWith('tour-2');
  });

  it('should handle setDisabledState', () => {
    component.setDisabledState(true);
    expect(component.disabled).toBeTrue();

    component.setDisabledState(false);
    expect(component.disabled).toBeFalse();
  });
});
