import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddCountryPageComponent } from './add-country-page.component';

describe('AddCountryPageComponent', () => {
  let component: AddCountryPageComponent;
  let fixture: ComponentFixture<AddCountryPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddCountryPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddCountryPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
