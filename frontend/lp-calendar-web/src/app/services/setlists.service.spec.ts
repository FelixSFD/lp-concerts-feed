import { TestBed } from '@angular/core/testing';

import { SetlistsService } from './setlists.service';

describe('SetlistsService', () => {
  let service: SetlistsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SetlistsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
