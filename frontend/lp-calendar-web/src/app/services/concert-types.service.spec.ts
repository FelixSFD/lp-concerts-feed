import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ConcertTypesService } from './concert-types.service';

describe('ConcertTypesService', () => {
  let service: ConcertTypesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(ConcertTypesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
