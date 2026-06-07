import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditAlbumPageComponent } from './edit-album-page.component';

describe('EditAlbumPageComponent', () => {
  let component: EditAlbumPageComponent;
  let fixture: ComponentFixture<EditAlbumPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditAlbumPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditAlbumPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
