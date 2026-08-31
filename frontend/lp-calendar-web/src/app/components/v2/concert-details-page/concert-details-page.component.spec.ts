import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConcertDetailsPageComponent } from './concert-details-page.component';

describe('ConcertDetailsPageComponent', () => {
  let component: ConcertDetailsPageComponent;
  let fixture: ComponentFixture<ConcertDetailsPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConcertDetailsPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConcertDetailsPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
