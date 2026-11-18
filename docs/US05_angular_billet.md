# US05 - Billet dématérialisé (Angular)

## User Story
En tant que spectateur authentifié, je souhaite consulter et imprimer mon billet dématérialisé après réservation pour retrouver les informations de ma séance.

## Fonctionnalités
- Lien **Voir et imprimer mon billet** après la réservation.
- Route Angular `/reservations/{id}/billet`.
- API `GET /api/reservations/{id}/ticket` avec JWT Bearer.
- Affichage : film, salle, date, places, numéro de réservation, code du billet.
- Impression / sauvegarde en PDF via le navigateur (`window.print`).
- Redirection vers la connexion puis retour au billet si la session est absente ou expirée.
- Erreurs 404 (billet inexistant ou non autorisé) et 403 gérées sans afficher d'autres données.

## Sécurité / limites du MVP
- Le Backend contrôle le propriétaire de la réservation ou le rôle Admin.
- Le Frontend n'est pas responsable du contrôle d'accès ; il se contente d'envoyer le JWT.
- Le ticketCode est un identifiant de billet, pas une preuve de paiement ni un QR code.
- Pas d'envoi automatique par e-mail ni de paiement intégré dans ce MVP.

## Tests manuels / critères d'acceptation
1. Un utilisateur connecté réserve un siège => reçoit un numéro de réservation.
2. Lien « Voir et imprimer mon billet » => HTTP 200 et les données correspondent à la réservation.
3. Actualiser la page du billet => données toujours disponibles avec une session valide.
4. Sans session => redirection `/connexion`, puis retour au billet après connexion.
5. Réservation inconnue => message « Billet introuvable ou inaccessible » (404).
6. Autre utilisateur connecté => ne peut pas voir le billet du premier (404).
7. Bouton imprimer => boîte d'impression du navigateur avec le billet.

## Preuves pour le dossier STUDI
- Capture page de confirmation et bouton vers billet.
- Capture du billet complet (masquer les données de personnes réelles si nécessaire).
- Capture Swagger de `GET /api/reservations/{id}/ticket` et des réponses 200 / 404.
- Pull Request `feature/US05-frontend-billet` vers `develop`.
