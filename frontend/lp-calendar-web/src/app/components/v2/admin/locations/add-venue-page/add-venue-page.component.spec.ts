import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddVenuePageComponent } from './add-venue-page.component';

describe('AddVenuePageComponent', () => {
  let component: AddVenuePageComponent;
  let fixture: ComponentFixture<AddVenuePageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddVenuePageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddVenuePageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
