import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SetlistEntryIconsComponent } from './setlist-entry-icons.component';

describe('SetlistEntryIconsComponent', () => {
  let component: SetlistEntryIconsComponent;
  let fixture: ComponentFixture<SetlistEntryIconsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SetlistEntryIconsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SetlistEntryIconsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
