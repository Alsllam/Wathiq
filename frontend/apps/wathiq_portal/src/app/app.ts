import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { LanguageService } from '@wathiq/shared/i18n';
import { DocumentTypesPreview } from './document-types-preview';

@Component({
  imports: [RouterModule, TranslocoPipe, DocumentTypesPreview],
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  // Public so the template can call toggle() and read lang() - the service IS the view state.
  protected readonly language = inject(LanguageService);
}
