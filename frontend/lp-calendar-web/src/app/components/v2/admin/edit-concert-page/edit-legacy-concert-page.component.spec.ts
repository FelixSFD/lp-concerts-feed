import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditLegacyConcertPageComponent } from './edit-legacy-concert-page.component';

describe('EditConcertPageComponent', () => {
  let component: EditLegacyConcertPageComponent;
  let fixture: ComponentFixture<EditLegacyConcertPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditLegacyConcertPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditLegacyConcertPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
