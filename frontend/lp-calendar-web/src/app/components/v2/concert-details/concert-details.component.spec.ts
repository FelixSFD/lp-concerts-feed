import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConcertDetailsComponent } from './concert-details.component';
import { MessageService } from 'primeng/api';
import { MatomoTracker } from 'ngx-matomo-client';
import { provideRouter } from '@angular/router';

describe('ConcertDetailsComponent', () => {
  let component: ConcertDetailsComponent;
  let fixture: ComponentFixture<ConcertDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConcertDetailsComponent],
      providers: [
        MessageService,
        provideRouter([]),
        { provide: MatomoTracker, useValue: { trackLink: jasmine.createSpy('trackLink') } }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConcertDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
