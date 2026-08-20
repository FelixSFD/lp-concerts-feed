import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LegacyConcertFormComponent } from './legacy-concert-form.component';

describe('ConcertFormComponent', () => {
  let component: LegacyConcertFormComponent;
  let fixture: ComponentFixture<LegacyConcertFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LegacyConcertFormComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LegacyConcertFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
