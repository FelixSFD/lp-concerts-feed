import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LegacyConcertDetailsPageComponent } from './legacy-concert-details-page.component';

describe('ConcertDetailsPageComponent', () => {
  let component: LegacyConcertDetailsPageComponent;
  let fixture: ComponentFixture<LegacyConcertDetailsPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LegacyConcertDetailsPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LegacyConcertDetailsPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
