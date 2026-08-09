import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageCountriesPageComponent } from './manage-countries-page.component';

describe('ManageCountriesPageComponent', () => {
  let component: ManageCountriesPageComponent;
  let fixture: ComponentFixture<ManageCountriesPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageCountriesPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManageCountriesPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
