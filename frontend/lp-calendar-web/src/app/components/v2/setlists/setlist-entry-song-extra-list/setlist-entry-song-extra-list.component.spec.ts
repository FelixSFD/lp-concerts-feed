import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SetlistEntrySongExtraListComponent } from './setlist-entry-song-extra-list.component';

describe('SetlistEntrySongExtraListComponent', () => {
  let component: SetlistEntrySongExtraListComponent;
  let fixture: ComponentFixture<SetlistEntrySongExtraListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SetlistEntrySongExtraListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SetlistEntrySongExtraListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
