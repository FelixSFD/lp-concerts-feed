import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageAlbumsPageComponent } from './manage-albums-page.component';

describe('ManageAlbumsPageComponent', () => {
  let component: ManageAlbumsPageComponent;
  let fixture: ComponentFixture<ManageAlbumsPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageAlbumsPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManageAlbumsPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
