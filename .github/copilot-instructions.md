# GitHub Copilot Instructions for [RF] Rational Romance (Continued)

Welcome to the GitHub Copilot instructions for the [RF] Rational Romance (Continued) mod for RimWorld. This document provides an overview of the mod's purpose, key features, coding patterns, and integration with XML and Harmony patching. It also includes suggestions for using Copilot effectively while working on this project.

## Mod Overview and Purpose

[RF] Rational Romance (Continued) is an update and continuation of the Rainbeau Flambe mod, which overhauls RimWorld's romance system. The mod introduces new features such as orientations, new traits, romantic activities, and improved cultural interactions. It also includes support for polyamory and fixes various issues in the game's romance-related code.

## Key Features and Systems

1. **Orientations**: Pawns are assigned one of four orientations: asexual, bisexual, gay, or straight. The mod allows customization of the probabilities for each orientation.

2. **New Traits**: The mod introduces "Faithful" and "Philanderer" traits, which influence a pawn's likelihood to cheat.

3. **New Activities**: Added long walks as a date option for couples and casual hookups as a joy activity for single pawns.

4. **Improved Romantic Behavior**: Pawns are less likely to initiate romance attempts if recently rebuffed, less likely to target certain pawns, and more likely to rebuff attempts from the "wrong" gender.

5. **Cultural Component**: Romantic interactions are influenced by cultural background, affecting the likelihood of initiating encounters or proposing marriage.

6. **Polyamory Support**: Polyamorous pawns prefer relationships involving multiple partners and can be assigned a chance of being polyamorous in the mod's options menu.

7. **Adjusted Mood Penalties**: Reduced penalties and stack limits for rebuffs and romantic failures to prevent extreme mood swings.

8. **Bug Fixes**: Fixed issues such as female pawns being less likely to initiate romance attempts, gay pawns being less likely to be parents, and age preferences for partners.

## Coding Patterns and Conventions

### C# Coding Patterns

- **Mod Structure**: The mod is organized into several C# files, each responsible for specific aspects of the mod, such as trait assignment, interaction workers, and child relation utilities.
- **Harmony Patches**: Extensive use of Harmony patches to modify vanilla RimWorld behavior. Patches are used to add new features, adjust probabilities, and fix bugs.
- **Type and Member Naming**: Types and members are named descriptively to reflect their functionality, such as `InteractionWorker_RomanceAttempt_RandomSelectionWeight` and `AssignOrientation`.

### XML Integration

- **Defs**: XML files define various RimWorld elements such as interaction definitions, job definitions, joy definitions, thought definitions, and trait definitions.
- **Structure**: XML files are well-structured and organized, making it easy to add or modify definitions without disrupting the existing mod.

## Harmony Patching

Harmony patching is a core component of this mod, allowing developers to modify vanilla RimWorld code without altering the original game files. Key Harmony patches include:

- **Trait Assignment**: Modifies the trait assignment process to include orientation traits.
- **Romance Attempts**: Adjusts the probabilities of romance attempts based on orientation and other factors.
- **Breakups**: Modifies the behavior of breakups to include orientation-based rebuffs.
- **Child Relation Utilities**: Adjusts the probability of becoming a child based on various factors.

## Suggestions for Copilot

1. **Start with Existing Code**: Utilize existing C# and XML code snippets for inspiration and to ensure consistency with the mod's coding patterns.
2. **Focus on Harmony Patches**: When implementing new features, consider using Harmony patches to modify vanilla behavior, ensuring compatibility with existing mod systems.
3. **Review and Test**: After generating code, thoroughly review and test the code to ensure it integrates correctly with the mod and does not introduce bugs.
4. **Consult Documentation**: Refer to RimWorld's API documentation and modding guides for best practices and additional insights into Harmony patching and XML integration.

By following these guidelines, you can effectively use GitHub Copilot to enhance and maintain the [RF] Rational Romance (Continued) mod for RimWorld. Happy coding!

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

