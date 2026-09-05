import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { EditTourPageComponent } from './edit-tour-page.component';
import { ToursApi } from '../../../../../modules/lpshows-api/v3';

describe('EditTourPageComponent', () => {
  let component: EditTourPageComponent;
  let fixture: ComponentFixture<EditTourPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditTourPageComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            data: of({
              tour: {
                id: 'from-zero-world-tour-2025',
                name: 'From Zero World Tour 2025',
                legs: []
              }
            })
          }
        },
        ConfirmationService,
        MessageService,
        ToursApi
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditTourPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
