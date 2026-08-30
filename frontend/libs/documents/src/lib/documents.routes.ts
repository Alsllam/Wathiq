import { Routes } from '@angular/router';
import { authGuard } from '@wathiq/shared/auth';
import { DocumentsList } from './documents-list';
import { DocumentDetail } from './document-detail';

/// The lib exports ROUTES, the app lazy-loads them: the feature owns its own map, the app owns
/// where it mounts - the Nx boundary in routing form.
export const DOCUMENTS_ROUTES: Routes = [
  { path: '', canActivate: [authGuard], component: DocumentsList },
  { path: ':id', canActivate: [authGuard], component: DocumentDetail },
];
