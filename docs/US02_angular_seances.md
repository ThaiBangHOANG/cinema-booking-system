# US02 - Consultation des seances (Frontend Angular)

## User story
En tant que spectateur, je souhaite consulter les horaires des seances afin de choisir une projection.

## Fonctionnement
- La page `/seances` affiche les seances futures renvoyees par `GET /api/seances`.
- La page `/seances?filmId=1` filtre les seances par film via `GET /api/seances?filmId=1`.
- La fiche d'un film comporte un lien vers les seances de ce film.
- Les horaires UTC sont presentes dans le fuseau local du navigateur via Angular DatePipe.
- Chargement, erreur API, absence de resultats et affichage progressif sont geres.

## Criteres d'acceptation
- La liste des prochaines seances est visible.
- Le filtrage par film fonctionne.
- Le titre du film, la salle et l'horaire sont visibles.
- L'absence de seances est indiquee.

## Tests manuels (a completer apres execution)
- [ ] GET /api/seances via Angular : donnees affichees.
- [ ] /seances?filmId=1 : seances du film 1 uniquement.
- [ ] Navigation depuis /films/1 vers les seances.
- [ ] ng build : success.
