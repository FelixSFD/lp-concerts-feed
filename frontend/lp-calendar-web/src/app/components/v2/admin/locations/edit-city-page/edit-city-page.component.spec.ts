import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditCityPageComponent } from './edit-city-page.component';

describe('EditCityPageComponent', () => {
  let component: EditCityPageComponent;
  let fixture: ComponentFixture<EditCityPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditCityPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditCityPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
