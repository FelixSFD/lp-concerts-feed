import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TourMapPageComponent } from './tour-map-page.component';

describe('TourMapPageComponent', () => {
  let component: TourMapPageComponent;
  let fixture: ComponentFixture<TourMapPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TourMapPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TourMapPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
