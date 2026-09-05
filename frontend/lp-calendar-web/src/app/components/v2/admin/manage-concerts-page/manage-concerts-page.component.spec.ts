import {ComponentFixture, TestBed} from '@angular/core/testing';
import {of} from 'rxjs';
import {ManageConcertsPageComponent} from './manage-concerts-page.component';
import {ConcertsService} from '../../../../services/concerts.service';

describe('ManageConcertsPageComponent', () => {
  let component: ManageConcertsPageComponent;
  let fixture: ComponentFixture<ManageConcertsPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageConcertsPageComponent],
      providers: [{
        provide: ConcertsService,
        useValue: {getFilteredConcerts: () => of([])},
      }],
    }).compileComponents();

    fixture = TestBed.createComponent(ManageConcertsPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
