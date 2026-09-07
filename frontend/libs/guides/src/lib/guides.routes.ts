import { Routes } from '@angular/router';
import { GuidesList } from './guides-list';
import { GuideDetail } from './guide-detail';
import { ChatPage } from './chat-page';

export const GUIDES_ROUTES: Routes = [
  { path: '', component: GuidesList },
  // 'chat' BEFORE ':slug' - a literal segment after a parameter would never match.
  { path: 'chat', component: ChatPage },
  { path: ':slug', component: GuideDetail },
];
