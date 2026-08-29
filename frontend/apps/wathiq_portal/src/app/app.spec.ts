import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      // TestBed builds its own injector - the app's zoneless choice must be repeated here or
      // the test would run under a different change-detection model than production.
      providers: [provideZonelessChangeDetection(), provideRouter([])],
    }).compileComponents();
  });

  it('renders the Arabic-first shell header', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable(); // zoneless: await stability instead of detectChanges() rituals
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('وثيق');
  });
});
