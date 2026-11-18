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
  {
    path: 'seances',
    loadComponent: () => import('./pages/seances/seances').then(m => m.SeancesPage)
  },
  {
    path: 'seances/:id/places',
    loadComponent: () => import('./pages/seats/seats').then(m => m.SeatsPage)
  },
  { path: 'connexion', loadComponent: () => import('./pages/auth/auth').then(m => m.AuthPage) },
  { path: '**', redirectTo: 'films' }
];
