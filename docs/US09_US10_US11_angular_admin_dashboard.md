# Administration - Suivi des réservations et statistiques (US09 / US10 / US11)

## User Stories
- **US09** : En tant qu'administrateur, je veux suivre les réservations pour gérer l'activité du cinéma.
- **US10** : En tant qu'administrateur, je veux connaître les taux de remplissage des séances pour optimiser la programmation.
- **US11** : En tant qu'administrateur, je veux visualiser les tendances d'affluence pour identifier les périodes à surveiller.

## Frontend Angular 20

| Page | Route | API | Autorisation |
| --- | --- | --- | --- |
| Suivi des réservations | `/admin/reservations` | `GET /api/admin/reservations` | Admin Guard + JWT Admin côté API |
| Statistiques | `/admin/statistiques` | `GET /api/admin/statistics/occupancy` | Admin Guard + JWT Admin côté API |
| Affluence | `/admin/statistiques` | `GET /api/admin/statistics/affluence` | Admin Guard + JWT Admin côté API |

Le Backend existe déjà : ce lot n'ajoute ni migration ni modification de PostgreSQL.

## Critères d'acceptation
1. Un administrateur authentifié consulte les réservations récentes et peut filtrer par identifiant de séance.
2. Les indicateurs affichent les places réservées, disponibles et le taux de remplissage, cohérents avec les données PostgreSQL.
3. Les tendances quotidiennes et par plage horaire permettent d'identifier les périodes sensibles.
4. Les paramètres `days` (7, 14, 30) et dates de festival sont validés.
5. Un visiteur non connecté est redirigé vers la connexion ; un utilisateur non administrateur n'accède pas à l'espace Admin.
6. Une perte d'authentification côté API (`401`) provoque la déconnexion ; `403` mène à la page d'accès refusé.

## Limites MVP
- **Occupancy** : toujours les 14 prochains jours, conformément à l'API existante.
- **Affluence** : horizon configurable de 7 à 30 jours. Les heures sont regroupées en **UTC**.
- **Festival** : les dates sont saisies manuellement pour simuler un scénario ; pas de calendrier officiel importé.
- **Anticipation** : alertes heuristiques (réservations, week-ends, festival), **pas une prévision prédictive fiable**.
- **Reservation list** : les 20/50/100/200 dernières réservations, sans pagination serveur réelle.

## Tests manuels à exécuter et à capturer (ne pas cocher sans exécution)
- [ ] `ng build` réussi
- [ ] Login Admin > `/admin/reservations` HTTP 200, présence de la réservation créée en US04
- [ ] Filtre `seanceId=11` (ou ID réel) réduit la liste aux réservations correspondantes
- [ ] `/admin/statistiques` affiche les KPI, les séances et les tendances
- [ ] `days=7` et deux dates de festival : changements visibles dans la table quotidienne
- [ ] Sans JWT, accès aux routes Admin redirigé vers `/connexion`
- [ ] Avec JWT User, accès aux routes Admin refusé ; API retourne 403

## Pratiques de sécurité
- Le JWT est transmis par `Authorization: Bearer ...` dans les requêtes d'administration.
- Les identités et autorisations doivent toujours être validées par l'API ; les guards Angular ne suffisent pas.
- Aucun secret, mot de passe ou token n'est logué ou documenté.