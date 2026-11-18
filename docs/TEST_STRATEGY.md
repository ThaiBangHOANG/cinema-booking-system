# Strategie de tests - Le Grand Cinema

## Objectif
Valider le parcours de reservation, les regles metier et les contraintes de base de donnees.

## Tests automatises xUnit
Les tests de `ReservationServiceTests.cs` couvrent :
- Test unitaire pur de la generation de TicketCode.
- Creation d'une reservation avec plusieurs places et generation d'un TicketCode.
- Rejet d'une place deja reservee.
- Rejet d'une selection vide ou de doublons de places.
- Rejet d'une place appartenant a une autre salle.
- Rejet d'une seance inexistante ou deja commencee.
- Autorisation de reserver la meme place pour deux seances differentes.
- Verification de la contrainte d'unicite `(SeanceId, SeatId)`.

Ces tests utilisent SQLite **en memoire** avec le vrai `CinemaDbContext`, pour eviter de modifier la base Neon pendant les tests. Il s'agit de tests automatises de service / integration legere, pas de tests de concurrence PostgreSQL.

## Coverage
Commande :
`dotnet test tests/Cinema.Api.Tests/Cinema.Api.Tests.csproj --collect:"XPlat Code Coverage" --logger "trx;LogFileName=cinema-tests.trx" --results-directory TestResults`

Sorties attendues :
- `TestResults/cinema-tests.trx`
- `TestResults/<id>/coverage.cobertura.xml`

La couverture de code mesure les lignes exercees dans l'assembly de l'API. Elle n'est pas une preuve de la qualite des tests, et ne doit pas etre decrite comme 100% si ce n'est pas le cas.

## Validation de concurrence (a effectuer separement)
L'index unique PostgreSQL `IX_ReservationSeats_SeanceId_SeatId` est la protection ultime. Un vrai test de concurrence doit envoyer deux demandes simultanees vers le meme siege de la meme seance contre PostgreSQL, verifier qu'une seule reservation est validee et que l'autre reponse est HTTP 409, puis verifier le nombre de lignes reservees.

Un script `tests/concurrent-booking.ps1` est fourni pour le test manuel automatise contre le backend local connecte a Neon. Il cree une reservation de demonstration ; utiliser uniquement des donnees de test.

## Captures a conserver pour le dossier
- Terminal avec les tests reussis.
- Fichier `coverage.cobertura.xml` genere.
- Screenshot du test de concurrence PostgreSQL.
- Screenshot de l'index unique dans Neon

## Limites
- Les tests SQLite ne valident pas le comportement specifique des erreurs PostgreSQL (SQLSTATE 23505).
- Le test de concurrence doit etre execute sur PostgreSQL avant d'affirmer qu'il est valide.
- Les tests de controllers, securite JWT et frontend peuvent etre ajoutes lors des iterations suivantes.
