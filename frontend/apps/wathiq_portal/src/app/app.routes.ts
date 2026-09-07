import { Route } from '@angular/router';

export const appRoutes: Route[] = [
  {
    path: 'guides',
    // Public: no guard - the guides are the community half of the service (5.1's design).
    loadChildren: () => import('@wathiq/guides').then((m) => m.GUIDES_ROUTES),
  },
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
