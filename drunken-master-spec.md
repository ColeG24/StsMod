# The Drunken Master — Character Spec v0.2

Target: STS2 character mod, BaseLib 3.4.7+, game version 0.111 (beta branch).
Status: this document describes **what is built** as of 2026-09-13, plus the open design questions.
Where the code and an older decision disagree, the code wins and the change is dated inline.
Build notes, gotchas and file locations live in `DrunkenMaster/DEV_NOTES.md`.

---

## 1. Concept in one paragraph

The Drunken Master fights by brewing drinks mid-combat and then drinking them. Playing
**Ingredient** cards fills a **Brew** (a 3-slot pot). When the pot is full it automatically seals
into a **Concoction** — a real potion that goes into a potion slot, composed of the three
ingredients' effects stacked. Brewing **Ethanol** into a potion and drinking it grants
**Intoxication**, a banded resource that powers his payoff cards and, at high bands, randomizes card
costs. Sober is when you brew; drunk is when you spend. The two halves of the character deliberately
can't run at the same time.

---

## 2. Character stats

| Property | Value |
|---|---|
| `StartingHp` | 72 |
| Potion slots | 5 (3 base + 2 from Tavern Rag on pickup; the game hard-codes 3 with no character override) |
| `Gender` | Masculine |
| Starting relic | Tavern Rag |
| Base class | `PlaceholderCharacterModel`; own static combat sprite, select portrait and head icon; rest site / merchant / energy counter still Ironclad placeholders |

Base-game comparison (decompiled 0.111): Ironclad 80, Defect 75, Regent 75, Silent 70, Necrobinder 66.

---

## 3. Starting deck (10 cards)

| # | Card | Type | Cost | Text | Upgraded |
|---|---|---|---|---|---|
| 4 | Strike | Attack | 1 | Deal 6 damage. | Deal 9 damage. |
| 4 | Defend | Skill | 1 | Gain 5 Block. | Gain 8 Block. |
| 1 | Free Pour | Skill | 1 | Gain 6 Block. Add a random Ingredient into your hand. | Gain 9 Block. Choose 1 of 3 Ingredients instead. |
| 1 | Hard Liquor | Attack | 2 | Deal 12 damage. Add an Ethanol into your hand. | Deal 16 damage. The Ethanol is Upgraded. |

Notes:
- Hard Liquor is the Bash slot. It is the starter deck's **only** Intoxication source, and only
  indirectly: the Ethanol has to be brewed and the potion drunk.
- Archaic Tooth turns Hard Liquor into **Cask Strength** (Ancient Attack, 1 Energy: deal 20 (30) damage, add 2
  Ethanol (+) into your hand; 2026-09-16, the Bash -> Break pattern). Dusty Tome still gives Top Shelf.
- Liquid Courage was the starting Intoxication card until 2026-09-12; it is now a Common.
- Free Pour's 1-of-3 choice moved to the upgrade on 2026-09-12 so the base card stays quick. Block
  raised from 5 (8) to 6 (9) on 2026-09-13 so it sits at the Common block benchmark, not the basic.

**Starting relic — Tavern Rag:** At the start of combat, add 1 random Ingredient into your hand.
Upon pickup, gain 2 potion slots.

Changed 2026-09-12 from "every turn" to "start of combat" — **decided**: every turn was judged too
strong and stepped on Still and Bar Tab. Known consequence (2026-09-13 review): the starter deck
generates roughly one Ingredient per turn and seals its first potion around turn 3, so Act 1 hallway
fights show little of the engine. The 2026-09-13 Common batch (attacks that brew) is the intended fix,
not a Rag change.

---

## 4. The Brew system

### Rules

1. **Brew** is a pot with a **capacity of 3**, visible in combat UI above the draw pile.
   Capacity is per player and clamped to 1–5: Stockpot adds 1 per stack, Shot Glass removes 1.
2. Playing an Ingredient card adds its effect to the Brew. Ingredients have **no immediate effect**.
3. When the pot reaches capacity it **automatically seals** into a Concoction and empties.
4. The Concoction goes into a free potion slot. **If there is no free slot, the Brew holds at
   capacity** and Ingredients are unplayable (greyed out) until a slot frees up. The pot seals itself
   the moment a potion is drunk or discarded, after every "whenever you drink" power has run (so Chaser
   gets its slot back first). Entropic Brew refills its own slot, so the pot keeps waiting.
5. The Brew **does not survive combat.** Unsealed Ingredients are lost at end of combat.
6. There is no manual seal/bottle command.

### Expiry

A Concoction still held when combat ends **vanishes** and its slot frees up. Nothing brewed can be banked
for a boss, and no per-instance state crosses a save (§8). The potion text says so ("Vanishes at the end
of combat") and the Brew tooltip repeats it. Until 2026-09-19 a leftover Concoction became **Dregs**, a
stateless potion that let you choose 1 of 3 Ingredients into your hand; the user cut the concept.

### Concoction composition

The Concoction's effects are the **additive sum** of its Ingredients. No recipe table. The potion's
description is the Ingredient brew texts stacked, with duplicates collapsed by (Ingredient, upgraded):
Rotgut + Rotgut + Muddle reads *"Deal 10 damage. Gain 4 Block."*

Two Ingredients modify the others instead of adding an effect:
- **Everclear**: the other Ingredients trigger twice. Stacks additively. Upgraded: still twice, but Retain instead of
  Ethereal (2026-09-15; tripled until then).
- **Seltzer**: enemy-facing effects hit ALL enemies. Upgraded: Retain instead of Ethereal (2026-09-15; the ally splash
  it used to add did nothing in singleplayer and is gone).

### Targeting

- Effects resolve to their **natural recipient**: damage and debuffs to the target, Block and buffs
  to the drinker.
- A Concoction is **targeted** if it contains any enemy-facing Ingredient, **untargeted** otherwise.
  `TargetType` is **per-instance, not per-model**.
- Brewed potions never *target* an ally. Seltzer+ is the single exception that *splashes* buffs to
  allies (added 2026-09-12; the original "no ally-facing effects ever" rule was relaxed for it).

---

## 5. Ingredient tokens

Ingredients are **generated token cards**, not cards you draft into your deck.

| Property | Value |
|---|---|
| Cost | 0 |
| Type | Skill, rarity Token |
| Keywords | Exhaust, Ethereal |
| On play | Adds its effect to the Brew. No immediate effect. |
| End of turn | Unplayed Ingredients Exhaust (Ethereal). |
| Generation | Only by the mod's own generators. Never by "add a random card" effects. |
| Upgrades | Real `OnUpgrade`s. Top Shelf (Ancient Power) upgrades every Ingredient you create; Restock+ / Hard Liquor+ / Sour Punch+ / Mash+ / Wake-Up Call+ hand out Upgraded ones. |

Random pool (10 in singleplayer, 11 in co-op; the original plan was 5 "for learnability" — grew on 2026-09-12, Jungle Juice added 2026-09-13, Punch Bowl 2026-09-15):

| Ingredient | Effect contributed | Upgraded | Enemy-facing |
|---|---|---|---|
| Rotgut | Deal 5 damage. | 8 | yes |
| Muddle | Gain 4 Block. | 7 | no |
| Bitters | Apply 1 Vulnerable. | 2 | yes |
| Wormwood | Apply 1 Weak. | 2 | yes |
| Jungle Juice | Apply 2 (3) Confusion. (1 (2) until 2026-09-18.) | 2 | yes |
| Hair of the Dog | Draw 1 card. | 2 | no |
| Grain Spirit | Gain 1 Energy. | 2 | no |
| Ethanol | Gain 2 Intoxication. | 3 | no |
| Everclear | Other Ingredients trigger twice. | Retain, not Ethereal | — |
| Seltzer | Brew hits ALL enemies. | Retain, not Ethereal | — |
| Punch Bowl | Brew's buffs reach every ally. Co-op only: in the random pool only when the run has 2+ players. | Retain, not Ethereal | — |

Because they're free tokens, they can be pure pot-fillers with no immediate payoff — the usual
"cards that do nothing on play are a tempo hole" rule doesn't apply to cards that cost no energy
and no deck slot. Ingredients in the exhaust pile are a resource (Leftovers, Empties, Scrape the Barrel).

---

## 6. Intoxication

A BaseLib **CustomResource** (not a power, not a keyword), shown as a dial above the character with
the band name under it. Cards can **cost** Intoxication (`SetCanonicalCost` / `SetXCost`); unaffordable
ones are greyed out and the cost badge shows the price.

| Rule | Value |
|---|---|
| Start | **3** at the start of every combat, the top of Sober (2026-09-15; was 0). |
| Gain | Ethanol (+3, +4 upgraded, when the potion containing it is drunk; was +2 / +3 until 2026-09-14) and specific cards. **Drinking a potion grants nothing by itself** (2026-09-12). |
| Decay | By band, at the start of your turn (2026-09-15; was a flat 1): **Sober 0, Tipsy 1, Drunk 2**. Sober is the resting state and 3 is where the dial settles. Decay and per-turn sources (Bar Tab, Nightcap) are summed and applied as **one** change after the energy reset and before the draw, so the band never flickers and drawn cards see the real band. Bar Tab (2/turn) therefore pins you at exactly 8; Bar Tab+ climbs 1/turn and Blackouts about every 4 turns. |
| Persistence | Resets at end of combat. |
| Max | 12 (Blackout). |

### Bands (4 / 8 / 12 — **decided** 2026-09-12; the v0.1 plan of 3 / 6 / 9 was rejected as too easy)

| Band | Range | Effect |
|---|---|---|
| Sober | 0–3 | No bonus; Intoxication does not fade. Sober-gated: Steady Hands, Sober Strike. (Pregame was cut 2026-09-18.) |
| Tipsy | 4–7 | You have 1 Strength and 1 Dexterity (real powers; 2026-09-15, was 2 / 2 from 2026-09-14, hidden +2 damage / +2 Block before that). |
| Drunk | 8–11 | You have 2 Strength and **-1** Dexterity as the band total, not on top of Tipsy (2026-09-15). Cards you draw get a random cost 0–3 **for this turn**. Entering Drunk re-rolls the hand. Dropping below Drunk restores the costs. |
| Blackout | 12 | See below. |

Every band change shows a full-screen banner and pops the dial.

Cost randomization is "this turn only" (2026-09-12; was "until played"). The deliberate structural
tension stands: randomized costs sabotage the brew engine, so you accumulate sober, cash out drunk,
sober up, repeat.

### Blackout

**Automatic** (changed from the v0.1 "player may trigger" rule): if you are at 12 at the end of your
turn, Blackout fires.

- Plays the top **3** cards of your draw pile.
- Your hand is discarded as normal (2026-09-14; it used to be Exhausted).
- Resets Intoxication to 0.
- Applies **Hungover** for the next turn only: 1 less Energy, draw 1 fewer card. It ticks off at the
  end of that turn (a counter in code, but since Blackout resets Intoxication to 0 a second Blackout
  lands after it is gone; in practice it never stacks).

Design note kept from v0.1: the auto-trigger risk is that every point of Intoxication becomes a step
toward a turn you didn't want. Mitigated so far by the high threshold and by 1/turn decay; revisit if
players start avoiding potions.

---

## 6b. Confusion (enemy debuff, added 2026-09-13)

A one-turn debuff the Drunken Master puts on enemies. While an enemy has N Confusion, each hit of its
attacks deals N less damage, and the damage taken off the hit is dealt to the enemy itself instead.
The self-hit never exceeds what the hit would have done: an enemy attacking 2x3 with 1 Confusion does
2x2 to you and 2x1 to itself; with 5 Confusion it does 2x0 to you and 2x3 to itself.

- Applied after the enemy's own modifiers (Strength, Weak) and before yours (Vulnerable), so being
  Vulnerable raises what you take but not what the enemy takes.
- All stacks fall off at the end of the enemy's turn, however many were applied.
- The self-hit is Unpowered and blockable (Thorns rules).
- Sources: Barstool Swing (1 to ALL enemies), Corkscrew, Spiked Drink, Jungle Juice Ingredient.

---

## 7. Card pool (as built, 2026-09-13)

Totals as of 2026-09-15 late: 4 Basic, 20 Common (11 Attacks / 9 Skills), 35 Uncommon, 22 Rare (7 Attacks / 9 Skills / 6 Powers), 1 Ancient = 82 (target 88: 4 / 20 / 36 / 26 / 2). The tables below are the 2026-09-13 snapshot plus the rows touched since; DEV_NOTES.md lists the later batches.
Numbers are base (upgraded). "+N Intox" in the cost column is an Intoxication cost.

### Common (20)

| Card | Type | Cost | Text |
|---|---|---|---|
| Liquid Courage | Attack | 1 | Gain 2 Intoxication. Deal 6 damage, +1 (2) for each Intoxication. (2026-09-17; was gain 2 (3), 4 (6) base, flat +1.) |
| Hurl | Attack | 0 + 3 Intox | Deal 14 (18) damage. |
| Barstool Swing | Attack | 1 | Deal 2 (3) damage 3 times to ALL enemies. Apply 1 Confusion to ALL enemies. |
| Corkscrew | Attack | 2 | Deal 13 (16) damage. Apply 2 (3) Confusion. |
| Upper Deckie | Attack | 0 | Deal 3 (5) damage. Increase the damage of ALL Upper Deckie cards by your Intoxication this combat (lands after this play). |
| Sour Punch | Attack | 1 | Deal 9 (12) damage. Add a Bitters (+) into your hand. (Uncommon with a fixed 1 Vulnerable from 2026-09-14 to 2026-09-15.) |
| Mash | Attack | 1 | Deal 7 (10) damage. Add a Muddle (+) into your hand. |
| Wake-Up Call | Attack | 1 | Deal 10 (12) damage. Add a Hair of the Dog (+) into your hand. |
| Sober Strike | Attack | 1 | Deal 9 (11) damage. If you are Sober, draw 1 (2) cards. Carries the Strike tag. (2026-09-15) |
| Rimshot | Attack | 0 + 3 Intox | Deal 8 (12) damage. Apply 1 Weak and 1 Vulnerable. (2026-09-15; merges Salt the Rim and Water It Down, priced off Regent's Falling Star at 1 Star = 1.5 Intoxication.) |
| Spit Take | Attack | 0 + 2 Intox | Deal 9 (12) damage to ALL enemies. (2026-09-15) |
| Coaster | Skill | 1 | Gain 7 (10) Block. Add a Rotgut (+) into your hand. (2026-09-15) |
| Cold Water | Skill | 0 + 2 Intox | Gain 7 (10) Block. |
| Sway | Skill | 1 + 1 Intox | Gain 8 (11) Block. Draw 1 (2). |
| Beer Jacket | Skill | 2 | Gain 3 (4) Intoxication, then 12 (16) Block. |
| Nightcap | Skill | 1 | Gain 2 Intoxication. Next turn, gain 3 (4). (1 now until 2026-09-15 night.) |
| Leftovers | Skill | 1 | Exhaust (upgrade removes). Gain 4 (7) Block. Put an Ingredient from your exhaust pile into your hand. |
| Restock | Skill | 1 | Add 2 random (Upgraded) Ingredients into your hand. |
| Distill | Skill | 1 | Transform a card in your hand into a random Ingredient (Upgraded: an Upgraded one). (2026-09-15; the upgrade used to cut the cost and the card briefly gave 2 Intoxication.) |
| Slip a Mickey | Skill | 1 | Apply 1 (2) Weak. Gain 5 (8) Block. Add a Wormwood into your hand. |
| Spiked Drink | Skill | 1 | Gain 7 (10) Block. Apply 1 (2) Confusion. |

### Uncommon (30)

| Card | Type | Cost | Text |
|---|---|---|---|
| Boilermaker | Attack | 2 | Deal 14 (20) damage. Add a Bitters and a Wormwood into your hand. |
| Bottle Smash | Attack | 1 + 1 Intox | Deal 10 (13) damage. Apply 1 (2) Weak and 1 (2) Confusion. (Common until 2026-09-15.) |
| Stir the Pot | Skill | 1 | Gain 2 (3) Intoxication. Add a random Poised card from your draw pile into your hand; upgraded, you choose it. (2026-09-18 rework; was an Attack: 10 (13) + draw per Concoction brewed this turn.) |
| Molotov | Attack | 2 | Gain 3 Intoxication, then deal 14 (18) damage to ALL enemies. (12 (16) until 2026-09-17.) |
| Scrape the Barrel | Attack | 2 | Deal 4 damage 4 times. Put 1 (2) random Ingredient from your exhaust pile into your hand. |
| Staggering Blow | Attack | 2 | Deal 15 (20) damage. Gain 1 (2) Block for each Intoxication. |
| Knock One Back | Skill | 0 | Gain 2 (3) Intoxication. Draw 1 card. |
| Pick-Me-Up | Skill | 0 + 3 Intox | Gain 1 (2) Energy. |
| Bouncer | Skill | 3 | Gain 13 (17) Block. Costs 1 less for each Ingredient played this turn. |
| Sweat It Out | Skill | X Intox | Gain 2 (3) Block for each Intoxication spent. |
| Line 'Em Up | Skill | 0 | Exhaust. Gain 1 Energy for each Ingredient in your hand. (Upgrade: Retain.) |
| Bar Tab | Power | 1 | At the start of your turn, gain 2 (3) Intoxication. |
| Iron Liver | Power | — | Whenever you drink a potion, gain 3 (5) Block. |
| Steady Hands | Power | — | While Sober, whenever you play an Ingredient, draw 1 card. |
| Beer Muscles | Power | 1 | Whenever you drink a potion, gain 1 Strength. (Upgrade: Innate.) |
| Barback | Power | — | Whenever you create an Ingredient, gain 2 (3) Block. |
| Stockpot | Power | 2 | Your Brew holds 1 more Ingredient. Gain 2 (3) Strength and 2 (3) Dexterity. (2026-09-19: Bulk Up mirror; was 1 (0) cost, capacity only, never picked.) |
| Shot Glass | Power | 1 (0) | Your Brew holds 1 fewer Ingredient. Ingredients cost 1 more Intoxication to play. (2026-09-19 rework; was +1 (2) Intoxication per Concoction drunk. The tax stacks per copy; direct-to-pot effects such as Cellar Raid bypass it.) |
| Drunken Strike | Attack | 1 | Deal 8 (10) damage. Hits twice if you are Tipsy or above. Strike tag. (Was Rare; was "Drunken Fist" until 2026-09-15.) |
| Cheap Shot | Attack | 0 | Deal 5 (7) damage. Apply 1 (2) Confusion. |
| Soda Gun | Attack | 1 | Deal 8 (11) damage to ALL enemies. Add a Seltzer (+) into your hand. |
| Stir Crazy | Attack | 1 | Deal 6 (8) damage. Deal it again once for each Ingredient in your Brew. (2026-09-19: base hit added; was per-Ingredient only, 0 hits on an empty pot.) |
| Double Vision | Skill | 0 | Exhaust. Apply 6 (9) Confusion. |
| Spin the Bottle | Skill | 0 + 3 Intox | Apply 3 (5) Confusion to ALL enemies. |
| Double Down | Skill | 1 | Double your Intoxication. Add an Everclear (+) into your hand. (2026-09-18) |
| Dutch Courage | Power | 1 | Whenever you Blackout, gain 3 (4) Strength. (Rare at 2 Energy until 2026-09-18.) |
| Blow Smoke | Skill | 2 | Gain 10 (13) Block. Apply 4 (6) Confusion. |
| Order Up | Skill | 1 | Gain 7 (10) Block. Next turn, add a Grain Spirit into your hand. |
| Numb | Skill | 1 | Gain 4 (7) Block. If Tipsy or above, take half damage from attacks until your next turn. |
| Karaoke Night | Power | 1 | At the start of your turn, apply 1 (2) Confusion to ALL enemies. |

### Rare (11)

Blackout Form (Power 3, Ethereal, upgrade removes Ethereal; 2026-09-18: start each turn at 12 Intoxication, so every turn ends in a Blackout), Empties (Attack 1: 8 damage, +2 (3) per Ingredient in your exhaust pile; Uncommon at +1 (2) until 2026-09-18), Still (Power 2 (1): draw 1 additional card each turn; at the start of your turn choose a non-Ingredient card in hand to become a random Ingredient; 2026-09-18 rework, was 1 random Ingredient per turn), Chug (Skill 1: 5 (7) Block; twice if Tipsy, 4 times if Drunk; Poised; Common until 2026-09-15 evening), Last Round (X Intox: 6 (8) damage X times), Open Bar (1 (0), Exhaust: fill the Brew with random
Ingredients), Cellar Raid (3 (2), Exhaust: fill your hand with random Ingredients), Lights Out (0, Exhaust: gain 9 (12) Intoxication; the draw was dropped 2026-09-15), Last Call (0,
Exhaust: +3 Intoxication, draw 2 (3)), Chaser (next potion drunk twice), Moonshiner (Power, 2: whenever the Brew seals, 3 (4) damage to ALL enemies per Ingredient
in it), Bottomless Cup (Power, 2 (1): the first potion you drink each turn, +1 Energy and draw 1), Tolerance (Power, 2: the first time you are Drunk each turn, gain 2 (3) Energy; once per turn, so dipping out and back in pays nothing;
2026-09-17 rework, was a clamp on random costs while Drunk). Drunken Fist moved to Uncommon on 2026-09-13 and became Drunken Strike on 2026-09-15.
Added 2026-09-15 late: Bar Brawl (Attack 3: 23 (28) + 3 Muddles (+) since 2026-09-18; up from Uncommon), Kitchen Sink (Attack 2: 14 (16) + 4 (5) per
Ingredient played this turn), Clear the Bar (Attack 0: can only be played by a Blackout; 50 (65) to ALL), Haymaker (Attack 1: 8 (11),
triple damage while Drunk), Drink to Forget (Skill 0 + 5 Intox, Exhaust: discard your hand, draw 5 (7)), Muddle Through (Skill 2, Exhaust: transform any number of cards in
your hand into Muddles (+)). Rare is 22. 2026-09-18: Dizzy Spell cut, Dutch Courage down to Uncommon (Rare 23 in total with the 09-16 additions); Easy Mark 1 Energy, +25% (50%); Pickled 5 (9) base Block.

Pool rules (2026-09-12): **no Common Powers**, and the Rare pool must keep **at least four Powers**
(Lasting Candy gotcha, see DEV_NOTES). No Harmony patches on base-game relics — fix content instead.

### Ancient (1)

Top Shelf (Power, 2 (1), Innate): your Ingredients are Upgraded. Required: Darv's Ancient event
crashes without an Ancient-rarity card in the pool.

---

## 8. Benchmarks to build against

Decompiled from 0.111. Every base character has exactly 20 Commons, none of them Powers, none of
them unplayable without a resource.

- 1-cost Common attack: **8–10 damage** single-target with a rider, ~6–9 AoE. Basic Strike is 6.
- 1-cost Common block: **6–9** with a rider. Basic Defend is 5.
- Common attack share: Ironclad 13/20, Defect 12, Necrobinder 12, Silent 9, Regent 9.
- Every base pool has 3–5 Common draw cards.
- Upgrade deltas: attacks +2/+3 at 1 cost; block +3 almost universally.
- Full pool: 4 Basic + 20 Common + 36–38 Uncommon + 27 Rare + 2 Ancient ≈ 90.
- **A potion-generating character's cards should sit slightly under benchmark** — potion output is
  entirely off the energy budget. "Slightly under" means under the Common benchmark, not under the
  basic Strike/Defend.

Review findings (2026-09-13) that drove the 2026-09-13 batch: the Common pool was 4 Attacks / 10
Skills with 5 of 14 gated on Intoxication, one ungated single-target attack, and no draw. The batch
added five ungated attacks (three of which brew), a draw attack, a 0-cost Intoxication source, and a
cost-reducing block card, and raised Liquid Courage to +2 so it is net positive against decay.

---

## 9. Technical notes

### Stack
- Godot 4.5 (MegaDot 4.5.1-m.14) / .NET 9 / HarmonyX / first-party loader (`MegaCrit.Sts2.Core.Modding`)
- BaseLib v3.4.7+ (`Alchyr.Sts2.BaseLib`), Workshop id 3737335127

### Classes used
| Thing | Base class |
|---|---|
| Character | `PlaceholderCharacterModel` |
| Cards, Ingredient tokens | `CustomCardModel` via `DrunkenMasterCard` (carries the `[Pool]` attribute) |
| Concoction | `CustomPotionModel` |
| Character potion pool | `CustomPotionPoolModel` |
| Card pool | `CustomCardPoolModel` |
| Tavern Rag | `CustomRelicModel` |
| Powers (Hungover, Bar Tab, Confusion, …) | `CustomPowerModel` |
| Confusion damage redirect | Harmony postfix on `Hook.ModifyDamage` (`Patches/ConfusionDamagePatch.cs`) |
| Intoxication | `CustomResource` (auto-registered by BaseLib) |
| Brew zone | static `BrewSystem` keyed on `PlayerCombatState` via `SpireField`; drawn by `NBrewDisplay` |
| Band effects, Blackout | `CustomSingletonModel(HookType.Combat)` |

### Constraints
- Registration is **reflection-based**; the ID slug derives from the class name
  (`LiquidCourage` → `LIQUID_COURAGE`). There is no "register card" call.
- Canonical model instances are **immutable**, guarded by `AssertMutable()`.
- Anything numbered (Intoxication) must be a resource/power/DynamicVar with a tooltip; BaseLib
  keywords are numberless single words.
- `dotnet publish` (not `build`) is required for **any** change to text, images, scenes, or
  localization.
- `ancients.json` is required or the project won't compile.
- Multiplayer: `affects_gameplay: true`.

### Per-instance state (resolved 2026-09-12)

A Concoction's Ingredient list, description and `TargetType` are composed at runtime via `SpireField`.
Rather than round-trip that through the save file, Concoctions are **combat-scoped**: anything still in
a slot at end of combat is discarded. Nothing with per-instance data crosses a save.
Co-op sync of the composed description is untested.

---

## 10. Open questions (do not guess — flag and ask)

1. **Sober band bonus.** Partly answered 2026-09-15: combat opens at 3, Intoxication does not fade while Sober, and
   Sober Strike (9 damage, draw 1 while Sober) is the first Sober payoff. A brewing-side bonus (choose 1 of 2
   Ingredients, pot capacity +1) is still open.
2. **Full-slot overflow.** The Brew holds and blocks. Consider a softening effect (e.g. gain Block
   instead of stalling) since sitting at full slots will be a common state. Unresolved.
3. **Matching-ingredient bonus.** Three of the same Ingredient producing an amplified potion. This is
   what makes mixed potions a consolation prize rather than a design failure. Decide before the pool
   grows further.
4. **Ingredient selection.** Partly answered: Free Pour+ is choose-1-of-3; Hard Liquor,
   Slip a Mickey, Sour Punch, Mash, Wake-Up Call and Boilermaker grant specific Ingredients. The
   random generators (Rag, Restock, Still, Open Bar, Cellar Raid) stay random.
5. **Blackout card source.** Decided: draw pile.
6. **Decay vs. gain math.** Decided: direct sources grant 2–3 (Liquid Courage 2, Beer Jacket 3,
   Knock One Back 2, Molotov 3, Nightcap 1 + 3); Bar Tab is 2 base. Decay is now per band (0 / 1 / 2), so holding
   Tipsy costs 1/turn and holding Drunk 2/turn. Liquid Courage from the new start of 3 is 14 on its first play and
   Chug went to Rare the same evening (5 (7), x2 Tipsy, x4 Drunk); Liquid Courage went to +1 per Intoxication that night (6 to 16).
   Pricing rule for Intoxication-cost cards (2026-09-15): compare to Regent's Star cards at 1 Star = 1.5 Intoxication,
   because Intoxication comes in at about twice the rate per card (Liquid Courage 2 vs Solar Strike 1, Knock One Back
   2 for 0 Energy vs Glow 1 for 1, Bar Tab at Uncommon vs Genesis at Rare) and decay only bites above 3.
7. **Potion slot count.** Decided: relic fallback (Tavern Rag +2 on pickup). A Harmony patch on
   `Player` construction is the alternative if Neow relic swaps prove awkward.
8. **Band thresholds.** Decided: 4 / 8 / 12. Drunk and Blackout are rarely reached before Act 2
   under the current Common economy; that is accepted for now, revisit only with playtest data.
9. **Tavern Rag cadence.** Decided: once per combat (§3).
10. **Bouncer's rarity.** Decided: Uncommon (2026-09-13).
11. **Cellar Raid.** Resolved 2026-09-16 the other way: Open Bar was cut, Cellar Raid stays (judged the more interesting of the two).

---

## 11. Milestones

1. ~~Playable run on vanilla art with the starter deck, Rag, Ingredients, Brew, Intoxication, Concoction.~~
2. ~~Persistence spike.~~ Resolved by combat-scoped Concoctions.
3. **Current:** fill the pool toward 20 / 36 / 26 / 2 with the §8 benchmarks; gated cards
   (Sober-only / Tipsy+ / Drunk+ / Hungover-only); a real Intoxication icon; card art for the
   2026-09-12 and 2026-09-13 batches.
4. Resolve §10 items 1, 3, 8 and 9 through playtesting.
