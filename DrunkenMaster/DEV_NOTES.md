# Drunken Master — dev notes (v0.1 vertical slice)

## Build / publish (macOS)

The game targets .NET 9. A user-local SDK is installed at `~/.dotnet` (the system dotnet is 8).

```bash
export DOTNET_ROOT="$HOME/.dotnet"; export PATH="$HOME/.dotnet:$PATH"
cd ~/dev/StsMod/DrunkenMaster
dotnet build      # .cs changes only → copies .dll to the game's mods folder
dotnet publish    # ANY text/image/localization change → also regenerates the .pck via MegaDot
```

- MegaDot 4.5.1-m.12 (matches the game's engine build) lives at `~/dev/StsMod/tools/MegaDot.app`.
  `Directory.Build.props` points at it. Move it if you like, then update `<GodotPath>`.
- Mods folder: `Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/MacOS/mods/DrunkenMaster/`
- Logs: `~/Library/Application Support/SlayTheSpire2/logs/godot.log`
- Dev console in game: `` ` `` / `~` / `*`, then `help card` to spawn content.

## What exists

| Piece | File | Status |
|---|---|---|
| Character (72 HP) | `Character/DrunkenMaster.cs` | own combat sprite (`images/character/drunken_master.png`, static PNG via BaseLib NodeFactory), select portrait + head icon in `images/charui/`; rest site / merchant / energy counter still Ironclad placeholders |
| Strike / Defend | `Cards/Basic/` | done |
| Free Pour | `Cards/Basic/FreePour.cs` | Gain 6 Block, add a random Ingredient (upgraded: 9 Block, choose 1 of 3 via the choose-a-card screen). Was 5/8 until 2026-09-13 |
| Liquid Courage | `Cards/Common/LiquidCourage.cs` | Common since 2026-09-12, no longer a starter. 2026-09-13: grants 2 (3) Intoxication instead of 1 (2) so it beats the 1/turn decay |
| Hard Liquor (starter) | `Cards/Basic/HardLiquor.cs` | The Bash: 2 Energy, 12 damage, adds an Ethanol to hand. Upgraded 16 |
| Ethanol | `Cards/Ingredients/Ethanol.cs` | Ingredient: brewed effect is +2 Intoxication. Since 2026-09-12 Concoctions grant NO Intoxication by themselves; Ethanol is the default source. In the random pool (Rag / Still / Open Bar / Free Pour offers) |
| 7 Ingredient tokens (incl. Ethanol, Wormwood) | `Cards/Ingredients/` | done (0-cost, Exhaust, Ethereal, no immediate effect) |
| Brew (3-slot pot, seals to potion) | `Brew/BrewSystem.cs`, `Ui/NBrewDisplay.cs` | combat-scoped; pot drawn above the draw pile with mini card nodes |
| Concoction potion | `Potions/Concoction.cs` | effects + per-instance targeting; description composes ingredient texts with duplicates collapsed. **Combat-scoped**: at end of combat any Concoction still in a slot becomes Dregs (same slot) |
| Dregs potion | `Potions/Dregs.cs` | stateless (save-safe). Drink: choose 1 of 3 Ingredients into your hand |
| Batch 2026-09-12 pm | `Cards/`, `Powers/`, `Cards/Ingredients/` | Leftovers, Distill, Beer Jacket, Pick-Me-Up, Sway, Restock (Common); Cellar Raid (Rare); Stockpot / Shot Glass / Barback (Uncommon Powers; pot capacity is per-player via `BrewSystem.CapacityFor`); Everclear (doubles other ingredients) and Seltzer (splash: all enemies / all allies) resolved in `Concoction.OnUse`; Bottle Smash costs 1 Energy + 1 Intoxication for 10; Bitters/Wormwood apply 1; Chaser re-procures the potion and queues a real second use |
| Intoxication resource + dial | `Resources/IntoxicationResource.cs`, `Ui/NIntoxicationDial.cs` | BaseLib CustomResource; dial above the character; cards can cost it (`SetCanonicalCost` / `SetXCost`) |
| Band effects + Blackout | `Resources/DrunkenMasterBands.cs`, `Ui/NBandBanner.cs` | Tipsy+ : +2 card damage/+2 card block. Drunk+ : drawn cards get random cost 0-3 for this turn; entering Drunk re-rolls the hand too; dropping below Drunk restores costs. Blackout at 12: end of turn, play top 3 of draw pile, Exhaust hand, reset to 0, Hungover next turn. Every band change shows a full-screen banner (flash + shake + slosh) and the dial pops; band name sits under the dial |
| Slip a Mickey (Common: 1 Weak, 5 Block, adds a Wormwood), Empties (Uncommon: 8 dmg +1/+2 per Ingredient in exhaust), Barstool Swing (2x3 AoE) | `Cards/` | added 2026-09-12 (Knockout Drops potion was added and removed the same day) |
| Cold Water, Hurl (Common), Sweat It Out (Uncommon) | `Cards/Common|Uncommon/` | Intoxication-cost cards; greyed out when you can't afford them |
| Line 'Em Up (Uncommon Skill: 0 cost, Exhaust, +1 Energy per Ingredient in hand, upgrade adds Retain), Beer Muscles (Uncommon Power: 1 cost, +1 Strength whenever you drink a potion; upgrade makes it Innate) | `Cards/Uncommon/`, `Powers/BeerMusclesPower.cs` | added 2026-09-12; no card/power art yet |
| Top Shelf (Ancient Power, 2 cost, Innate: your Ingredients are Upgraded; upgrade costs 1) | `Cards/Ancient/TopShelf.cs`, `Powers/TopShelfPower.cs` | added 2026-09-12. This is the Dusty Tome card: Darv's Ancient event crashed (NRE in `DustyTome.SetupForPlayer`) because the pool had no Ancient-rarity card. Every Ingredient creation goes through `BrewSystem.CreateIngredient`, which upgrades when Top Shelf is up; the power's `AfterCardGeneratedForCombat` hook covers Hard Liquor / Slip a Mickey |
| Nightcap (Common Skill, 1 cost: +1 Intoxication now, +3/+4 next turn via `NightcapPower`, a one-shot `IPerTurnSource` that removes itself in `AfterTurnStartApplied`), Staggering Blow (Uncommon Attack, 2 cost: 15/20 damage, +1/+2 Block per Intoxication) | `Cards/Common/Nightcap.cs`, `Cards/Uncommon/StaggeringBlow.cs`, `Powers/NightcapPower.cs` | added 2026-09-12 |
| Molotov | `Cards/Uncommon/Molotov.cs` | 2026-09-12: moved Rare→Uncommon, Exhaust removed, gains 3 Intoxication BEFORE the 12/16 AoE hit. Same day: Hard Liquor+ hands out an Ethanol+, Liquid Courage+ also grants +1 Intoxication, Bottle Smash applies 1/2 Weak, and the "Costs N Intoxication" line was dropped from every Intoxication-cost card (the cost badge shows it) |
| Ingredient upgrades | `Cards/Ingredients/*.cs` | 2026-09-12: Ingredients have real `OnUpgrade`s. Rotgut 5→8, Muddle 4→7, Bitters/Wormwood 1→2, Hair of the Dog draw 1→2, Grain Spirit 1→2 Energy, Ethanol 2→3 Intoxication, Everclear doubles→triples (`ExtraTriggers`, stacks additively in `Concoction.Multiplier`), Seltzer base hits all enemies only, upgraded also reaches allies (`ReachesAllies`). Brew text can use `{IfUpgraded:show:a|b}`; Concoction groups lines by (Id, IsUpgraded) |
| Hungover power | `Powers/HungoverPower.cs` | applied by Blackout; next turn only (-1 Energy, -1 draw) |
| Tavern Rag relic | `Relics/TavernRag.cs` | 1 Ingredient at start of combat (was every turn); +2 potion slots (spec fallback for 5 slots) |
| Bottle Smash, Chug, Bar Tab (Common), Last Call (Rare) | `Cards/Common|Uncommon|Rare/` | placeholder reward pool |
| Rare set (2026-09-12): Still (promoted), Last Round, Drunken Fist, Open Bar, Lights Out, Chaser, Dutch Courage, Moonshiner | `Cards/Rare/`, `Powers/` | Added so the Rare pool has 4+ Powers: Lasting Candy needs the unoffered Powers to span two rarities or the rewards screen throws (see gotchas). Chaser re-runs a potion's `OnUse` via reflection; Dutch Courage listens through `IntoxicationResource.GainAsync`; Moonshiner through `BrewSystem.IBrewSealedListener` |
| Batch 2026-09-13 (Common pool review): Sour Punch (1: 8/11 dmg + Bitters(+)), Mash (1: 6/9 + Muddle(+)), Wake-Up Call (1: 10/12 + Hair of the Dog(+)), Stir the Pot (1: 7/10, draw 1 per Ingredient in the Brew), Scrape the Barrel (2: 4x4, put 1/2 random Ingredients from exhaust into hand; Uncommon since later on 2026-09-13), Knock One Back (0: +2/3 Intoxication, draw 1; Uncommon since later on 2026-09-13), Bouncer (Uncommon, 3: 13/17 Block, costs 1 less per Ingredient played this turn; Stomp pattern via `AfterCardEnteredCombat` + `BeforeCardPlayed` + `EnergyCost.AddThisTurn`); Boilermaker (Uncommon, 2: 14/20 + Bitters + Wormwood) | `Cards/Common/`, `Cards/Uncommon/` | added 2026-09-13; all have art except Boilermaker and Bouncer. Common pool was 17 (11 Attacks / 6 Skills) before Corkscrew and Spiked Drink took it to 19 and Upper Deckie to 20; Pick-Me-Up also moved to Uncommon on 2026-09-13 |
| Confusion (2026-09-13) | `Powers/ConfusionPower.cs`, `Patches/ConfusionDamagePatch.cs` | Enemy debuff, Counter, removed at end of the owner's turn (all stacks). Each attack hit deals N less and the removed part hits the owner instead, capped at the hit's own damage (2x3 with 5 Confusion: 0x3 to you, 2x3 to it). Implemented as a Harmony **postfix on `Hook.ModifyDamage`** (the one place with both base and final damage; also drives the intent preview): re-runs the hooks with `target == null` to get the dealer-side amount (Strength/Weak in, Vulnerable out), redirected = min(N, that), final scaled by `(outgoing-redirected)/outgoing`. A prefix on `CreatureCmd.Damage` (not the ModifyDamage postfix, see gotchas) stashes the per-target redirected amount on the power; `AfterDamageGiven` deals it to the owner as Unpowered damage with no dealer (Thorns-style, blockable). Applied by Barstool Swing (1 to ALL, after the hits), Corkscrew (Common Attack, 2: 13/16 + 2/3), Spiked Drink (Common Skill, 1: 7/10 Block + 1/2 to one enemy) and the Jungle Juice Ingredient (1/2, in the random pool). Power icon is a copy of the template `power.png`; the three cards have no art |
| Upper Deckie (2026-09-13, was "Zyn" until the same evening) | `Cards/Common/UpperDeckie.cs` | Common Attack, 0 cost: 3 (5) damage, then every Upper Deckie in this combat gets +damage equal to your current Intoxication (Claw pattern: `PlayerCombatState.AllCards.OfType<UpperDeckie>()`, buff lands after the hit, `AfterDowngraded` restores it). Combat-scoped, not permanent. No art yet |
| Batch 2026-09-13 pm (Confusion/Uncommon expansion) | `Cards/Uncommon/`, `Cards/Rare/Tolerance.cs`, `Powers/` | Edits: Beer Jacket 3 (4) Intox; Bar Tab 2 (3)/turn; Cold Water 0 Energy for 7 (10); Last Call 0 Energy; Drunken Fist -> Uncommon, hits twice if Tipsy+ (band-count logic removed); Dutch Courage -> +3 (4) Strength whenever you Blackout (new `DrunkenMasterBands.IBlackoutListener`, fired at the end of `Blackout()`). New Uncommons: Karaoke Night (Power 1: 1/2 Confusion to ALL at turn start, `KaraokeNightPower`), Double Vision (0, Exhaust: 6/9 Confusion), Spin the Bottle (0 + 3 Intox: 3/5 Confusion to ALL), Pregame (1: 4/6 Intox if Sober else 2/3), Cheap Shot (0: 5/7 + 1/2 Confusion), Blow Smoke (2: 10/13 Block + 4/6 Confusion; was "Haze", a Silent card name), Order Up (1: 7/10 Block + a Grain Spirit next turn via one-shot `OrderUpPower`; was 1/2 random Ingredients until 2026-09-14, upgrade is Block only now), Numb (1: 4/7 Block; Tipsy+: `NumbPower` halves attack damage until your next turn, Flame Barrier timing), Soda Gun (1: 8/11 to ALL + Seltzer(+)), Salt the Rim / Water It Down (0 + 1 Intox: 3/5 + 1/2 Vuln / Weak), Stir Crazy (1: 6/8 damage once per Ingredient in the Brew; 0 hits on an empty pot; upgrade value was my guess). New Rare: Tolerance (Power 2: while Drunk random costs never exceed the card's current cost, clamp in `IntoxicationResource.RandomizeCost` via `TolerancePower`; upgrade grants 4 Intox once on play). Rare pool stays 11 with 5 Powers. Uncommon is 30. All new power icons are the template placeholder; no card art |
| Batch 2026-09-14 (loop/overlap pass) | `Powers/`, `Cards/`, `Brew/BrewSystem.cs`, `Patches/BrewSealPatch.cs` | Bottomless Cup: first potion each turn only (`_usedThisTurn` on the power, reset in `AfterPlayerTurnStart`). Moonshiner: 3 (4) Unpowered damage to ALL enemies per Ingredient in the sealed Concoction (was +2 Energy; upgrade no longer cuts the cost). Shot Glass: 1 cost, Str/Dex dropped, +1 (2) Intoxication whenever you drink a Concoction (Counter; capacity delta stays -1). Order Up: fixed Grain Spirit, Block-only upgrade. Pot: `AddIngredient` seals before adding (the played Ingredient used to be thrown away with the seal); `IngredientCard.IsPlayable` is false while `BrewSystem.IsBlocked` (full pot, no slot); `BrewSealPatch` wraps the Task returned by `Hook.AfterPotionUsed` / `Hook.AfterPotionDiscarded` so the pot seals after Chaser re-procures, and skips when `CombatManager.IsOverOrEnding` (Dregs conversion) |
| Architect dialogue | `localization/eng/ancients.json` | placeholder lines |

## Gotchas learned

- Lasting Candy swaps a reward card for a Power *not already offered*, reusing the reward's rarity odds. If the
  candidates are a single rarity the game throws and the rewards screen soft-locks. With no Common Powers this
  means the pool needs at least four Rare Powers (a boss shows three Rares). Don't let it drop below that.

- The card pool must contain at least one non-Basic card, and ideally one Attack, one Skill and one
  Power, or the game throws `couldn't generate a valid rarity` on every card reward (post-combat,
  Lost Coffer, merchant) and the run soft-locks.
- Strike/Defend borrow Ironclad art via `PortraitPath` / `CustomPortraitPath` overrides.
- Harmony postfixes on `Hook.ModifyDamage` must guard against re-entrancy if they call `Hook.ModifyDamage` themselves (Confusion uses a static flag). Its `out modifiers` is patched as `ref`.
- **Multiplayer determinism:** `Hook.ModifyDamage` is also called by the enemy intent preview, for the LOCAL player only (`LocalContext.GetMe`). Anything a ModifyDamage hook writes to game state is therefore machine-specific and desyncs co-op (2026-09-13: Confusion's per-target self-damage stash was written there; Flail Knight HP diverged by 12 and the client got kicked). Modify hooks must stay pure; record state from a `CreatureCmd.Damage` prefix or a real hook instead. Diagnose desyncs from the host's `godot.log`: it prints LOCAL/REMOTE STATE DUMPs after `State divergence message received`; diff them.
- Custom Godot node classes must be `partial` (Godot source generator) and `MainFile` must call
  `ScriptManagerBridge.LookupScriptsInAssembly`, or `_Draw`/`_Process` overrides never fire.
- In-combat UI is added through BaseLib `ExtraCombatUi.RegisterCombatUiElement` (positioning `Left`
  stacks controls above the draw pile). A `CustomResource` subclass is auto-registered by BaseLib.

## Next (from spec §8 / §10)

1. ~~Persistence spike~~ Resolved 2026-09-12 by making Concoctions combat-scoped: they expire into stateless Dregs, so nothing with per-instance data crosses a save.
3. Composed Concoction description.
4. Gated cards (Sober-only / Tipsy+ / Drunk+ / Hungover-only) from the design pass; a real Intoxication icon (`images/ui/intoxication.png` is a drawn placeholder).

## Card art pipeline

Source art arrives as ~1438x1093 PNGs (same 1.316 aspect as the game's 1000x760 portraits), so it is
scaled, not cropped. File name = card id slug in snake_case (`slip_a_mickey.png`). Two copies:

```bash
sips -s format png -z 760 1000 src.png --out DrunkenMaster/images/card_portraits/big/<slug>.png
sips -s format png -z 190 250  src.png --out DrunkenMaster/images/card_portraits/<slug>.png
```

Then `dotnet publish`. The `.import` sidecars are generated by MegaDot during publish.
Added 2026-09-13: leftovers, distill, pick_me_up, restock, slip_a_mickey, empties, molotov, shot_glass, sour_punch, stockpot, knock_one_back, mash, nightcap, scrape_the_barrel, stir_the_pot, wake_up_call. 2026-09-13 pm (cardart4): boilermaker, bouncer, dutch_courage, last_round, lights_out. 2026-09-13 evening (loose files in Downloads): salt_the_rim, karaoke_night, upper_deckie (card was still called Zyn at the time), jungle_juice, beer_muscles, line_em_up, open_bar, staggering_blow. Later that evening: numb, cheap_shot, then a 10-image ChatGPT batch (unnamed files, identified by eye, in prompt order): corkscrew, spiked_drink, double_vision, spin_the_bottle, pregame, tolerance, blow_smoke, order_up, soda_gun, water_it_down. Then stir_crazy and top_shelf. **Every card now has art.** Top Shelf is the Ancient card: Ancient cards use the full-art slot (`NCard._ancientPortrait` with the ancient border and a text panel over the bottom third), so its files are 606x852 / 250x350 instead of 1000x760 / 250x190. Newer enemy-facing art is drawn enemy-free (no monster references needed).
