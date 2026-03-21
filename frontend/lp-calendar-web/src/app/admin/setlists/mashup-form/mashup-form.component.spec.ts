import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MashupFormComponent } from './mashup-form.component';

describe('MashupFormComponent', () => {
  let component: MashupFormComponent;
  let fixture: ComponentFixture<MashupFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MashupFormComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MashupFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
