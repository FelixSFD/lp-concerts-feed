import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LinkinpediaConcertImporterPageComponent } from './linkinpedia-concert-importer-page.component';

describe('LinkinpediaConcertImporterPageComponent', () => {
  let component: LinkinpediaConcertImporterPageComponent;
  let fixture: ComponentFixture<LinkinpediaConcertImporterPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LinkinpediaConcertImporterPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LinkinpediaConcertImporterPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
