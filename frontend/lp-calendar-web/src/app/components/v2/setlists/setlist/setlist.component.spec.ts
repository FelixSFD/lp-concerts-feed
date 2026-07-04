import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SetlistComponent } from './setlist.component';

describe('SetlistComponent', () => {
  let component: SetlistComponent;
  let fixture: ComponentFixture<SetlistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SetlistComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SetlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
