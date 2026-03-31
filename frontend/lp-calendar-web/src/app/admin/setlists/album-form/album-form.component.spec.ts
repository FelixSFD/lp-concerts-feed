import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AlbumFormComponent } from './album-form.component';

describe('AlbumFormComponent', () => {
  let component: AlbumFormComponent;
  let fixture: ComponentFixture<AlbumFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AlbumFormComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AlbumFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
