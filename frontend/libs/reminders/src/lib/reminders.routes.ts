import { Routes } from '@angular/router';
import { authGuard } from '@wathiq/shared/auth';
import { RemindersPage } from './reminders-page';

export const REMINDERS_ROUTES: Routes = [
  { path: '', canActivate: [authGuard], component: RemindersPage },
];
