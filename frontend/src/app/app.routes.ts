import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'films', pathMatch: 'full' },
  {
    path: 'films',
    loadComponent: () => import('./pages/films/films').then(m => m.FilmsPage)
  },
  {
    path: 'films/:id',
    loadComponent: () => import('./pages/film-detail/film-detail').then(m => m.FilmDetailPage)
  },
  { path: '**', redirectTo: 'films' }
];
