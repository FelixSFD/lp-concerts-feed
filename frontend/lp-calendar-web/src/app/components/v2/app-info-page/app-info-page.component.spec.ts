import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppInfoPageComponent } from './app-info-page.component';

describe('AppInfoPageComponent', () => {
  let component: AppInfoPageComponent;
  let fixture: ComponentFixture<AppInfoPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppInfoPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AppInfoPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
