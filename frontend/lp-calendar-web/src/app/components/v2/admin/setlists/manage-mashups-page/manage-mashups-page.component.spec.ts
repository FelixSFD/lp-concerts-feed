import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageMashupsPageComponent } from './manage-mashups-page.component';

describe('ManageMashupsPageComponent', () => {
  let component: ManageMashupsPageComponent;
  let fixture: ComponentFixture<ManageMashupsPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageMashupsPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManageMashupsPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
