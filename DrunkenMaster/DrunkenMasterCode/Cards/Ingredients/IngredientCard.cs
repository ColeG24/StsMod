using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;

/// <summary>
/// Spec §5. Ingredients are generated 0-cost Exhaust tokens. Playing one has NO immediate effect;
/// it is added to the Brew. Unplayed Ingredients vanish at end of turn (Ethereal).
/// Subclasses declare the effect they contribute to a Concoction.
/// </summary>
public abstract class IngredientCard() : DrunkenMasterCard(0, CardType.Skill, CardRarity.Token, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];

    // Ingredients are never produced by "add a random card" effects; only by the mod's own generators.
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(DrunkenMasterTips.Ingredient),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];

    /// <summary>
    /// The text this ingredient contributes to a Concoction's description, from the card's
    /// `.brewText` localization entry with this card's variables filled in.
    /// </summary>
    public string BrewEffectText => GetBrewEffectText(1);

    /// <summary>
    /// The brew text with every numeric variable multiplied by <paramref name="count"/>, so two Hair of
    /// the Dog read "Draw 2 cards" instead of "Draw 1 card. Draw 1 card." (Concoction collapses duplicates.)
    /// </summary>
    public string GetBrewEffectText(int count)
    {
        var loc = new LocString("cards", Id.Entry + ".brewText");
        if (!loc.Exists()) return count > 1 ? $"{Title} x{count}" : Title;
        if (count <= 1) DynamicVars.AddTo(loc);
        else foreach (var (name, dynamicVar) in DynamicVars) loc.Add(name, dynamicVar.BaseValue * count);
        // Lets brewText use {IfUpgraded:show:upgraded text|normal text}, same as card descriptions.
        loc.Add(new IfUpgradedVar(IsUpgraded ? UpgradeDisplay.Upgraded : UpgradeDisplay.Normal));
        return loc.GetFormattedText();
    }

    /// <summary>True if this ingredient's brewed effect resolves on an enemy (damage, debuffs).</summary>
    public abstract bool TargetsEnemy { get; }

    /// <summary>
    /// Apply this ingredient's contribution when the Concoction containing it is drunk.
    /// Self-facing effects use <paramref name="drinker"/>; enemy-facing effects use <paramref name="enemyTarget"/>,
    /// which is non-null only when the Concoction was targeted.
    /// </summary>
    public abstract Task ApplyBrewedEffect(PlayerChoiceContext choiceContext, Player drinker, Creature? enemyTarget);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await BrewSystem.AddIngredient(choiceContext, this);
    }

    /// <summary>
    /// Ingredients upgrade like any card (Top Shelf, Restock+). Each subclass bumps its own numbers;
    /// the modifiers (Everclear, Seltzer) change what they do instead.
    /// </summary>
    protected override abstract void OnUpgrade();
}
