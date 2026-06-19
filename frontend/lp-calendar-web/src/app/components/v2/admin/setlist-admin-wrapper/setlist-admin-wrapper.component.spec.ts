import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SetlistAdminWrapperComponent } from './setlist-admin-wrapper.component';

describe('SetlistAdminWrapperComponent', () => {
  let component: SetlistAdminWrapperComponent;
  let fixture: ComponentFixture<SetlistAdminWrapperComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SetlistAdminWrapperComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SetlistAdminWrapperComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
