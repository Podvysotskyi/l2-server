# Physical damage research note

- Status: Unresolved; suitable for implementation design, not yet an accepted fixture
- Primary reference: L2J Mobius CT_0 Interlude at
  `74b5756d7a1df020f715ffb89fcce306ef0d7de8`
- Secondary cross-check: aCis 409 at
  `55ff8a4ec7e186d9816cd549246b4cf1f59c9f12`

## Source locations

Mobius:

- `L2J_Mobius_CT_0_Interlude/java/org/l2jmobius/gameserver/model/actor/Creature.java`
  selects hit, shield, critical, soulshot, and integer-damage inputs.
- `L2J_Mobius_CT_0_Interlude/java/org/l2jmobius/gameserver/model/stats/Formulas.java`
  contains `calcPhysDam` and its trait, attribute, PvP, PvE, shield, position,
  and random-damage modifiers.

aCis:

- `aCis_gameserver/java/net/sf/l2j/gameserver/model/actor/attack/CreatureAttack.java`
  selects the equivalent attack inputs.
- `aCis_gameserver/java/net/sf/l2j/gameserver/skills/Formulas.java` contains
  `calcPhysicalAttackDamage`, elemental and position modifiers.
- `aCis_gameserver/java/net/sf/l2j/gameserver/model/actor/Creature.java` defines
  the weapon random-damage multiplier.

## Shared behavior

Both references model an ordinary physical hit from these inputs:

- effective physical attack and defense;
- shield result, including a perfect block that produces one damage;
- critical and soulshot state;
- attacker position relative to the target;
- weapon random range;
- attack/defense traits, elements, weapon vulnerabilities, and NPC race where
  applicable;
- PvP/PvE modifiers.

For a non-critical front attack with every optional modifier equal to one, no
shield, and no randomness, both implementations reduce to a constant multiplied
by physical attack and divided by physical defense. The constant is not yet
accepted because the references disagree.

## Material differences

- Mobius uses `76 * attack / defense` in the base physical-damage branch; aCis
  uses `77 * attack / defense`.
- Mobius labels its critical branch as a High Five formula. This is evidence of
  cross-chronicle behavior and must not be assumed to be retail Interlude.
- Mobius applies its random-damage multiplier unconditionally and may apply it a
  second time when the relevant randomization setting is enabled. aCis applies
  one random multiplier.
- The two sources place soulshot multiplication at different stages. That can
  change rounding and additive-critical behavior even when the nominal boost is
  two.
- Position multipliers differ, especially for critical side and rear attacks.
- Mobius includes class-balance arrays and other configurable PvP/PvE modifiers
  that are custom policy, not an Interlude compatibility requirement.

## Implementation boundary

The .NET combat model should expose the calculation as ordered, independently
testable stages: effective stats, shield result, base damage, soulshot, critical,
position, traits/elements, randomization, contextual PvP/PvE modifiers, and final
clamping/rounding. Do not reproduce either Java method as one monolithic port.

Before accepting numeric fixtures, resolve the base constant, critical formula,
randomization count, position multipliers, and rounding order using additional
retail evidence or an explicit project decision. Each accepted fixture must cite
that decision and use deterministic random input.
