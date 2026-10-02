import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CompleteUserProfileComponent } from './complete-user-profile.component';

describe('CompleteUserProfileComponent', () => {
  let component: CompleteUserProfileComponent;
  let fixture: ComponentFixture<CompleteUserProfileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CompleteUserProfileComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(CompleteUserProfileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
