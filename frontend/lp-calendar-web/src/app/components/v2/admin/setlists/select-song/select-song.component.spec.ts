import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SelectSongComponent } from './select-song.component';

describe('SelectSongComponent', () => {
  let component: SelectSongComponent;
  let fixture: ComponentFixture<SelectSongComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SelectSongComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SelectSongComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
