import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditVenuePageComponent } from './edit-venue-page.component';

describe('EditVenuePageComponent', () => {
  let component: EditVenuePageComponent;
  let fixture: ComponentFixture<EditVenuePageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditVenuePageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditVenuePageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
