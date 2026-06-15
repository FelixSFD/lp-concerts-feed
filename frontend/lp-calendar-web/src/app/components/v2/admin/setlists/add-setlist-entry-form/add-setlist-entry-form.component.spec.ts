import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddSetlistEntryFormComponent } from './add-setlist-entry-form.component';

describe('AddSetlistEntryFormComponent', () => {
  let component: AddSetlistEntryFormComponent;
  let fixture: ComponentFixture<AddSetlistEntryFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddSetlistEntryFormComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddSetlistEntryFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
