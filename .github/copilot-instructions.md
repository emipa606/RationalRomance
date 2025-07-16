# GitHub Copilot Instructions for RimWorld Mod "Rational Romance"

## Mod Overview and Purpose

The "Rational Romance" mod aims to enhance the romantic and relationship dynamics within RimWorld, a popular colony simulation game. By introducing new traits, interactions, and relationship mechanics, this mod provides a more nuanced and realistic portrayal of relationships among colonists, enhancing players' storytelling experiences.

## Key Features and Systems

- **Expanded Relationship Interactions:** The mod adds new job drivers and interaction workers that allow for richer romantic interactions, including dating, marriage proposals, and casual relationships.

- **New Traits and Thought Systems:** Introduces new traits that affect how pawns interact with each other romantically, as well as new thoughts that reflect their romantic experiences and preferences.

- **Dynamic Relationship Generation:** Utilizes systems to determine relationship chances, proposal acceptance, and the impact of breakup scenarios.

## Coding Patterns and Conventions

- **Class Design:** The mod primarily utilizes static classes and methods for utilities and interaction workers, ensuring that they are stateless and reusable across different game contexts.

- **Naming Conventions:** Classes and methods are named clearly to reflect their purpose. E.g., `JobDriver_ProposeDate` suggests this class handles the logic for proposing dates.

- **Visibility Modifiers:** Most utility classes are `public static`, facilitating easy access across different parts of the mod, while some game element classes, like `RationalRomance`, adapt `internal` or `public` as needed.

- **Methods and Logic Separation:** Methods within classes, such as `private bool isTargetPawnOkay()`, are used to manage conditions or processes within larger systems, promoting separation of concerns and reusability.

## XML Integration

XML integration in this mod involves defining new game elements such as traits, thoughts, and job definitions. XML files are critical in RimWorld for defining how these elements interact with the game. Despite the summary indicating errors in parsing, ensure that these XML definitions follow RimWorld's modding guidelines to prevent runtime issues.

## Harmony Patching

The mod uses Harmony for patching existing RimWorld functionalities to integrate new behaviors without modifying the original game code. This ensures compatibility and maintainability.

- **Patch Methods:** Use `HarmonyPatch` attributes to target specific methods in the base game.
- **Pre/Postfix Logic:** Implement prefix and postfix methods for augmenting behaviors without overwriting the original method logic.

## Suggestions for Copilot

- **Automated Trait and Thought Generation:** Utilize Copilot for generating boilerplate `Def` XML files for new traits or thoughts. Ensure placeholders for key attributes are available for easy editing.

- **Error Handling and Validation:** Implement suggestions or diagnostic tools in C# methods which verify that runtime conditions do not inadvertently break mod logic or game stability.

- **Code Refactoring:** Encourage implementing DRY (Don't Repeat Yourself) principles by suggesting common utility methods for repeated logic across different classes.

- **Documentation Comments:** Insert XML documentation comments for methods and classes to provide insights into their intended use and facilitate further community contributions.

By following these guidelines and utilizing GitHub Copilot effectively, contributors can maintain high-quality, engaging, and stable mods for the RimWorld community.