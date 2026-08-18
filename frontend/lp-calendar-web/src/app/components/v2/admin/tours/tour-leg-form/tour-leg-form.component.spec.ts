import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MessageService } from 'primeng/api';
import { TourLegFormComponent } from './tour-leg-form.component';

describe('TourLegFormComponent', () => {
  let component: TourLegFormComponent;
  let fixture: ComponentFixture<TourLegFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TourLegFormComponent],
      providers: [MessageService]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TourLegFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
