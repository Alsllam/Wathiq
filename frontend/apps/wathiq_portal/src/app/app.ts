import { Component, signal } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  imports: [RouterModule],
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  // Even a static title is a signal: under zoneless, signal reads are what tell Angular a
  // template can change - plain fields are the exception now, not the rule.
  protected readonly appName = signal('وثيق');
  protected readonly tagline = signal('مساعدك لوثائقك ومواعيدها · Your documents & deadlines assistant');
}
