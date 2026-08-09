import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditCountryPageComponent } from './edit-country-page.component';

describe('EditCountryPageComponent', () => {
  let component: EditCountryPageComponent;
  let fixture: ComponentFixture<EditCountryPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditCountryPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditCountryPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
