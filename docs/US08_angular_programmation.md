# US08 - Gestion de la programmation (Angular)

## User Story
En tant qu'administrateur, je souhaite creer, modifier et supprimer des seances
afin de tenir a jour la programmation du cinema.

## API utilisees
- GET /api/admin/seances (liste)
- POST /api/admin/seances (creation)
- PUT /api/admin/seances/{id} (modification)
- DELETE /api/admin/seances/{id} (suppression)
- GET /api/films (choix du film)

## Regles et securite
- Route /admin/programmation protegee par adminGuard.
- API backend accessible au seul role Admin via JWT.
- La date saisie en heure locale est convertie au format UTC ISO 8601.
- Le backend rejette les chevauchements de projections (HTTP 409).
- Le backend interdit modification/suppression d'une seance ayant des reservations (HTTP 409).
- L'interface demande confirmation avant toute suppression.
- Les droits sont verifies sur le serveur, le guard Angular ne constitue pas une protection suffisante.

## Criteres d'acceptation a verifier
- Un admin peut consulter les seances, filtrer et paginer l'affichage.
- Un admin peut creer une seance future valide (HTTP 201).
- Un admin peut modifier une seance sans reservations (HTTP 204).
- Un admin peut supprimer une seance sans reservations (HTTP 204).
- Un conflit d'horaire dans une meme salle est refuse (HTTP 409).
- Une seance reservee ne peut etre modifiee/supprimee (HTTP 409).
- Un utilisateur non admin ne peut pas acceder a l'API (HTTP 403).

## Limite MVP
En l'absence d'endpoint distinct pour les salles, la liste des salles est deduite
_des seances existantes_. Cela fonctionne avec les 10 salles du jeu de demonstration.
Une salle sans aucune seance n'apparaitrait pas dans la liste.

## Preuves STUDI
Captures d'ecran Angular, Swagger des endpoints Admin et Pull Request vers develop.
Ne declarer comme tests passes que ceux qui ont effectivement ete executes.
