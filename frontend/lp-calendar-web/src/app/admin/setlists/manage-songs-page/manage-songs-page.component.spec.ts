import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageSongsPageComponent } from './manage-songs-page.component';

describe('ManageSongsPageComponent', () => {
  let component: ManageSongsPageComponent;
  let fixture: ComponentFixture<ManageSongsPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageSongsPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManageSongsPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
