import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ManageToursPageComponent } from './manage-tours-page.component';
import { ToursApi } from '../../../../../modules/lpshows-api/v3';

describe('ManageToursPageComponent', () => {
  let component: ManageToursPageComponent;
  let fixture: ComponentFixture<ManageToursPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageToursPageComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        ConfirmationService,
        MessageService,
        ToursApi
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManageToursPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
