import { Routes } from '@angular/router';
import { adminGuard } from './guards/admin.guard';

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
  {
    path: 'reservations/:id/billet',
    loadComponent: () => import('./pages/ticket/ticket').then(m => m.TicketPage)
  },
  {
    path: 'admin/programmation',
    canActivate: [adminGuard],
    loadComponent: () => import('./pages/admin-programmation/admin-programmation').then(m => m.AdminProgrammationPage)
  },
  {
    path: 'admin/reservations',
    canActivate: [adminGuard],
    loadComponent: () => import('./pages/admin-reservations/admin-reservations').then(m => m.AdminReservationsPage)
  },
  {
    path: 'admin/statistiques',
    canActivate: [adminGuard],
    loadComponent: () => import('./pages/admin-statistics/admin-statistics').then(m => m.AdminStatisticsPage)
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () => import('./pages/admin-home/admin-home').then(m => m.AdminHomePage)
  },
  {
    path: 'acces-refuse',
    loadComponent: () => import('./pages/access-denied/access-denied').then(m => m.AccessDeniedPage)
  },
  { path: 'connexion', loadComponent: () => import('./pages/auth/auth').then(m => m.AuthPage) },
  { path: '**', redirectTo: 'films' }
];
