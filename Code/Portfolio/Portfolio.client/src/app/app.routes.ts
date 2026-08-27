import { Routes } from '@angular/router';
import { Login } from './admin/login/login';
import { ChangePassword } from './admin/change-password/change-password';
import { Dashboard } from './admin/dashboard/dashboard';
import { ProfileEditor } from './admin/profile-editor/profile-editor';
import { EducationEditor } from './admin/education-editor/education-editor';
import { ExperienceEditor } from './admin/experience-editor/experience-editor';
import { TechnologiesManager } from './admin/technologies-manager/technologies-manager';
import { ProjectsList } from './admin/projects-list/projects-list';
import { ProjectEditor } from './admin/project-editor/project-editor';
import { Home } from './public/home/home';
import { ProjectDetail } from './public/project-detail/project-detail';
import { Contact } from './public/contact/contact';
import { authGuard } from './core/guards/auth.guard';
import { environment } from '../environments/environment';

export const routes: Routes = [
  // --- Admin (intentionally not linked from anywhere public) ---
  { path: 'admin/login', component: Login },
  { path: 'admin/change-password', component: ChangePassword, canActivate: [authGuard] },
  {
    path: 'admin',
    component: Dashboard,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'profile', pathMatch: 'full' },
      { path: 'profile', component: ProfileEditor },
      { path: 'education', component: EducationEditor },
      { path: 'experience', component: ExperienceEditor },
      { path: 'technologies', component: TechnologiesManager },
      { path: 'projects', component: ProjectsList },
      { path: 'projects/new', component: ProjectEditor },
      { path: 'projects/:id', component: ProjectEditor },
    ],
  },

  // --- Public ---
  { path: 'u/:slug', component: Home },
  { path: 'u/:slug/contact', component: Contact },
  { path: 'u/:slug/projects/:projectId', component: ProjectDetail },

  // Root domain lands directly on the owner's public portfolio, not the admin login.
  { path: '', redirectTo: `/u/${environment.ownerSlug}`, pathMatch: 'full' },

  // Anything unmatched also falls back to the public portfolio, rather than
  // exposing a generic 404 or accidentally hinting at admin routes.
  { path: '**', redirectTo: `/u/${environment.ownerSlug}` },
];