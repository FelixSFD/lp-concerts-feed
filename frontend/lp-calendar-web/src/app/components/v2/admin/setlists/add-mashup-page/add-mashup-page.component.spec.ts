import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddMashupPageComponent } from './add-mashup-page.component';

describe('AddMashupPageComponent', () => {
  let component: AddMashupPageComponent;
  let fixture: ComponentFixture<AddMashupPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddMashupPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddMashupPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
