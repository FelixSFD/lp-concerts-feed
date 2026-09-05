import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddLegacyConcertPageComponent } from './add-legacy-concert-page.component';

describe('AddConcertPageComponent', () => {
  let component: AddLegacyConcertPageComponent;
  let fixture: ComponentFixture<AddLegacyConcertPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddLegacyConcertPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddLegacyConcertPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
