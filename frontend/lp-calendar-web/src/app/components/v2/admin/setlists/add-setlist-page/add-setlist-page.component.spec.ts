import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddSetlistPageComponent } from './add-setlist-page.component';

describe('AddSetlistPageComponent', () => {
  let component: AddSetlistPageComponent;
  let fixture: ComponentFixture<AddSetlistPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddSetlistPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddSetlistPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
