import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ImportConcertDialogContentComponent } from './import-concert-dialog-content.component';

describe('ImportConcertDialogContentComponent', () => {
  let component: ImportConcertDialogContentComponent;
  let fixture: ComponentFixture<ImportConcertDialogContentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImportConcertDialogContentComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(ImportConcertDialogContentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
