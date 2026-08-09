import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageCitiesPageComponent } from './manage-cities-page.component';

describe('ManageCitiesPageComponent', () => {
  let component: ManageCitiesPageComponent;
  let fixture: ComponentFixture<ManageCitiesPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageCitiesPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManageCitiesPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
