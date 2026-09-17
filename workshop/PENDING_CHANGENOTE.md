# Pending Workshop change note

Changes committed but not yet released. When releasing, condense this into `workshop.json`'s `changeNote`
(one paragraph, Steam BBCode allowed), run `upload.sh`, then clear this file.

## Since v0.1.38 (2026-09-17)

Balance pass.

- Liquid Courage: 6 base damage (was 4), and the upgrade now doubles its scaling to 2 damage per
  Intoxication instead of adding base damage and Intoxication; it always grants 2 Intoxication.
- Molotov: 14 (18) damage, up from 12 (16).
- Walk It Off: 2 (3) Block each time you gain, spend or lose Intoxication, up from 1 (2) on losses only.
- Muddler: 2 damage per Ingredient played, up from 1.
- Tolerance reworked: the first time you are Drunk each turn, gain 2 (3) Energy. Starting the turn
  Drunk or getting there mid turn both count, and it only pays once per turn. It no longer affects
  random card costs, and the upgrade no longer grants Intoxication.

Compatibility.

- Intoxication-cost cards (Hurl, Cold Water, Rimshot and others) can no longer be played without
  enough Intoxication when another mod overrides the game's resource check.

Not player-facing (leave out of the note): `intox` dev-console command; local builds are stamped
`v<count + 1>-dirty` so they outrank the subscribed Workshop copy.
