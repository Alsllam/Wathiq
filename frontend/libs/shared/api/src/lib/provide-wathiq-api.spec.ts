import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { injectApiUrl, provideWathiqApi } from './provide-wathiq-api';

describe('provideWathiqApi', () => {
  it('defaults to relative URLs (dev proxy mode)', () => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
    const apiUrl = TestBed.runInInjectionContext(() => injectApiUrl());
    expect(apiUrl('/api/x')).toBe('/api/x');
  });

  it('joins a configured base without doubled slashes', () => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection(), provideWathiqApi({ baseUrl: 'https://api.wathiq.app/' })],
    });
    const apiUrl = TestBed.runInInjectionContext(() => injectApiUrl());
    expect(apiUrl('api/x')).toBe('https://api.wathiq.app/api/x');
  });
});
