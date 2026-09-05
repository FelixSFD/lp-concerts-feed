import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { MessageService } from 'primeng/api';
import { AddTourPageComponent } from './add-tour-page.component';
import { ToursApi } from '../../../../../modules/lpshows-api/v3';

describe('AddTourPageComponent', () => {
  let component: AddTourPageComponent;
  let fixture: ComponentFixture<AddTourPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddTourPageComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        MessageService,
        ToursApi
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddTourPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
