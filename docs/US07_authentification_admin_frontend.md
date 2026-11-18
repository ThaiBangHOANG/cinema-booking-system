# US07 — Authentification administrateur (Angular)

## User story
En tant qu'administrateur, je souhaite accéder à un espace de gestion sécurisé pour suivre l'activité du cinéma.

## Fonctionnement
- Même formulaire de connexion `/connexion` que le rôle User ; aucun second système de mots de passe.
- `POST /api/auth/login` retourne un JWT avec claim `role = Admin` pour le compte préalablement autorisé dans la base.
- Route `/admin` protégée par `adminGuard` : redirection vers Connexion en l'absence de session, vers `acces-refuse` pour le rôle User.
- La page `/admin` interroge `GET /api/admin/statistics/occupancy` avec `Authorization: Bearer <JWT>` afin de valider aussi les droits côté Backend.
- Le lien Administration est visible uniquement quand le compte connecté a le rôle Admin.
- Les API sous `/api/admin/...` restent protégées par `[Authorize(Roles = "Admin")]` indépendamment du guard Angular.
- L'inscription publique crée uniquement le rôle User. Un compte Admin de démonstration est préconfiguré par un administrateur de la base ; il ne peut pas se promouvoir depuis le frontend.

## Acceptance criteria (à vérifier réellement)
1. Sans session : navigation `/admin` -> `/connexion?returnUrl=%2Fadmin`.
2. Connexion en compte Admin -> `/admin` et chargement d'indicateurs HTTP 200.
3. Connexion en compte User -> `/admin` redirige vers `/acces-refuse`.
4. Appel direct `/api/admin/statistics/occupancy` avec JWT User -> HTTP 403 ; sans token -> 401.
5. Déconnexion Admin puis tentative d'accès `/admin` -> retour à Connexion.
6. Les retours `/seances/:id/places` et `/reservations/:id/billet` après connexion fonctionnent toujours.

## Sécurité et limites
- Le guard Angular n'est qu'un contrôle d'interface : la sécurité est imposée par ASP.NET Core.
- La session est stockée dans `sessionStorage` (choix limité à la démonstration STUDI). Eviter d'exposer les JWT dans captures d'écran, logs ou Git.
- La page d'accueil admin affiche pour l'instant un aperçu des statistiques ; les opérations complètes de programmation et de suivi des réservations constituent des étapes séparées.
