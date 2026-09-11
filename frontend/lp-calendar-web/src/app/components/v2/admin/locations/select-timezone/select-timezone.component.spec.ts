import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SelectTimezoneComponent } from './select-timezone.component';
import { TimeZone } from 'timezones-list';

describe('SelectTimezoneComponent', () => {
  let component: SelectTimezoneComponent;
  let fixture: ComponentFixture<SelectTimezoneComponent>;

  const mockTimezones: TimeZone[] = [
    { name: 'Europe/Berlin', label: '(GMT+01:00) Berlin', tzCode: 'Europe/Berlin', utc: '+01:00' },
    { name: 'America/New_York', label: '(GMT-05:00) New York', tzCode: 'America/New_York', utc: '-05:00' },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SelectTimezoneComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SelectTimezoneComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load default timezones on init', () => {
    expect(component.timezones().length).toBeGreaterThan(0);
    expect(component.value()).toBeNull();
  });

  it('should use availableTimezones if provided', () => {
    component.availableTimezones = mockTimezones;
    component.loadTimezones();
    expect(component.timezones()).toEqual(mockTimezones);
  });

  it('should implement ControlValueAccessor writeValue', () => {
    component.writeValue('Europe/Berlin');
    expect(component.value()).toBe('Europe/Berlin');

    component.writeValue(null);
    expect(component.value()).toBeNull();

    component.writeValue('');
    expect(component.value()).toBeNull();
  });

  it('should propagate changes through onChange, onTouched and timezoneChange', () => {
    const onChangeSpy = jasmine.createSpy('onChange');
    const onTouchedSpy = jasmine.createSpy('onTouched');
    const eventEmitterSpy = spyOn(component.timezoneChange, 'emit');

    component.registerOnChange(onChangeSpy);
    component.registerOnTouched(onTouchedSpy);

    component.onValueChange('America/New_York');

    expect(component.value()).toBe('America/New_York');
    expect(onChangeSpy).toHaveBeenCalledWith('America/New_York');
    expect(onTouchedSpy).toHaveBeenCalled();
    expect(eventEmitterSpy).toHaveBeenCalledWith('America/New_York');
  });

  it('should handle setDisabledState', () => {
    component.setDisabledState(true);
    expect(component.disabled).toBeTrue();

    component.setDisabledState(false);
    expect(component.disabled).toBeFalse();
  });
});
