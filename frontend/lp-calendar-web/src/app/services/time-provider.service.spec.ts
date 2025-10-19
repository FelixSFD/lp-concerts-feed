import { TestBed } from '@angular/core/testing';

import { TimeProviderService } from './time-provider.service';

describe('TimeProviderService', () => {
  let service: TimeProviderService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TimeProviderService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
