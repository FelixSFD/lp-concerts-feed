import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditSetlistPageComponent } from './edit-setlist-page.component';

describe('EditSetlistPageComponent', () => {
  let component: EditSetlistPageComponent;
  let fixture: ComponentFixture<EditSetlistPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditSetlistPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditSetlistPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
