import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppleMusicArtworkComponent } from './apple-music-artwork.component';

describe('AppleMusicArtworkComponent', () => {
  let component: AppleMusicArtworkComponent;
  let fixture: ComponentFixture<AppleMusicArtworkComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppleMusicArtworkComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AppleMusicArtworkComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
