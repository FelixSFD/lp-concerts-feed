import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CalendarFeedBuilderComponent } from './calendar-feed-builder.component';

describe('CalendarFeedBuilderComponent', () => {
  let component: CalendarFeedBuilderComponent;
  let fixture: ComponentFixture<CalendarFeedBuilderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CalendarFeedBuilderComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CalendarFeedBuilderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
