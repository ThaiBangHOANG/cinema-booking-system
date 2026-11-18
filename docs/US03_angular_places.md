# US03 - Visualiser et sélectionner les places (Angular)

## User Story
En tant que spectateur, je souhaite consulter les places disponibles et sélectionner mes sièges pour une séance afin de préparer ma réservation.

## Critères d'acceptation
- Afficher toutes les places de la salle liée à une séance.
- Distinguer places libres, sélectionnées et déjà réservées.
- Empêcher la sélection d'une place déjà réservée.
- Afficher la sélection courante et le nombre de places disponibles.
- Rafraîchir automatiquement les disponibilités toutes les 15 secondes et permettre une actualisation manuelle.
- Signaler lorsqu'une place sélectionnée est devenue indisponible.
- Afficher un message pour une séance inexistante.

## API
GET /api/seances/{seanceId}/seats
GET /api/seances/{id}

## Périmètre
Cette User Story prépare la sélection des sièges. Elle ne crée pas encore de réservation : l'authentification et l'appel POST /api/reservations seront intégrés dans la prochaine étape.

## Validation à réaliser
- Accéder à /seances/11/places et vérifier l'affichage de la salle.
- Vérifier qu'une place déjà réservée (A01 pour la séance 11, si les données sont inchangées) est désactivée.
- Sélectionner deux places disponibles et constater la mise à jour du récapitulatif.
- Actualiser et vérifier que la sélection valide est conservée.
- Vérifier le comportement pour une séance inexistante.
- Exécuter ng build et conserver une capture d'écran.
