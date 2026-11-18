# US04 - Réserver des places (intégration Angular)

En tant que spectateur authentifié, je souhaite réserver une ou plusieurs places pour une séance.

## Implémentation
- Plan de salle : `/seances/{id}/places`.
- Jusqu'à 6 places peuvent être sélectionnées.
- Le bouton Confirmer appelle POST `/api/reservations` avec un JWT Bearer.
- En cas d'utilisateur non connecté, redirection vers `/connexion`, puis retour à la séance.
- La réponse affiche l'identifiant de réservation et le code du billet.
- Erreur HTTP 409 : informe qu'une place a été réservée et actualise le plan.
- Erreur HTTP 401 : purge la session expirée et demande une nouvelle connexion.
- La protection finale contre une double réservation repose toujours sur le unique index `(SeanceId, SeatId)` de PostgreSQL, pas sur l'interface Angular.

## Critères d'acceptation à tester
1. Sélectionner une place libre sans être connecté => redirection connexion.
2. Se connecter => retour au plan et sélection conservée si disponible.
3. Confirmer => réservation validée et ticketCode affiché.
4. Rafraîchir les places => la place réservée est indisponible.
5. Tenter de réserver la même place avec un second compte => HTTP 409.

## À faire ensuite
Afficher la page de billet numérique depuis `GET /api/reservations/{id}/ticket` (US05).
