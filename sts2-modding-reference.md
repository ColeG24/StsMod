# Slay the Spire 2 Modding — Agent Reference

> Reference document for an AI coding agent working on a Slay the Spire 2 mod, focused on **character mods**.
> Compiled September 2026 against game `0.107.x`, BaseLib `3.4.5`.

---

## 0. Rules for the agent

Read these before writing any code.

1. **There is no official modding API.** Everything here is community-maintained and reverse-engineered. Treat every API in this document as *probably* correct and *definitely* subject to change.
2. **Verify against the decompiled game before inventing.** The authoritative source is `sts2.dll` (see §10). If you are unsure whether a method, enum value, or property exists, decompile and check. Do not hallucinate plausible-looking game APIs — they will compile against nothing and fail at load.
3. **Use commands, not direct data manipulation.** This is the single most important convention in the codebase. Game behavior is expressed as `DamageCmd`, `PowerCmd`, `CardCmd`, `CardPileCmd`, `CreatureCmd`, `CardSelectCmd`. Mutating model state directly breaks multiplayer sync, undo, and previews.
4. **Prefer BaseLib's `Custom*Model` base classes** over inheriting the game's raw models. In a template-generated project, prefer the mod-specific base classes (`YourModCard`, `YourModRelic`) over `CustomCardModel`/`CustomRelicModel` directly — they carry the asset-path conventions.
5. **When implementing a mechanic, find a base-game analogue first.** Search the decompiled code for a card/relic/power that does something similar and adapt it. This is faster and more correct than reasoning from first principles.
6. **The game is in Early Access.** Main branch updates roughly monthly; beta branch every week or two. Updates routinely break mods. If something that used to work suddenly throws, suspect a game update before suspecting your code.
7. **Non-code changes require Publish, not Build.** See §4. This trips up everyone.

---

## 1. Constants and paths

| Item | Value |
|---|---|
| Steam App ID | `2868840` |
| BaseLib Workshop ID | `3737335127` |
| BaseLib Workshop install path | `Steam/steamapps/workshop/content/2868840/3737335127/BaseLib` |
| Workshop mods root | `Steam/steamapps/workshop/content/2868840/` |
| Local mods dir (Win/Linux) | `<install>/mods` |
| Local mods dir (Mac) | `SlayTheSpire2.app/Contents/MacOS/mods` |
| Game code assembly | `sts2.dll`, in `<install>/data_sts2_windows_x86_64` (platform-varying `data_*`) |
| Game asset pack | `<install>/SlayTheSpire2.pck` |
| Game root namespace | `MegaCrit.Sts2.Core` |
| .NET SDK | 9.0+ |
| Project SDK / TFM | `Godot.NET.Sdk/4.5.1`, `net9.0` |
| Template NuGet package | `Alchyr.Sts2.Templates` |
| Template short names | `alchyrsts2mod`, `alchyrsts2charmod`, `alchyrsts2contentmod` |
| BaseLib NuGet | `Alchyr.Sts2.BaseLib` |
| Analyzer NuGet | `Alchyr.Sts2.ModAnalyzers` |
| Publicizer | `Krafs.Publicizer` 2.3.0 |
| Log file | `godot.log` |
| Log dir (Win) | `%appdata%/SlayTheSpire2/logs` |
| Log dir (Mac) | `~/Library/Application Support/SlayTheSpire2/logs` |
| Log dir (Linux) | `~/.local/share/SlayTheSpire2/logs` |
| Dev console keys | `` ~ ``, `` ` ``, `*`, `'`, `Shift+8` |

Launch flags: `-nomods`, `-fastmp host_standard`, `-fastmp join`, `-clientId <n>` (default 1000).

---

## 2. Environment setup

**Install:**

- **Rider** (recommended; the templates and the `Custom Card` file template assume it). Godot requires a `.sln`, not `.slnx` — relevant if using another IDE.
- **MegaDot** — Mega Crit's Godot fork, from `https://megadot.megacrit.com/`. Fallback: the Godot .NET build whose version exactly matches the current MegaDot.
- **.NET SDK 9.0+**
- **BaseLib** — subscribe on the Steam Workshop (ID `3737335127`). Manual fallback: drop the `.dll` / `.pck` / `.json` from GitHub releases into the mods folder (does not auto-update).

**Scaffold:**

```
dotnet new install Alchyr.Sts2.Templates
```

CLI:

```
dotnet new alchyrsts2charmod --ModAuthor YourName -o YourModName
```

Rider: `File > New Solution` → **Slay the Spire 2 Character**.

> **Critical:** check **"Put solution and project in same directory"**. Godot requires it. Without it, the `Add > Custom Card` file template will not appear and the project will not work as-is. There is no fix short of recreating the project.

No spaces or underscores in the mod name. Format must be `.sln`.

**Configure:**

1. `Directory.Build.props` → set `<GodotPath>` to the MegaDot install. **No quotes.**
2. If the game is not at the default Steam path, uncomment and set `<Sts2Path>`. Symptom of not doing this:
   `Error : Slay the Spire 2 data not found at path '?/steamapps/common/Slay the Spire 2/data_sts2_windows_x86_64'`
3. `YourModName.json` → set `name`, `author`, `description`. **Do not change `id`** — it determines the filenames the game looks for.

---

## 3. Project anatomy

### Folder layout

```
ModName
├── ModName/                     # asset root — becomes the .pck, namespaced to your mod
│   ├── mod_image.png
│   ├── localization/
│   │   └── eng/                 # ISO 639-2 codes: eng, deu, jpn, rus, zhs, ita, kor
│   │       ├── cards.json
│   │       ├── characters.json
│   │       ├── relics.json
│   │       ├── powers.json
│   │       ├── ancients.json
│   │       ├── card_keywords.json
│   │       └── static_hover_tips.json
│   └── images/
│       ├── card_portraits/{,big/,beta/}
│       ├── powers/{,big/}
│       ├── relics/{,big/}
│       ├── potions/{,outline/}
│       └── charui/
├── ModNameCode/                 # C#
│   ├── MainFile.cs
│   ├── Character/
│   ├── Cards/
│   ├── Relics/
│   └── Extensions/StringExtensions.cs
├── ModName.csproj
├── ModName.json                 # manifest
└── ModName.sln
```

**The nested `ModName/` folder is load-bearing.** With it, `localization/` files are *merged* with base-game localization and `images/` are namespaced to your mod. Without it (assets at project root), `localization/` and `images/` **replace** base-game files. The second layout is how you override base-game assets — and is almost never what you want for localization.

### Manifest (`ModName.json`)

```json
{
  "id": "ModName",
  "name": "Display Name",
  "author": "you",
  "description": "A description.",
  "version": "v0.0.1",
  "has_pck": true,
  "has_dll": true,
  "min_game_version": "0.107.0",
  "dependencies": [{ "id": "BaseLib", "min_version": "3.3.0" }],
  "affects_gameplay": true
}
```

> `affects_gameplay: false` mods are **not** checked when joining a multiplayer lobby. Setting this incorrectly on a gameplay mod causes desyncs. A character mod is always `true`.

`min_game_version` and dependency `min_version` exist as of game `0.105`.

### Entry point

```csharp
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace ModName.ModNameCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "ModName";
    public const string ResPath = $"res://{ModId}";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; }
        = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        // REQUIRED if you attach mod scripts to Godot scenes:
        // Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(assembly);

        Harmony harmony = new(ModId);
        harmony.PatchAll(assembly);
    }
}
```

Log with `MainFile.Logger.Info("...")`.

### csproj references

```xml
<ItemGroup>
  <PackageReference Include="Alchyr.Sts2.BaseLib" Version="*" PrivateAssets="All"/>
  <PackageReference Include="Alchyr.Sts2.ModAnalyzers" Version="*" />
  <AdditionalFiles Include="ModName/localization/**/*.json"/>   <!-- feeds the analyzer -->
</ItemGroup>

<ItemGroup Condition="Exists('$(Sts2DataDir)')">
  <Reference Include="0Harmony"><HintPath>$(Sts2DataDir)/0Harmony.dll</HintPath><Private>false</Private></Reference>
  <Reference Include="sts2"><HintPath>$(Sts2DataDir)/sts2.dll</HintPath><Private>false</Private></Reference>
</ItemGroup>
```

### Template-generated path helpers

`ModNameCode/Extensions/StringExtensions.cs` is generated into your project (not BaseLib) and supplies the `*Path()` extensions used everywhere:

```csharp
ImagePath()          // ModName/images/…
CardImagePath()      // images/card_portraits/…      (falls back to card.png)
BigCardImagePath()   // images/card_portraits/big/…
PowerImagePath()     // images/powers/…
BigPowerImagePath()  // images/powers/big/…
RelicImagePath()     // images/relics/…
BigRelicImagePath()  // images/relics/big/…
CharacterUiPath()    // images/charui/…
```

Each does a `ResourceLoader.Exists` check and logs + returns a placeholder if the file is missing — so a missing image is a log line, not a crash.

---

## 4. The build/publish loop

| Change | Action |
|---|---|
| `.cs` only | **Build** (hammer). Compiles the `.dll`, copies it to the mods folder. |
| Anything else — text, images, scenes, localization | **Publish**. Regenerates the `.pck` via Godot. |

Publish setup: right-click project → `Publish` → destination `Local folder`, defaults otherwise. Publishing compiles the `.dll`, generates the `.pck`, and copies `.dll` + `.pck` + `.json` into the game's mods folder.

**If you change localization or art and only Build, your changes will not appear.** This is the most common "why isn't my card showing the right text" cause.

Confirm the mod loaded: in-game `Settings → Mod Settings`.

### Setup failure modes

| Symptom | Fix |
|---|---|
| `[MSB4236] The SDK 'Godot.NET.Sdk/4.5.1' specified could not be found.` | `dotnet nuget add source https://api.nuget.org/v3/index.json` |
| `[MSB4236] ... 'Microsoft.NET.SDK.WorkloadAutoImportPropsLocator' ...` | Rider: `File > Settings`, search `toolset`, set MSBuild version to the Rider one |
| Publish fails silently | Prefix the GodotPublish command with `DOTNET_ROOT=~/.dotnet` (path to your dotnet) |
| Long `Godot.Bridge.CSharpInstanceBridge.Call` error full of `System.ArgumentException: Value does not fall within the expected range` / `MonoMod.Core.Interop...InvokeCompileMethod` | In csproj, find `<Publicize Include="sts2"` and flip the line above it from `False` to `True` — or remove `Krafs.Publicizer` entirely |
| `System.ArgumentException: Undefined resource string ID:0x80070057` | Same as above |
| Red lint after adding a publicized field | `Build > Clean Solution`, then rebuild |
| Missing BaseLib APIs | NuGet dependency out of date. Rider: `Alt+Shift+7`, update BaseLib **and** the analyzer |

---

## 5. Core mental model

> Almost all content exists as a **Model** (`CardModel`, `RelicModel`, `PowerModel`, `CharacterModel`…) that hooks into shared lifecycle methods (`OnPlay`, `OnUpgrade`, `AfterTurnEnd`…). Behavior inside those methods is expressed as **Commands**. Models are tracked in `ModelDb` and loaded automatically at launch.

Each model has a corresponding Godot node class (`CardModel` → `NCard`).

### ID prefixing

BaseLib auto-prefixes any `ICustomModel`'s ID with its **root namespace, uppercased, plus `-`**. A mod rooted at namespace `MyMod` produces IDs like `MYMOD-FANCYSTRIKE`. This is what keeps mods from colliding, and it is why localization keys are `MODPREFIX-THING.title`.

Override with `[CustomID("EXACT_ID")]` when you need an exact ID (e.g. deliberately overriding base-game content).

`ICustomModel` is a bare marker interface — you can implement it on a class that doesn't inherit a `Custom*Model` to get prefixing.

### Pools

Every card/relic/potion must live in a pool, declared with `[Pool(typeof(SomePool))]`. The attribute is inherited, which is why the character template's `YourModCard` base class carries it and individual cards don't need it.

If your class does **not** inherit `CustomCardModel`, you must add a default constructor calling `CustomContentDictionary.AddModel(GetType())` for `[Pool]` to work. Missing `[Pool]` throws at startup with a clear message.

### Dynamic variables

`CanonicalVars` is the source of truth for a model's numbers; `DynamicVars` is the *computed* view after modifiers, and is what you read when executing commands.

```csharp
protected override IEnumerable<DynamicVar> CanonicalVars =>
[
    new DamageVar(6, ValueProp.Move),
    new BlockVar(5, ValueProp.Move),
    new PowerVar<StrengthPower>(1),
    new CardsVar(1),
    new RepeatVar(3),
    new IntVar(nameof(SomeName), 2),
    new StringVar("TamerName", "Franklin"),
    new BoolVar("IsStinky", true),
];
```

Read them as `DynamicVars.Damage.BaseValue`, `DynamicVars.Repeat.IntValue`, or by name when there is no getter: `DynamicVars[nameof(StrengthLossPower)].BaseValue`.

Upgrade with `DynamicVars.Damage.UpgradeValueBy(3m)` inside `OnUpgrade`.

---

## 6. Character mods

### `CustomCharacterModel`

Inherit `CustomCharacterModel`, or `PlaceholderCharacterModel` (which reuses a base-game character's art, defaulting to `ironclad`) while you're building out.

Template character:

```csharp
public class CharMod : PlaceholderCharacterModel
{
    public const string CharacterId = "CharMod";
    public static readonly Color Color = new("ffffff");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;

    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<StrikeIronclad>(), /* ×5 */
        ModelDb.Card<DefendIronclad>()  /* ×5 */
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ ModelDb.Relic<BurningBlood>() ];

    public override CardPoolModel   CardPool   => ModelDb.CardPool<CharModCardPool>();
    public override RelicPoolModel  RelicPool  => ModelDb.RelicPool<CharModRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<CharModPotionPool>();

    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    public override string CustomIconTexturePath                => "character_icon_name.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath        => "char_select_name.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath  => "char_select_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath                  => "map_marker_name.png".CharacterUiPath();
}
```

### Full override surface

Defaults worth knowing: `StartingGold => 99`, `AttackAnimDelay => 0.15f`, `CastAnimDelay => 0.25f`, `DeathAnimTime => 1.5f`, `UnlocksAfterRunAs => null`.

Visibility:

```csharp
bool     HideFromVanillaCharacterSelect       // default false
bool     AllowInVanillaRandomCharacterSelect  // default !HideFromVanillaCharacterSelect
bool     HideInCompendium                     // default false
ModelId  DefaultCompendiumOpenModelId         // default Id
```

Asset paths (all `string?`, all null by default):

```
CustomVisualPath                 CustomTrailPath
CustomIconTexturePath            CustomIconOutlineTexturePath   CustomIconPath
CustomEnergyCounterPath          CustomEnergyCounter (CustomEnergyCounter?)
CustomRestSiteAnimPath           CustomMerchantAnimPath
CustomArmPointingTexturePath     CustomArmRockTexturePath
CustomArmPaperTexturePath        CustomArmScissorsTexturePath
CustomCharacterSelectBg          CustomCharacterSelectIconPath
CustomCharacterSelectLockedIconPath                CustomCharacterSelectTransitionPath
CustomMapMarkerPath              CustomYummyCookie (RelicIconData?)
CustomAttackSfx                  CustomCastSfx                  CustomDeathSfx
```

Overridable methods:

```csharp
public virtual NCreatureVisuals? CreateCustomVisuals();
public virtual CreatureAnimator? SetupCustomAnimationStates(MegaSprite controller);  // Spine only
```

`CustomYummyCookie` example:

```csharp
public override RelicIconData CustomYummyCookie => new(
    "relic.png".BigRelicImagePath(),
    "relic.png".RelicImagePath(),
    "relic_outline.png".RelicImagePath());
```

### Card pool (this is where card-back color and energy icon live)

```csharp
public class CharModCardPool : CustomCardPoolModel
{
    public override string Title => CharMod.CharacterId;   // NOT a display name

    public override string BigEnergyIconPath  => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    // HSV shader applied over an already-colored card back image
    public override float H => 1f;
    public override float S => 1f;
    public override float V => 1f;

    public override Color DeckEntryCardColor => new("ffffff");
    public override bool  IsColorless => false;
}
```

Other members: `CustomFrame(CustomCardModel card)`, `CardFrameMaterialPath` (default `"card_frame_red"`), `ShaderColor`, `IsShared`, `SeenByDefault`, `CustomCardPoolModel.MarkAllAsSeen()`.

`CustomRelicPoolModel` and `CustomPotionPoolModel` exist analogously.

### Creature visuals

Root node must be a `Node2D`. Contract:

| Node | Unique name | Type | Required |
|---|:-:|---|:-:|
| `Visuals` | yes | any Node2D, holds all visuals | **yes** |
| `Bounds` | yes | Control — hitbox | **yes** |
| `IntentPos` | yes | Marker2D — intent position | **yes** |
| `CenterPos` | yes | Marker2D — vfx spawn | **yes** |
| `PhobiaModeVisuals` | yes | Node2D — phobia-mode alternative | no |
| `OrbPos` | yes | Marker2D — orb center (falls back to IntentPos) | no |
| `TalkPos` | yes | Marker2D — speech bubble origin | no |

If the scene is converted by `NodeFactory`, only `Visuals` is required (`Bounds` strongly recommended).

The Godot editor can't see script classes from another assembly. Four workarounds:

1. `NodeFactory<NCreatureVisuals>.CreateFromScene(path)` — `CustomCharacterModel` does this automatically when the visuals scene path doesn't inherit `NCreatureVisuals`.
2. `NodeFactory<NCreatureVisuals>.CreateFromResource(pngPath)` — simplest path for a static character image:
   ```csharp
   public override NCreatureVisuals CreateCustomVisuals()
       => NodeFactory<NCreatureVisuals>.CreateFromResource("res://ModName/images/character/image.png");
   ```
3. Define a `[GlobalClass]` class inheriting `NCreatureVisuals` and use it as the scene's script.
4. Hand-edit the `.tscn` as text.

**Animation:** BaseLib auto-plays animations from an `AnimationPlayer`, `AnimationPlayer2D`, or an `AnimationTree` wired to an `AnimationPlayer`. Recognized names are the `CreatureAnimator` consts or: `idle`, `attack`, `cast`, `hurt`, `die`. With an `AnimationTree`, the Tree Root must be an `AnimationNodeStateMachine` and transitions generally need switch mode **At End**.

**Spine:** the game uses Spine for character animation. Full Spine is ~$400, so most modders recolor atlas pieces (`animations/characters/silent/silent.png`) or build their own Godot animation instead. Anything loadable in the Godot editor is loadable in-game.

### Energy counter

Root `Control`, size 128×128, pivot offset (64, 64).

| Node | Unique name | Type | Required |
|---|:-:|---|:-:|
| `EnergyVfxBack` | yes | Node2D with GpuParticles2D | yes* |
| `Layers` | yes | Control — static images | yes |
| `RotationLayers` | yes | Control, child of Layers | yes* |
| `EnergyVfxFront` | yes | Node2D with GpuParticles2D | yes* |
| `Label` | no | MegaLabel (plain Label if converting) | yes* |

\* optional if the scene is converted by `NodeFactory`.

To match the base-game font: theme Overrides → Constants → Shadow Offset X `3`, Shadow Offset Y `2`, Outline Size `16`, Shadow Outline Size `16`. Font *color* can't be set in Godot (`NEnergyCounter` recolors by energy level); set the outline color via the `EnergyLabelOutlineColor` override on the player class.

### Merchant / rest site

`CustomMerchantAnimPath` and `CustomRestSiteAnimPath`. BaseLib converts the scene to `NMerchantCharacter` if needed. The game plays `relaxed_loop` (or `idle`/`Idle`), and `die`. For Spine, the first child node must be the Spine node.

### Character localization

`ModName/localization/eng/characters.json`. `CharacterLoc` record fields, in order:

```
Title, TitleObject, Description,
PronounObject, PronounSubject, PronounPossessive, PossessiveAdjective,
AromaPrinciple, EndTurnPingAlive, EndTurnPingDead,
EventDeathPrevention, GoldMonologue,
CardsModifierTitle, CardsModifierDescription
```

emitting keys `title`, `titleObject`, `description`, `pronounObject`, `pronounSubject`, `pronounPossessive`, `possessiveAdjective`, `aromaPrinciple`, `banter.alive.endTurnPing`, `banter.dead.endTurnPing`, `eventDeathPrevention`, `goldMonologue`, `cardsModifierTitle`, `cardsModifierDescription`.

In Rider: put the cursor on the class, `Alt+Enter` → **Generate localization**, then cut the generated block into the JSON file. The analyzer will keep flagging the class until the entry exists.

### Architect dialogue is mandatory

> **Custom characters are required to define an Architect dialogue, or players cannot finish a run.**

Key grammar: `{ANCIENT_ID}.talk.{CHARACTER_ID}.{dialogueIndex}-{lineIndex}[r].{ancient|char|next}`

```json
"THE_ARCHITECT.talk.MYMOD-MYCHAR.0-0r.char":   "I kill you",
"THE_ARCHITECT.talk.MYMOD-MYCHAR.0-0r.next":   "Continue",
"THE_ARCHITECT.talk.MYMOD-MYCHAR.0-1r.ancient":"No",
"THE_ARCHITECT.talk.MYMOD-MYCHAR.0-attack":    "Both"
```

- `r` marks a line repeatable; `ANY` as character ID applies to all characters.
- `-attack` decides who animates: `None`, `Player`, `Architect`, `Both` (default `Architect`).
- Optional `.ancient.sfx` sibling key for a sound event.
- For normal ancients, dialogue 0 = first visit, 1 = second, 2 = fifth; past 2, each subsequent dialogue needs 3 more visits. Override with a `{index}-visit` key. For the Architect, dialogues are consecutive.
- Do **not** define interactions between base-game characters and ancients — index checking starts at 0 and you will override base-game localization.

---

## 7. Cards

Create with right-click folder → `Add` → **Custom Card** (Rider, requires the character/content template plus "same directory"). Otherwise write the class by hand.

```csharp
[Pool(typeof(CharModCardPool))]                 // inherited from your mod's base class — usually omit
public class FancyStrike() : CharModCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CommonActions.CardAttack(this, play.Target, vfx: "vfx/vfx_attack_slash")
                           .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
```

`CustomCardModel` constructor: `(int baseCost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true, bool autoAdd = true)`.

Visual overrides: `CustomFrame`, `CreateCustomFrameMaterial`, `CustomBannerMaterial`, `CustomBannerMaterialPath`, `CustomPortraitPath`, `CustomPortrait`.

Calculated-value helpers (static on `CustomCardModel`): `MakeCalculatedDamage`, `MakeCalculatedBlock`, `MakeCalculatedVar`, each taking `Func<CardModel, Creature?, decimal> bonus`.

### `ConstructedCardModel` — builder alternative

Seals `CanonicalVars`/`CanonicalKeywords`/`ExtraHoverTips`/`CanonicalTags` and configures in the constructor instead. Auto-generates tooltips for powers referenced by `PowerVar`/keywords.

```csharp
public FancyStrike() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    WithDamage(77, 23);
    WithTags(CardTag.Strike);
    WithTip(typeof(StrikeIronclad));
}
```

Fluent surface: `WithVars`, `WithVar`, `WithBlock`, `WithDamage`, `WithCards`, `WithEnergy`, `WithHeal`, `WithPower<T>`, `WithTags`, `WithKeywords`, `WithKeyword`, `WithCostUpgradeBy`, `WithCalculatedVar/Block/Damage`, `WithTip`, `WithTips`, `WithEnergyTip`, `WithUpgradingCardTip<T>`.

`WithTip` accepts a `Type` (power/card/potion/enchantment), a `CardKeyword`, or a `StaticHoverTip` — implicit conversions via `TooltipSource`.

### Card art

| Slot | Path | Size |
|---|---|---|
| Big portrait | `images/card_portraits/big/` | 1000×760 (full art: 606×852) |
| Small portrait | `images/card_portraits/` | 250×190 (full art: 250×350) |
| Beta art | `images/card_portraits/beta/` | same as small |

Filename = class name, lowercase, underscore-separated: `FancyStrike` → `fancy_strike.png`. Smaller images are fine if the aspect ratio holds — they scale up. The small variant is an optimization, not a requirement. `card.png` is the fallback for cards without art.

### Card localization

`ModName/localization/eng/cards.json`. An empty file must still contain `{}`. Generate per-card via `Alt+Enter` → Generate localization, then move the block into the JSON.

---

## 8. Relics and powers

```csharp
[Pool(typeof(CharModRelicPool))]
public class DummyRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override RelicModel? GetUpgradeReplacement() => null;
}
```

Icon paths default to `images/atlases/relic_atlas.sprites/` (packed), `images/atlases/relic_outline_atlas.sprites/` (outline), `images/relics/` (big), resolved by `IconBaseName`. When you override `PackedIconPath` / `PackedIconOutlinePath` / `BigIconPath` you may return a `.png` for all three — the template's relic base class does exactly that.

Powers:

```csharp
public class ExplosivesPower : CustomPowerModel
{
    public override PowerType      Type      => PowerType.Buff;      // Buff | Debuff
    public override PowerStackType StackType => PowerStackType.Counter; // None | Counter | Single
}
```

Power vars require the type parameter: `new PowerVar<ExplosivesPower>(1)` or `new PowerVar<ExplosivesPower>("FollowupExplosivePower", 1)`.

---

## 9. Command cookbook

Lifecycle methods commonly overridden: `OnPlay`, `OnUpgrade`, `BeforeCardPlayed`, `AfterCardPlayed`, `AfterSideTurnStart`, `AfterTurnEnd`, `AfterCardDrawn`, `AfterPowerAmountChanged`.

### Attacks

```csharp
// single target — TargetType.AnyEnemy
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this).Targeting(play.Target)
    .WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);

// all enemies — TargetType.AllEnemies
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this).TargetingAllOpponents(CombatState)
    .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3").Execute(choiceContext);

// random — TargetType.RandomEnemy
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this).TargetingRandomOpponents(CombatState)
    .WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);

// multi-hit — add new RepeatVar(3)
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this).WithHitCount(DynamicVars.Repeat.IntValue)
    .WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
```

Card `TargetType` must match the command's targeting.

### Block, powers

```csharp
await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

await PowerCmd.Apply<ExplosivesPower>(new ThrowingPlayerChoiceContext(), Owner.Creature,
    DynamicVars[nameof(ExplosivesPower)].BaseValue, Owner.Creature, this);

await PowerCmd.Decrement(this);
await PowerCmd.Remove(this);
await PowerCmd.ModifyAmount(ctx, this, -1, null, null);
SetAmount(Amount - 6);
```

### Cards

```csharp
CardCmd.Upgrade(card);
await CardCmd.AutoPlay(choiceContext, card, target);
await CardCmd.Exhaust(choiceContext, card);
await CardCmd.Discard(choiceContext, card);
CardCmd.ApplyKeyword(card, CardKeyword.Ethereal);
CardCmd.Enchant<Sharp>(card, amount);
```

### Piles

```csharp
await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
await CardPileCmd.Add(card, PileType.Hand);
await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top);
await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Random, this);

CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
    card, PileType.Draw, Owner, CardPilePosition.Random));
CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(
    cards, PileType.Hand, Owner.Player));

await CardPileCmd.AddToCombatAndPreview<Debris>(Owner.Creature, PileType.Hand, 4, Owner);
await CardPileCmd.AddCursesToDeck(Enumerable.Repeat(ModelDb.Card<Guilty>(), 1), Owner);
```

### Card selection

```csharp
var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);

var card = (await CardSelectCmd.FromSimpleGrid(choiceContext,
    PileType.Discard.GetPile(Owner).Cards, Owner, prefs)).FirstOrDefault();

var selected = (await CardSelectCmd.FromHand(choiceContext, Owner, prefs, null, this))
    .FirstOrDefault();

// range with filter
var prefs2 = new CardSelectorPrefs(SelectionScreenPrompt, 0, DynamicVars.Cards.IntValue);
var picks = await CardSelectCmd.FromHand(choiceContext, Owner, prefs2, c => c.IsTransformable, this);
```

### `CommonActions` shorthands (BaseLib)

```csharp
CommonActions.CardAttack(card, target|play, hitCount, vfx, sfx, tmpSfx)  // → AttackCommand
CommonActions.CardBlock(card, play)
CommonActions.Draw(card, context)
CommonActions.Apply<T>(context, target, card)         // + several overloads
CommonActions.ApplySelf<T>(context, card)
CommonActions.SelectCards(...) / SelectSingleCard(...)
CommonActions.GenerateCards(card, count, filter) / GenerateSingleCard(card, filter)
```

---

## 10. Decompiling and asset extraction

**Code.** In Rider: press **Shift ×4** to search everything including `sts2` and BaseLib; **Ctrl+Click** any game type to jump into decompiled source; **Find Usages** works on decompiled members. External options: ILSpy (most readable), dnSpy, dotPeek.

Decompiled code has artifacts you must not copy literally:

| Artifact | Reality |
|---|---|
| `[OriginalAttributes(MethodAttributes.Family)]` | The member was originally non-public; Publicizer made it public |
| `<>z__ReadOnlySingleElementList<T>` | A `[ ... ]` collection expression |
| String literals where a const should be | `const` fields are inlined at compile time — find the real const |
| `0.75M`, redundant casts | Explicit `decimal` suffix / unnecessary cast |
| `HistoryCourse historyCourse = this;` at method top | Compiler rewrite of `this` in an async method |
| `// ISSUE: reference to compiler-generated method` | The decompiler failed on a lambda |

Example — decompiled vs. how you'd actually write it:

```csharp
// decompiled
public override IEnumerable<DynamicVar> CanonicalVars {
  [OriginalAttributes(MethodAttributes.Family)] get {
    return (IEnumerable<DynamicVar>) new <>z__ReadOnlySingleElementList<DynamicVar>(
        new DynamicVar("DamageDecrease", 0.75M));
  }
}

// real
public override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(_damageDecrease, 0.75)];
```

**Assets.** Use **GDRE Tools** (`https://github.com/GDRETools/gdsdecomp`): `RE Tools → Recover Project`, open `SlayTheSpire2.pck`. Text lands in `localization/`, code in `src/Core` (use a decompiler instead), everything else in sensibly named folders. Extract once to your own folder for repeated reference.

---

## 11. Patching, publicizing, reflection

Harmony (`0Harmony.dll`, docs at `https://harmony.pardeike.net/`) is how you change base-game behavior. `harmony.PatchAll(assembly)` in your initializer picks up `[HarmonyPatch]` classes.

**Publicizer** gives compile-time access to private members without reflection:

```xml
<PropertyGroup>
  <PublicizerClearCacheOnClean>true</PublicizerClearCacheOnClean>
</PropertyGroup>

<ItemGroup>
  <!-- format: dllName:Namespace.ClassMember -->
  <Publicize Include="sts2:MegaCrit.Sts2.Core.Commands.Builders.AttackCommand._combatState"/>
</ItemGroup>

<ItemGroup>
  <PackageReference Include="Krafs.Publicizer" Version="2.3.0" PrivateAssets="All">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

After adding a publicized member, `Clean Solution` then rebuild if lint goes red.

Harmony's `AccessTools` (`FieldRef`, `Field`, `Method`) and `Traverse` are the runtime alternatives. The community wiki page for these is still a stub — read Harmony's own docs.

**Scenes with mod scripts.** If you attach scripts from your assembly to Godot scenes, your initializer must call:

```csharp
Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());
```

---

## 12. Extending game state — `SpireField`

Attach your own data to existing objects without patching their classes. Backed by `ConditionalWeakTable`.

```csharp
public static class SpecialNumberThing
{
    public static readonly SpireField<CardModel, int> SpecialNumber = new(() => 0);
}

var model = ModelDb.Card<Bash>();
SpecialNumberThing.SpecialNumber[model] = 5;
int v = SpecialNumberThing.SpecialNumber.Get(model);
```

- Per-player, per-combat data (orbs-style): `SpireField<PlayerCombatState, T>`
- Persistent per-player data: `SavedSpireField<Player, T>`
- `NotNullSpireField`, `ReadonlySpireField` variants exist; `CopyOnClone` controls clone behavior.

**`SavedSpireField`** persists across saves. Supported holders: `CardModel`, `RelicModel`, `PotionModel`, `EnchantmentModel`, `Player`, `Reward`, `IRunState`. Natively serializable value types: `int`, `bool`, `string`, `int[]`, `ModelId`, `SerializableCard`, `SerializableCard[]`, `List<SerializableCard>`. Anything else must be registered in your initializer:

```csharp
ExtendedSaveTypes.RegisterObjectSaveType<MyType>(
    ExtendedSaveTypes.PropertyFunc<MyType, string>(nameof(MyType.Value)));
```

Also available: `RegisterAdditionalSaveType<T>`, `RegisterDictionarySaveType<K,V>`, `RegisterListSaveType<T>`, `RegisterSavedValue<TTarget,T>`, `FieldFunc`. Types implementing `IPacketSerializable` supply their own `Serialize`/`Deserialize`; otherwise set the field's `Serializer`/`Deserializer`.

**`AddedNode`** injects UI into existing scenes by Harmony-postfixing `_Ready`:

```csharp
public static AddedNode<NCard, MyDisplay> Node = new(card => { /* build + AddChild */ return control; });
// or from a scene file:
public static AddedNode<NCard, MyDisplay> FromScene =
    new("res://ModName/scenes/MyDisplay.tscn", (card, display) => { /* parent it */ });
```

For cards specifically, parent to `card.GetChild(0)` (the CardContainer), not the `NCard` — the `NCard` doesn't receive all transforms.

---

## 13. Custom enums and keywords

```csharp
[CustomEnum] public static CardTag MyNewTag;
[CustomEnum] public static StaticHoverTip Mechanic;
[CustomEnum, KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword MyKeyword;
```

> **Reference them by the declaring class, not the enum type.**
> `CardTag.MyNewTag` ✗ — `MyClassName.MyNewTag` ✓

Fields must be `public static` and **not** `readonly` (a readonly field may fail to be set). Values derive from a hash of (root namespace, field name), so they're stable across runs. `AutoKeywordPosition` is `None | Before | After`; `RichKeyword` enables energy icons in the tooltip.

Localization: `StaticHoverTip` needs `MODPREFIX-NAME.title` / `.description` in `static_hover_tips.json`; `CardKeyword` the same in `card_keywords.json`.

> Conceptual note: StS2's `CardKeyword` is narrower than StS1's keywords — single words that affect card behavior with no associated number. `Sly` is a `CardKeyword`; `Summon` is not.

BaseLib ships one keyword (`BaseLibKeywords.Purge`) and three dynamic vars usable on your cards: `Persist`, `Exhaustive`, `Refund` (plus `Scry`).

Extra target types in `CustomTargetType`: `Everyone`, `Anyone`, `AllAttackingEnemies`, `AnyAttackingEnemy`, `AllBlockingEnemies`, `AnyBlockingEnemy`, `AllNonBlockingEnemies`, `AnyNonBlockingEnemy`, `AllHighestHpEnemies`, `AllLowestHpEnemies`, `AnyFullLifeEnemy`, `AllFullLifeEnemies`, `Pet`, `PetOrSelf`. For multi-target types use `this.GetTargets()` with `TargetingFiltered`, or let `CommonActions.CardAttack` handle it.

---

## 14. Localization mechanics

Layout: `<ModId>/localization/<lang>/<table>.json`. Tables: `cards`, `relics`, `powers`, `characters`, `ancients`, `card_keywords`, `static_hover_tips`, `settings_ui`, `card_selection`, `gameplay_ui`, `main_menu_ui`, `credits`.

Set a fallback language in your initializer: `DefaultLoc.Set(ModId, "eng");`

**In-code localization** (convenient, but hostile to translators):

```csharp
public override List<(string, string)> Localization => new CardLoc("Title", "Description");
```

Loc records (all in `BaseLib.Abstracts`, all implicitly convert to `List<(string,string)>`): `ActLoc`, `CardLoc`, `CardModifierLoc`, `CharacterLoc`, `EncounterLoc`, `EventLoc`/`EventPageLoc`/`EventOptionLoc`, `ModifierLoc`, `MonsterLoc`, `OrbLoc`, `PotionLoc`, `PowerLoc`, `RelicLoc`.

**Text formatting** runs through SmartFormat. `{Var:diff()}` highlights upgraded values, `{Var:inverseDiff()}` the reverse, `{Var:plural:use|uses}` pluralizes, `{Var:cond:>1?...|}` conditionals. BBCode tags like `[gold]…[/gold]` and `[blue]…[/blue]` style text (Godot RichTextLabel BBCode).

Note that `.description` (static) and `.smartDescription` (formatted) are separate keys:

```json
"BASELIB-EXHAUSTIVE.title": "Exhaustive",
"BASELIB-EXHAUSTIVE.description": "This card [gold]Exhausts[/gold] after [blue]X[/blue] uses.",
"BASELIB-EXHAUSTIVE.smartDescription": "This card [gold]Exhausts[/gold] after {Exhaustive:cond:>1?[blue]{Exhaustive}[/blue] |}{Exhaustive:plural:use|uses}."
```

**Variable tooltips:** `new DynamicVar("Name", 6).WithTooltip()` attaches a tooltip; the key is `MODPREFIX-NAME` in `static_hover_tips.json`. Tooltips generated this way can only access their own variable.

**`DisplayVar`** injects arbitrary computed text into a description:

```csharp
new DisplayVar<ModelType>("key", m => (m.GetDynamicVar("Something").IntValue * 10).ToString())
```

**`SimpleLoc`** (opt in per-entry with a leading `#`, or mod-wide via `SimpleLoc.EnableSimpleLoc(ModId)` — not recommended, it processes every string):

| Shorthand | Expands to |
|---|---|
| `!Var!` | `{Var:diff()}` |
| `@Var@` | `{Var:inverseDiff()}` |
| `*Word` | `[gold]Word[/gold]` (ends at whitespace; `*Wo*rd` scopes it) |
| `card(s)` | `card{Cards:plural:\|s}` |
| `-text-` | removed when upgraded |
| `+text+` | added when upgraded |
| `[EEE]` / `[E?]` | fixed / variable energy icons |

Var shorthands: `D`→Damage, `CD`→CalculatedDamage, `B`→Block, `CB`→CalculatedBlock, `C`→Cards, `E`→Energy, `H`→Heal.

**Overriding base-game text** — use base-game keys in your merged localization:

```json
{ "STRIKE_IRONCLAD.title": "Strike 2", "STRIKE_IRONCLAD.description": "Electric Boogaloo" }
```

---

## 15. Mod configuration

```csharp
internal class MyModConfig : SimpleModConfig
{
    public static bool RandomExplosions { get; set; } = true;
    public static int  ExplosionSize    { get; set; } = 80;
}

// in Initialize()
ModConfigRegistry.Register(ModId, new MyModConfig());
```

Static properties are readable anywhere as `MyModConfig.ExplosionSize`. UI is generated automatically:

| Type | Control |
|---|---|
| `bool` | checkbox |
| any enum | dropdown |
| `int`/`float`/`double` | slider |
| `string` | line edit |
| `Color`, or `string` with `[ConfigColorPicker]` | color picker |
| method | button |

Static properties of other types need `[ConfigIgnore]` or they log a warning. Other attributes live in `BaseLib.Config` (`ConfigSectionAttribute`, etc.). Registry API: `ModConfigRegistry.Register/Get/Get<T>/GetAll`.

---

## 16. Testing and debugging

**Dev console** — open with `` ~ ``, `` ` ``, `*`, `'`, or `Shift+8` (mods must be enabled). `help`, `help <command>`. Spawns most content types directly for quick testing.

**Logs** — BaseLib adds `showlog` (opens a persistent log window) and `open logs` (opens the log directory). Most recent is `godot.log`. Auto-open at startup: main menu → `Mod Configuration` → `BaseLib` → "Open log window on startup".

**Vanilla comparison** — launch with `-nomods`, or add SlayTheSpire2 as a non-Steam game with different launch options. Note that **modded and unmodded runs use separate save files**.

**Local multiplayer testing** — create `steam_appid.txt` containing `2868840` in the game directory (lets the exe launch outside Steam). Then:

```
SlayTheSpire2.exe -fastmp host_standard
SlayTheSpire2.exe -fastmp join
SlayTheSpire2.exe -fastmp join -clientId 1001    # 3rd player, 1002 for 4th
```

**Debugger** — copy the pdb next to your dll by appending to the `CopyToModsFolderOnBuild` target in your csproj:

```xml
<Copy SourceFiles="$(TargetDir)$(TargetName).pdb" DestinationFolder="$(ModsPath)$(MSBuildProjectName)/" />
```

Then add a Run/Debug configuration of type **.Net Executable** pointing at the game exe and working directory. To step into game/BaseLib code: Rider → Settings → Build, Execution, Deployment → Debugger → .Net Languages → **Enable external source debug**; Visual Studio → Tools → Options → Debugging → **disable Just My Code**.

---

## 17. Distribution

Upload to the Steam Workshop with Mega Crit's uploader: `https://github.com/megacrit/sts2-mod-uploader`. Upload the *folder named after your mod* containing the `.json`, `.dll`, and `.pck`.

Workshop support shipped in Major Update 2 (2026-06-19). Before that, distribution was NexusMods and the community Discord; both still carry mods.

---

## 18. Known documentation gaps

Do not assume these are documented — read the source at `https://github.com/Alchyr/BaseLib-StS2`:

- The reflection wiki page (`AccessTools`, `Traverse`) is an empty stub.
- "Calculated Vars" and `CardCmd` "Transform" in the cookbook are TODO.
- The card *functionality* tutorial was never written.
- No wiki coverage at all for: `CustomResource` (the largest file in BaseLib), `CustomReward` / `CustomLinkedRewardSet`, `CustomBadge`, `CustomMessage` / `CustomTargetedMessage` (multiplayer), `CustomModifierModel`, `CustomEnchantmentModel`, `CustomPetModel`, `CustomRestSiteOption`, `CardModifier`, `MoveBuilder`, `FmodAudio` (in-repo `docs/FmodAudio.md` only), `Utils/Patching/*`, `Diagnostics/HarmonyPatchDump*`.
- `Notes.txt` in the BaseLib repo root holds unpolished reverse-engineering notes on the card-play pipeline, `CardPileCmd.Add`, and async state-machine IL — useful background, not API docs.

The `#sts2-modding` channel on the Slay the Spire Discord is where current knowledge actually lives.

---

## 19. Full model class inventory

`Abstracts/` in BaseLib: `CustomActModel`, `CustomAncientModel`, `CustomBadge`, `CustomCardModel`, `ConstructedCardModel`, `CustomCardPoolModel`, `CustomCharacterModel`, `PlaceholderCharacterModel`, `CustomCharacterSelectEntry`, `CustomEnchantmentModel`, `CustomEncounterModel`, `CustomEventModel`, `CustomMessage`, `CustomTargetedMessage`, `CustomModifierModel`, `CustomMonsterModel`, `CustomOrbModel`, `CustomPetModel`, `CustomPile`, `CustomPotionModel`, `CustomPotionPoolModel`, `CustomPowerModel`, `CustomRelicModel`, `CustomRelicPoolModel`, `CustomResource`, `CustomRestSiteOption`, `CustomReward`, `CustomSingletonModel`, `CustomTemporaryPowerModel(+Wrapper)`, `CardModifier`.

Marker interfaces: `ICustomModel`, `ICustomPower`, `ICustomTypeTextCard`, `IHasSecondAmount`, `ILocalizationProvider`, `ISceneConversions`, `ITomeCard`, `ITranscendenceCard`, `ITrashHeapCard`, `ITrashHeapRelic`, `ICustomEnergyIconPool`, `IAutoRegisterFormatSpecifier`.

Hook interfaces (implement on an `AbstractModel` subclass present in the active combat state): `IHealAmountModifier`, `IMaxHandSizeModifier`, `IHealthBarForecastSource`, `IAfterCardDowngraded`, `ICardTypeTextModifier`, `IAfterScryed`, `IModifyScryAmount`.

BaseLib namespaces: root `BaseLib`; `.Abstracts`, `.Cards(.Variables)`, `.Config(.UI)`, `.Hooks`, `.Utils(.Attributes|.ModInterop|.NodeFactories|.Patching)`, `.Audio`, `.Extensions`, `.Patches.Content`, `.Patches.Localization`, `.Patches.Features`, `.Patches.Saves`, `.Monsters`.

Game namespaces you'll touch: `MegaCrit.Sts2.Core.Models`, `.Entities.Cards`, `.Entities.Creatures`, `.Entities.Players`, `.GameActions.Multiplayer` (`PlayerChoiceContext`), `.Commands(.Builders)`, `.Localization.DynamicVars`, `.Helpers` (`ImageHelper`, `SceneHelper`, `ReflectionHelper`), `.Modding` (`ModInitializer`, `ModHelper`), `.Nodes.*`, `.ValueProps`, `.Random` (`Rng`).

---

## Sources

- Community modding wiki — https://github.com/Alchyr/ModTemplate-StS2/wiki
- BaseLib documentation — https://alchyr.github.io/BaseLib-Wiki/
- BaseLib source — https://github.com/Alchyr/BaseLib-StS2
- Mod template source — https://github.com/Alchyr/ModTemplate-StS2
- Analyzers — https://github.com/Alchyr/StS2ModAnalyzers
- Workshop uploader — https://github.com/megacrit/sts2-mod-uploader
- GDRE Tools — https://github.com/GDRETools/gdsdecomp
- Harmony — https://harmony.pardeike.net/
- Godot 4.5 docs — https://docs.godotengine.org/en/4.5/
- Blender animation guide — https://github.com/r2Nexus/The-Engineer/wiki/Animating-StS2-models-with-Blender-%E2%80%90-Introduction
- Shader demo project — https://github.com/Ind-E/StS2ShaderDemo
- Debugger setup screenshots — https://github.com/pikcube/Setting-Up-the-Slay-the-Spire-2-Debugger
- Rider localization plugin — https://github.com/lamali292/STS2-Rider-Localization-Plugin
- ModConfig alternative — https://github.com/xhyrzldf/ModConfig-STS2
