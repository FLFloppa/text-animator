# Contributing

Thank you for your interest in improving FLFloppa Text Animator! We welcome bug reports, feature requests, and pull requests. This guide explains how to get started.

---

## Reporting issues

* Use GitHub issues to report bugs or request features.
* Include Unity version, package version, steps to reproduce, and relevant logs.
* Attach screenshots or GIFs when UI or animation artefacts are involved.

---

## Development workflow

1. **Fork** the repository and create a feature branch (`git checkout -b feature/my-enhancement`).
2. Install dependencies:
   * Unity 2022.3 LTS (or newer).
   * TextMesh Pro essential resources.
   * FLFloppa Editor Helpers (Git URL).
3. Open the project in Unity. Follow the [Editor Setup Guide](Docs/editor_setup.md) to create necessary assets.
4. Implement your changes, ensuring:
   * Runtime code remains allocation-free in hot paths.
   * ScriptableObject inspectors leverage the shared `ScriptableObjectInspectorBase`.
   * New handlers/actions/subsystems follow SOLID principles and include XML documentation.
5. Add or update documentation:
   * Update `Docs/tag_reference.md` when introducing new tags or parameters.
   * Update `Docs/CHANGELOG.md` under the **Unreleased** section.
   * Include samples or usage notes in `Samples~/` when applicable.
6. Test your changes (enter Play Mode, run automated tests if available).
7. Submit a pull request with:
   * Clear description of changes and motivation.
   * Links to related issues.
   * Notes on testing performed.

---

## Coding standards

* Follow the existing folder structure: runtime ScriptableObjects under `Runtime/`, editor tooling under `Editor/`.
* Use `FLFloppa.EditorHelpers` components for custom inspectors.
* Avoid introducing `object`-typed parameters or shared context bags—maintain strong typing.
* Provide XML documentation for public types and members.
* Observe the dependency inversion principle; prefer interfaces and abstractions for extensibility.

---

## Commit messages

* Use concise, present-tense messages (e.g., `Add WaveTagHandler inspector`).
* Reference issues when applicable (`Fixes #123`).

---

## License

By contributing, you agree that your contributions will be licensed under the MIT License. See [`LICENSE`](LICENSE) for details.
