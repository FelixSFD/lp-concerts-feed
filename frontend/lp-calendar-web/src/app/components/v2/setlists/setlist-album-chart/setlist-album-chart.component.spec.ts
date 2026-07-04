import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SetlistAlbumChartComponent } from './setlist-album-chart.component';

describe('SetlistAlbumChartComponent', () => {
  let component: SetlistAlbumChartComponent;
  let fixture: ComponentFixture<SetlistAlbumChartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SetlistAlbumChartComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SetlistAlbumChartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
