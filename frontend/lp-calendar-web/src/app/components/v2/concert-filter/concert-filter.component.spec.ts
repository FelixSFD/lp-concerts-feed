import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConcertFilterComponent } from './concert-filter.component';

describe('ConcertFilterComponent', () => {
  let component: ConcertFilterComponent;
  let fixture: ComponentFixture<ConcertFilterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConcertFilterComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConcertFilterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
