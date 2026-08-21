import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { ConcertFormComponent } from './concert-form.component';
import { ConcertTypesService } from '../../../../../services/concert-types.service';
import { ConcertDetailsDto } from '../../../../../modules/lpshows-api/v3';

describe('ConcertFormComponent', () => {
  let component: ConcertFormComponent;
  let fixture: ComponentFixture<ConcertFormComponent>;
  let concertTypesService: jasmine.SpyObj<ConcertTypesService>;

  const mockConcertDetails: ConcertDetailsDto = {
    id: 'concert-123',
    customTitle: 'Special Show in Berlin',
    concertType: {
      id: 1,
      name: 'Headline Show',
    },
    venue: {
      id: 10,
      name: 'Uber Arena',
    } as any,
  };

  beforeEach(async () => {
    const concertTypesSpy = jasmine.createSpyObj('ConcertTypesService', ['getConcertTypes']);
    concertTypesSpy.getConcertTypes.and.returnValue(
      of([
        { id: 1, name: 'Headline Show' },
        { id: 2, name: 'Festival' },
      ])
    );

    await TestBed.configureTestingModule({
      imports: [ConcertFormComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        MessageService,
        { provide: ConcertTypesService, useValue: concertTypesSpy },
      ],
    }).compileComponents();

    concertTypesService = TestBed.inject(ConcertTypesService) as jasmine.SpyObj<ConcertTypesService>;
    fixture = TestBed.createComponent(ConcertFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with invalid status before concertTypeId is selected', () => {
    expect(component.concertForm.valid).toBeFalse();
    expect(component.concertForm.controls.concertTypeId.value).toBeNull();
  });

  it('should be valid when concertTypeId is selected', () => {
    component.concertForm.controls.concertTypeId.setValue(1);
    component.concertForm.controls.customTitle.setValue('Test Title');
    expect(component.concertForm.valid).toBeTrue();
  });

  it('should fill form with concert details', () => {
    component.fillFormWith(mockConcertDetails);

    expect(component.concertForm.controls.customTitle.value).toBe('Special Show in Berlin');
    expect(component.concertForm.controls.concertTypeId.value).toBe(1);
  });

  it('should read from form properly', () => {
    component.fillFormWith(mockConcertDetails);
    const result = component.readFromForm();

    expect(result).toEqual({
      customTitle: 'Special Show in Berlin',
      concertTypeId: 1,
    });
  });

  it('should reset form properly', () => {
    component.fillFormWith(mockConcertDetails);
    component.reset();

    expect(component.concertForm.controls.customTitle.value).toBe('');
    expect(component.concertForm.controls.concertTypeId.value).toBeNull();
  });

  it('should emit saveClicked when onSaveClicked is triggered with valid form', () => {
    spyOn(component.saveClicked, 'emit');
    component.fillFormWith(mockConcertDetails);

    component.onSaveClicked();

    expect(component.saveClicked.emit).toHaveBeenCalledWith({
      customTitle: 'Special Show in Berlin',
      concertTypeId: 1,
    });
  });
});
