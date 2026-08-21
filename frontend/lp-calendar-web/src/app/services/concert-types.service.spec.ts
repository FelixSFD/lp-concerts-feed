import { TestBed } from '@angular/core/testing';

import { ConcertTypesService } from './concert-types.service';

describe('ConcertTypesService', () => {
  let service: ConcertTypesService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ConcertTypesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
