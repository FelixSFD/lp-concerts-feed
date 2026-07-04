import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditConcertPageComponent } from './edit-concert-page.component';

describe('EditConcertPageComponent', () => {
  let component: EditConcertPageComponent;
  let fixture: ComponentFixture<EditConcertPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditConcertPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditConcertPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
