import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConcertsListComponent } from './concerts-list.component';

describe('ConcertsListComponent', () => {
  let component: ConcertsListComponent;
  let fixture: ComponentFixture<ConcertsListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConcertsListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConcertsListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
