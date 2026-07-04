import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditUserPageComponent } from './edit-user-page.component';

describe('EditUserComponent', () => {
  let component: EditUserPageComponent;
  let fixture: ComponentFixture<EditUserPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditUserPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditUserPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
