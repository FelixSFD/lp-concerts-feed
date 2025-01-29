import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConcertBadgesComponent } from './concert-badges.component';

describe('ConcertBadgesComponent', () => {
  let component: ConcertBadgesComponent;
  let fixture: ComponentFixture<ConcertBadgesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConcertBadgesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConcertBadgesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
