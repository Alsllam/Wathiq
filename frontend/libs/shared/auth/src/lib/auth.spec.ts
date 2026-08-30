import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HttpRequest, HttpHandlerFn, provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Subject } from 'rxjs';
import { OAuthService, OAuthEvent } from 'angular-oauth2-oidc';
import { AuthService } from './auth.service';
import { authInterceptor } from './auth.interceptor';
import { authGuard } from './auth.guard';

/// The library boundary, scripted: tests drive token state + events, and assert our signals,
/// interceptor and guard - the same seam-testing shape as the backend's FakeChatClient.
class FakeOAuthService {
  events = new Subject<OAuthEvent>();
  private valid = false;
  private claims: object | null = null;
  loginCalls = 0;

  setSession(valid: boolean, claims: object | null = null) {
    this.valid = valid;
    this.claims = claims;
    this.events.next({ type: 'token_received' } as OAuthEvent);
  }

  hasValidAccessToken() { return this.valid; }
  getIdentityClaims() { return this.claims; }
  getAccessToken() { return this.valid ? 'test-token' : ''; }
  initCodeFlow() { this.loginCalls++; }
  logOut() { this.setSession(false); }
}

describe('shared-auth', () => {
  let fakeOAuth: FakeOAuthService;

  beforeEach(() => {
    fakeOAuth = new FakeOAuthService();
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(),
        provideHttpClientTesting(), // init()'s app-config call stays stubbed
        { provide: OAuthService, useValue: fakeOAuth },
      ],
    });
  });

  it('derives isAuthenticated and userName from library events', () => {
    const auth = TestBed.inject(AuthService);
    expect(auth.isAuthenticated()).toBe(false);
    expect(auth.userName()).toBeNull();

    fakeOAuth.setSession(true, { preferred_username: 'admin' });

    expect(auth.isAuthenticated()).toBe(true);
    expect(auth.userName()).toBe('admin');

    fakeOAuth.logOut();
    expect(auth.isAuthenticated()).toBe(false);
  });

  it('interceptor attaches the bearer token to API calls only', () => {
    fakeOAuth.setSession(true, {});
    TestBed.inject(AuthService);

    const seen: string[] = [];
    const next: HttpHandlerFn = (req) => {
      seen.push(req.headers.get('Authorization') ?? 'none');
      return null!;
    };

    TestBed.runInInjectionContext(() => {
      authInterceptor(new HttpRequest('GET', '/api/documents/documents'), next);
      authInterceptor(new HttpRequest('GET', '/i18n/ar.json'), next);
    });

    expect(seen).toEqual(['Bearer test-token', 'none']);
  });

  it('guard admits an authenticated user and redirects an anonymous one', () => {
    const auth = TestBed.inject(AuthService);

    fakeOAuth.setSession(true, {});
    expect(TestBed.runInInjectionContext(() => authGuard({} as never, {} as never))).toBe(true);

    fakeOAuth.setSession(false);
    expect(TestBed.runInInjectionContext(() => authGuard({} as never, {} as never))).toBe(false);
    expect(fakeOAuth.loginCalls).toBe(1);
    expect(auth.isAuthenticated()).toBe(false);
  });
});
