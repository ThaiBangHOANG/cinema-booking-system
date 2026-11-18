# US08 - Gerer la programmation

En tant qu'administrateur, je souhaite gerer les seances pour organiser la programmation du cinema.

## API

- GET /api/admin/seances
- GET /api/admin/seances/{id}
- POST /api/admin/seances
- PUT /api/admin/seances/{id}
- DELETE /api/admin/seances/{id}

Tous les endpoints exigent un JWT avec role `Admin`.

## Regles

- Seules les dates UTC futures sont acceptees a la creation/modification.
- Le film et la salle doivent exister.
- Les creneaux d'une salle ne doivent pas se chevaucher; un delai de 15 min est prevu entre les seances.
- On ne modifie ni ne supprime une seance avec des reservations.
- La base possede deja un index unique (RoomId, StartsAtUtc), qui bloque deux seances a la meme heure.

## Limite connue

La verification d'un chevauchement a deux heures de debut differentes est applicative, non atomique. Pour un usage de production avec plusieurs administrateurs concurrents, ajouter une contrainte d'exclusion PostgreSQL ou une transaction avec verrouillage adequat.

## Tests a conserver comme preuves

- Sans JWT => HTTP 401.
- JWT User => HTTP 403.
- JWT Admin : POST => HTTP 201.
- GET => HTTP 200.
- Horaire chevauchant => HTTP 409.
- PUT sans reservation => HTTP 204.
- DELETE sans reservation => HTTP 204.
- Modifier/supprimer une seance reservee => HTTP 409.
