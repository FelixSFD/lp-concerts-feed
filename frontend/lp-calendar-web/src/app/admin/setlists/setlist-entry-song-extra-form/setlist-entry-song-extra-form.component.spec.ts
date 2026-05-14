import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SetlistEntrySongExtraFormComponent } from './setlist-entry-song-extra-form.component';

describe('SetlistEntrySongExtraFormComponent', () => {
  let component: SetlistEntrySongExtraFormComponent;
  let fixture: ComponentFixture<SetlistEntrySongExtraFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SetlistEntrySongExtraFormComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SetlistEntrySongExtraFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
