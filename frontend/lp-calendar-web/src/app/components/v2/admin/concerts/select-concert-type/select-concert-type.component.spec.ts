import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of } from 'rxjs';
import { SelectConcertTypeComponent } from './select-concert-type.component';
import { ConcertTypesService } from '../../../../../services/concert-types.service';
import { ConcertTypeDto } from '../../../../../modules/lpshows-api/v3';

describe('SelectConcertTypeComponent', () => {
  let component: SelectConcertTypeComponent;
  let fixture: ComponentFixture<SelectConcertTypeComponent>;
  let concertTypesService: jasmine.SpyObj<ConcertTypesService>;

  const mockConcertTypes: ConcertTypeDto[] = [
    { id: 1, name: 'Headline Show' },
    { id: 2, name: 'Festival' },
  ];

  beforeEach(async () => {
    const serviceSpy = jasmine.createSpyObj('ConcertTypesService', ['getConcertTypes']);
    serviceSpy.getConcertTypes.and.returnValue(of(mockConcertTypes));

    await TestBed.configureTestingModule({
      imports: [SelectConcertTypeComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ConcertTypesService, useValue: serviceSpy },
      ],
    }).compileComponents();

    concertTypesService = TestBed.inject(ConcertTypesService) as jasmine.SpyObj<ConcertTypesService>;
    fixture = TestBed.createComponent(SelectConcertTypeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load concert types on init', () => {
    expect(concertTypesService.getConcertTypes).toHaveBeenCalled();
    expect(component.concertTypes).toEqual(mockConcertTypes);
  });

  it('should implement ControlValueAccessor writeValue', () => {
    component.writeValue(1);
    expect(component.value).toBe(1);

    component.writeValue(null);
    expect(component.value).toBeNull();
  });

  it('should propagate changes through onChange and onTouched', () => {
    const onChangeSpy = jasmine.createSpy('onChange');
    const onTouchedSpy = jasmine.createSpy('onTouched');
    const eventEmitterSpy = spyOn(component.concertTypeChange, 'emit');

    component.registerOnChange(onChangeSpy);
    component.registerOnTouched(onTouchedSpy);

    component.onValueChange(2);

    expect(component.value).toBe(2);
    expect(onChangeSpy).toHaveBeenCalledWith(2);
    expect(onTouchedSpy).toHaveBeenCalled();
    expect(eventEmitterSpy).toHaveBeenCalledWith(2);
  });

  it('should handle setDisabledState', () => {
    component.setDisabledState(true);
    expect(component.disabled).toBeTrue();

    component.setDisabledState(false);
    expect(component.disabled).toBeFalse();
  });
});
