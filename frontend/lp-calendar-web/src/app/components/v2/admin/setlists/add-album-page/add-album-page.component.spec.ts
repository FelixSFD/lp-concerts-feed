import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddAlbumPageComponent } from './add-album-page.component';

describe('AddAlbumPageComponent', () => {
  let component: AddAlbumPageComponent;
  let fixture: ComponentFixture<AddAlbumPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddAlbumPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddAlbumPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
