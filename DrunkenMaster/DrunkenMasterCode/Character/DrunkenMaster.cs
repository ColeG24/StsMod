using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using DrunkenMaster.DrunkenMasterCode.Cards.Basic;
using DrunkenMaster.DrunkenMasterCode.Extensions;
using DrunkenMaster.DrunkenMasterCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace DrunkenMaster.DrunkenMasterCode.Character;

/// <summary>
/// The Drunken Master. See drunken-master-spec.md §2 / §3.
/// Uses Ironclad placeholder art (PlaceholderCharacterModel default) until custom visuals exist.
/// </summary>
public class DrunkenMaster : PlaceholderCharacterModel
{
    public const string CharacterId = "DrunkenMaster";

    // Warm amber, like a pint held up to the light.
    public static readonly Color Color = new("e0a13a");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Masculine;
    public override int StartingHp => 72;

    // Spec §3: 4 Strike, 4 Defend, 1 Free Pour, 1 Hard Liquor (the Bash: damage + an Ethanol Ingredient).
    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeDrunkenMaster>(),
        ModelDb.Card<StrikeDrunkenMaster>(),
        ModelDb.Card<StrikeDrunkenMaster>(),
        ModelDb.Card<StrikeDrunkenMaster>(),
        ModelDb.Card<DefendDrunkenMaster>(),
        ModelDb.Card<DefendDrunkenMaster>(),
        ModelDb.Card<DefendDrunkenMaster>(),
        ModelDb.Card<DefendDrunkenMaster>(),
        ModelDb.Card<FreePour>(),
        ModelDb.Card<HardLiquor>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<TavernRag>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<DrunkenMasterCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<DrunkenMasterRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<DrunkenMasterPotionPool>();

    /// <summary>
    /// Combat visuals from a single painted PNG (see DEV_NOTES for sizing). Falls back to the
    /// Ironclad placeholder rig while the image is missing.
    /// </summary>
    public const string CombatImagePath = MainFile.ResPath + "/images/character/drunken_master.png";

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        if (!ResourceLoader.Exists(CombatImagePath)) return null;
        return NodeFactory<NCreatureVisuals>.CreateFromResource(CombatImagePath);
    }

    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    public override string CustomIconTexturePath => "character_icon_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();
}
