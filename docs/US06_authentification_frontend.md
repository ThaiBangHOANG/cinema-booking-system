# US06 - Authentification utilisateur (Angular)

En tant que spectateur, je souhaite créer un compte et me connecter pour réserver mes places.

## Implémentation
- `/connexion` : connexion et inscription, validation du formulaire.
- API : POST `/api/auth/register` puis POST `/api/auth/login`.
- Le JWT est conservé en mémoire et dans `sessionStorage` pour une démo MVP. Il n'est pas poussé dans Git.
- Réservation accessible seulement avec JWT ; déconnexion dans l'en-tête.
- Après connexion, retour vers la sélection de places ; sélection temporaire conservée en mémoire.

## Critères d'acceptation à tester
- Un compte de démonstration valide peut être créé ; connexion HTTP 200.
- Une connexion erronée donne un message explicite (HTTP 401 de l'API).
- Les formulaires refusent les champs invalides.
- Après une connexion depuis le plan de salle, le parcours revient au bon écran.
- La déconnexion supprime la session côté navigateur.

## Limite de sécurité
Pour une application de production, privilégier une stratégie de session avec cookie HttpOnly et protection CSRF adaptée. `sessionStorage` reste exposé à un script injecté en cas de XSS. Ne jamais y placer un mot de passe.
