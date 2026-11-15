import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConcertMapComponent } from './concert-map.component';

describe('ConcertMapComponent', () => {
  let component: ConcertMapComponent;
  let fixture: ComponentFixture<ConcertMapComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConcertMapComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(ConcertMapComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
