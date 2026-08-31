import { Route } from '@angular/router';

export const appRoutes: Route[] = [
  {
    path: 'reminders',
    loadChildren: () => import('@wathiq/reminders').then((m) => m.REMINDERS_ROUTES),
  },
  {
    path: 'documents',
    // Lazy: the feature lib's code downloads on first navigation, not at boot.
    loadChildren: () => import('@wathiq/documents').then((m) => m.DOCUMENTS_ROUTES),
  },
];
