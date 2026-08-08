import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddCityPageComponent } from './add-city-page.component';

describe('AddCityPageComponent', () => {
  let component: AddCityPageComponent;
  let fixture: ComponentFixture<AddCityPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddCityPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddCityPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
