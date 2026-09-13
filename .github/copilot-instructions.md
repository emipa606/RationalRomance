# GitHub Copilot Instructions for [RF] Rational Romance (Continued)

## Mod Overview and Purpose

"Rational Romance (Continued)" is a mod for RimWorld that overhauls and enhances the game's romance system. It aims to address several issues in the vanilla game's romance mechanics while adding new features that provide a more comprehensive and realistic portrayal of relationships. By doing so, it serves as a spiritual successor to SeveralPuffins' "Romance Diversified" mod.

## Key Features and Systems

1. **Sexual Orientations**: All pawns receive a trait indicating their sexual orientation—such as asexual, bisexual, gay, or straight—which is assigned after regular trait assignment to avoid using up a regular trait slot.

2. **New Traits**: Introduces "Faithful" and "Philanderer" traits that modify a pawn's likelihood of cheating in relationships.

3. **New Activities**: Adds activities like long walks for couples and casual hookups for singles, enhancing social interactions.

4. **Romantic Behavior Improvements**: Implements behavioral changes such as reducing romance attempts after rebuffs, altering relationship probabilities based on traits, and adjusting responses based on gender and orientation.

5. **Cultural Dynamics**: Modifies romantic interactions with a cultural component, making some groups more forward based on background.

6. **Polyamory Support**: Allows pawns to engage in relationships with multiple partners, with customizable occurrence probability through the mod options menu.

7. **Mood Penalty Adjustments**: Tunes romance-related mood penalties to prevent disproportionate reactions to romantic failures.

8. **Code Fixes**: Corrects existing romance-related code to enhance gender equality in initiating relationships and adjusting parenthood likelihood.

## Coding Patterns and Conventions

Follow standard C# conventions with particular emphasis on:

- **CamelCase** for method and variable names.
- Use of meaningful names to reflect the function or purpose.
- Comment your code generously for clarity, especially around complex logic.

## XML Integration

XML files define custom game elements such as:

- **Defs**: Used for defining interaction, job, joy, and thought processes.
- **TraitDef**: To define new traits within the game.

Ensure XML structure adheres to RimWorld modding standards. 

## Harmony Patching

This mod utilizes Harmony for addressing vanilla limitations:

- **Harmony Prefix**: Used to modify or extend the function calls of existing RimWorld methods.
- Ensure patches are safe and don't conflict with other mods.
- Test patches thoroughly to ensure mod stability and performance.

## Suggestions for Copilot

When using GitHub Copilot, consider the following:

- **Generate Documentation**: When writing new functions or classes, prompt Copilot to comment on logic and intentions for clarity.

- **XML Template Creation**: Use Copilot to speed up repetitive XML structure creation.

- **Code Refactoring**: Utilize Copilot suggestions for more succinct, efficient methods.

- **Harmony Patches**: Use Copilot to generate boilerplate Harmony patch code, but verify accuracy against Harmony documentation.

- **Feature Expansion**: Leverage Copilot for prototyping new features, allowing for rapid iteration and experimentation.

- **SonarQube Issues**: When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.

---

Remember, while Copilot is a powerful tool, always verify its code suggestions align with the intended design and functionality of the mod.

## Project Solution Guidelines

- Relevant mod XML files are included as Solution Items under the solution folder named XML; these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.
- You have access to two custom MCP services: rimworld-mcp for querying game-log, harmony patches, etc., and sonarcube-mcp for fetching code maintainability issues.

## Hard rules (must follow)

- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).



