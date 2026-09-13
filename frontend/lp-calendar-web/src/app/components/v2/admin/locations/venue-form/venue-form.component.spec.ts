import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { MessageService } from 'primeng/api';
import { VenueFormComponent } from './venue-form.component';
import { PreviousVenueNameDto, VenueWithDetailsDto } from '../../../../../modules/lpshows-api/v3';

describe('VenueFormComponent', () => {
  let component: VenueFormComponent;
  let fixture: ComponentFixture<VenueFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VenueFormComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        MessageService,
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VenueFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with empty historic names', () => {
    expect(component.historicNames$()).toEqual([]);
  });

  it('should populate historic names when fillFormWith is called with venueNames', () => {
    const mockHistoricNames: PreviousVenueNameDto[] = [
      { id: '1', venueId: 10, name: 'O2 World', usedFrom: '2008-09-10', usedUntil: '2015-06-30' },
      { id: '2', venueId: 10, name: 'Mercedes-Benz Arena', usedFrom: '2015-07-01', usedUntil: '2024-03-21' },
    ];

    const mockVenue: VenueWithDetailsDto = {
      id: 10,
      currentName: 'Uber Arena',
      countryCode: 'DE',
      cityId: 1,
      timeZoneId: 'Europe/Berlin',
      latitude: 52.505,
      longitude: 13.443,
      venueNames: mockHistoricNames,
      city: {
        id: 1,
        name: 'Berlin',
        countryCode: 'DE',
        nativeName: 'Berlin',
        country: { isoCode: 'DE', name: 'Germany', nativeName: 'Deutschland' },
      },
    };

    component.fillFormWith(mockVenue);

    expect(component.venueForm.controls.currentName.value).toBe('Uber Arena');
    expect(component.venueForm.controls.countryCode.value).toBe('DE');
    expect(component.venueForm.controls.cityId.value).toBe(1);
    expect(component.historicNames$()).toEqual(mockHistoricNames);
  });

  it('should open dialog and reset historicNameForm when adding a historic name', () => {
    component.onAddHistoricNameClicked();

    expect(component.isShowingHistoricNameDialog$()).toBeTrue();
    expect(component.isEditingHistoricName$()).toBeFalse();
    expect(component.editingHistoricNameIndex$()).toBeNull();
    expect(component.historicNameForm.controls.name.value).toBeNull();
  });

  it('should add a new historic name when saving from dialog', () => {
    component.onAddHistoricNameClicked();
    component.historicNameForm.setValue({
      name: 'O2 World',
      usedFrom: new Date(2008, 8, 10),
      usedUntil: new Date(2015, 5, 30),
    });

    component.onSaveHistoricNameDialog();

    expect(component.isShowingHistoricNameDialog$()).toBeFalse();
    expect(component.historicNames$().length).toBe(1);
    expect(component.historicNames$()[0].name).toBe('O2 World');
    expect(component.historicNames$()[0].usedFrom).toBe('2008-09-10');
    expect(component.historicNames$()[0].usedUntil).toBe('2015-06-30');
  });

  it('should populate dialog form when editing an existing historic name', () => {
    component.historicNames$.set([
      { id: '1', venueId: 10, name: 'O2 World', usedFrom: '2008-09-10', usedUntil: '2015-06-30' },
    ]);

    component.onEditHistoricNameClicked(component.historicNames$()[0], 0);

    expect(component.isShowingHistoricNameDialog$()).toBeTrue();
    expect(component.isEditingHistoricName$()).toBeTrue();
    expect(component.editingHistoricNameIndex$()).toBe(0);
    expect(component.historicNameForm.controls.name.value).toBe('O2 World');
    expect(component.historicNameForm.controls.usedFrom.value).toEqual(new Date(2008, 8, 10));
    expect(component.historicNameForm.controls.usedUntil.value).toEqual(new Date(2015, 5, 30));
  });

  it('should update existing historic name when saving edit from dialog', () => {
    component.historicNames$.set([
      { id: '1', venueId: 10, name: 'O2 World', usedFrom: '2008-09-10', usedUntil: '2015-06-30' },
    ]);

    component.onEditHistoricNameClicked(component.historicNames$()[0], 0);
    component.historicNameForm.setValue({
      name: 'O2 World Berlin',
      usedFrom: new Date(2008, 8, 10),
      usedUntil: new Date(2015, 5, 30),
    });

    component.onSaveHistoricNameDialog();

    expect(component.isShowingHistoricNameDialog$()).toBeFalse();
    expect(component.historicNames$().length).toBe(1);
    expect(component.historicNames$()[0].name).toBe('O2 World Berlin');
    expect(component.historicNames$()[0].id).toBe('1');
  });

  it('should delete a historic name when onDeleteHistoricNameClicked is called', () => {
    component.historicNames$.set([
      { id: '1', venueId: 10, name: 'O2 World', usedFrom: '2008-09-10', usedUntil: '2015-06-30' },
      { id: '2', venueId: 10, name: 'Mercedes-Benz Arena', usedFrom: '2015-07-01', usedUntil: '2024-03-21' },
    ]);

    component.onDeleteHistoricNameClicked(0);

    expect(component.historicNames$().length).toBe(1);
    expect(component.historicNames$()[0].name).toBe('Mercedes-Benz Arena');
  });

  it('should include historic names in readFromForm output', () => {
    const mockHistoricNames: PreviousVenueNameDto[] = [
      { id: '1', venueId: 10, name: 'O2 World', usedFrom: '2008-09-10', usedUntil: '2015-06-30' },
    ];

    component.venueForm.setValue({
      countryCode: 'DE',
      cityId: 1,
      currentName: 'Uber Arena',
      timezone: 'Europe/Berlin',
      latitude: 52.505,
      longitude: 13.443,
    });
    component.historicNames$.set(mockHistoricNames);

    const result = component.readFromForm();

    expect(result).not.toBeNull();
    expect(result?.currentName).toBe('Uber Arena');
    expect(result?.historicNames).toEqual(mockHistoricNames);
  });
});
