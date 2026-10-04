# Comparable fan-translation projects

Reviewed 4 October 2026. These are examples of community practice, not legal
precedent or permission from Dressmaker's rights holders. The comparison is based
on the project authors' own READMEs, not an independent audit of their releases.

| Project | Relevant practices | Permission evidence in the reviewed README |
| --- | --- | --- |
| [Obero/dressmaker_fr](https://github.com/Obero/dressmaker_fr) | A public French fan translation for the same game/build. States it is unaffiliated, requires game ownership, excludes original English text, separates translations from plugin/tools, and discloses AI assistance. | No explicit developer authorization documented there. |
| [noccu/umamusu-translate](https://github.com/noccu/umamusu-translate) | A public translation toolset/patch for another Unity game. Disclaims affiliation with Cygames and separates translation work from tooling. | Its disclaimer explicitly acknowledges that its modifications conflict with the relevant terms; useful evidence that a disclaimer alone is not permission. |
| [wanjizheng/zephyr-remastered-multilingual](https://github.com/wanjizheng/zephyr-remastered-multilingual) | A free, unofficial multilingual patch requiring a legitimate Steam copy. Credits the original game owners for screenshots, discloses AI assistance, and checks supported game resources. | No explicit developer authorization documented in the reviewed README. |

The Dressmaker French project is the closest practical comparison. Its BepInEx
plugin adds a language at runtime without rewriting game files. Our adapter
currently rebuilds local English tables and patches the local TextMeshPro assembly;
this difference is disclosed in our README and setup guide.

## What we adopted

The README now clearly identifies our project as unofficial, unaffiliated,
noncommercial, and AI-assisted. It credits the game's rights holders, requires
legitimate ownership, and explains that no game binaries or original English text
tables are distributed. The translation pack remains separate from the adapter. We also follow the French
project's split licenses: MIT for our adapter code and CC BY-NC-SA 4.0 for our
licensable translation contributions, excluding rights in the original game.
We offer a rights-holder contact route and commit to withdrawing distribution on
request or an announcement of an official Hebrew localization.

## What these examples do not establish

A public repository's existence is not proof that its rights holder authorized it,
that its terms allow it, or that it has survived legal review. An unaffiliated
notice addresses confusion about sponsorship; it does not grant a license to the
original story or override contractual restrictions.

For comparison, [Rotoscope Studios' fan policy](https://www.rotoscopestudios.com/fanpolicy)
explicitly distinguishes an unofficial notice from a permission grant. Section 6
allows translations of specified public posts, while section 6.4 excludes game
text/assets unless separately authorized. This is a different studio's policy,
not a policy governing Dressmaker, and cannot be borrowed as permission for it.

[Steam Subscriber Agreement section 2.G](https://store.steampowered.com/subscriber_agreement/)
contains default restrictions with exceptions for permissions and applicable law.
We have not established an applicable exception or Dressmaker-specific permission.
The maintainer chose to publish under the same community standard as the French
project. Publication and these notices must not be described as developer approval
or legal clearance.
