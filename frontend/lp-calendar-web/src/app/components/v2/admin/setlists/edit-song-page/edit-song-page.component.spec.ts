import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditSongPageComponent } from './edit-song-page.component';

describe('EditSongPageComponent', () => {
  let component: EditSongPageComponent;
  let fixture: ComponentFixture<EditSongPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditSongPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditSongPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
