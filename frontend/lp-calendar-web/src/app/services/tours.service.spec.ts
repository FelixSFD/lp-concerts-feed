import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ToursService } from './tours.service';
import { ToursApi } from '../modules/lpshows-api/v3';

describe('ToursService', () => {
  let service: ToursService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        ToursApi,
        ToursService
      ]
    });
    service = TestBed.inject(ToursService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
