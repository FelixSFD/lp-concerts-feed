import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageSetlistsPageComponent } from './manage-setlists-page.component';

describe('ManageSetlistsPageComponent', () => {
  let component: ManageSetlistsPageComponent;
  let fixture: ComponentFixture<ManageSetlistsPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageSetlistsPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManageSetlistsPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
