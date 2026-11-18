import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-access-denied',
  standalone: true,
  imports: [RouterLink],
  template: `
    <section class="denied-page">
      <h1>Accès refusé</h1>
      <p>Votre compte ne dispose pas du rôle administrateur.</p>
      <p>Utilisez un compte Admin autorisé pour accéder à cet espace.</p>
      <a routerLink="/films">Retour à l'accueil</a>
    </section>
  `,
  styles: [`
    .denied-page { max-width: 700px; padding: 55px 20px; margin: 0 auto; }
    .denied-page h1 { font: bold 36px Georgia, serif; color: #25364c; }
    .denied-page p { color: #657187; }
    .denied-page a { color: #315886; font-weight: 700; }
  `]
})
export class AccessDeniedPage {}
