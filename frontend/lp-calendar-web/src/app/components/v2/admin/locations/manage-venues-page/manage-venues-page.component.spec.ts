import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageVenuesPageComponent } from './manage-venues-page.component';

describe('ManageVenuesPageComponent', () => {
  let component: ManageVenuesPageComponent;
  let fixture: ComponentFixture<ManageVenuesPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageVenuesPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManageVenuesPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
