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

Card changes (2026-09-18).

- Bar Brawl: adds 3 Muddles (was 2); the upgrade also upgrades the Muddles.
- Easy Mark: 1 Energy (was 2), +25% damage to Confused enemies, upgraded +50%.
- Dutch Courage: now Uncommon and costs 1 (was Rare, 2).
- Pickled: 5 (9) base Block, up from 4 (8).
- New Uncommon Skill, Double Down (1 Energy): double your Intoxication and add an Everclear (+) into your hand.
- Removed: Dizzy Spell, Pregame.
- Empties: now Rare, and deals 2 (3) additional damage per Ingredient in your exhaust pile, up from 1 (2).
- Jungle Juice: applies 2 (3) Confusion, up from 1 (2).
- Still reworked: draw 1 additional card each turn, and at the start of your turn a random card in
  your hand is transformed into a random Ingredient (was: add 1 random Ingredient per turn).
- Fix: cards that cost Intoxication now deal the damage (and Block) their preview shows. Paying the
  cost used to drop you out of Tipsy or Drunk before the hit landed, losing that band's Strength.

Compatibility.

- Intoxication-cost cards (Hurl, Cold Water, Rimshot and others) can no longer be played without
  enough Intoxication when another mod overrides the game's resource check.

Not player-facing (leave out of the note): `intox` dev-console command; local builds are stamped
`v<count + 1>-dirty` so they outrank the subscribed Workshop copy.
