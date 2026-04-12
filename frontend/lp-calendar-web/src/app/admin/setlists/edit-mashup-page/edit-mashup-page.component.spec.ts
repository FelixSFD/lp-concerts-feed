import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditMashupPageComponent } from './edit-mashup-page.component';

describe('EditMashupPageComponent', () => {
  let component: EditMashupPageComponent;
  let fixture: ComponentFixture<EditMashupPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditMashupPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditMashupPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
