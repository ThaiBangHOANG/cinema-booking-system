
# Git Workflow - Le Grand Cinema

## Branching Strategy

The project follows a simplified Git Flow workflow.

### Main branches

- `main`: stable, production-ready code.
- `develop`: integration branch for new features.

### Naming Convention

- `feature/US01-consulter-films`
- `feature/US02-consulter-seances`
- `feature/US03-places-disponibles`
- `feature/US04-reservation`
- `feature/US05-billet-numerique`
- `fix/description-du-bug`
- `docs/description`
- `chore/description`

### Development Workflow

1. Create a feature branch from develop.
2. Implement the corresponding User Story.
3. Commit changes with descriptive messages.
4. Push the feature branch to GitHub.
5. Create a Pull Request targeting develop.
6. Verify tests before merging.
7. Merge validated changes into develop.
8. Merge stable releases into main.

### Commit Convention

- `feat:` new functionality
- `fix:` bug correction
- `docs:` documentation
- `test:` automated tests
- `chore:` maintenance

### Quality Rules

- No direct commits to main for feature development.
- All features must correspond to documented User Stories.
- Tests must pass before merging.
- Pull Requests must preserve development history.
- Never commit secrets or production credentials.
