using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using DrunkenMaster.DrunkenMasterCode.Character;
using DrunkenMaster.DrunkenMasterCode.Extensions;
using DrunkenMaster.DrunkenMasterCode.Resources;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace DrunkenMaster.DrunkenMasterCode.Cards;

/// <summary>
/// This is the base class for your mod's cards, which is set up to load the card's images from your mod's resources.
/// When creating a card, right click the Cards folder and create a new file with the Custom Card template.
/// This will generate a class that extends this one.
/// You can also just create the class manually; just make sure to inherit from this class.
/// </summary>
[Pool(typeof(DrunkenMasterCardPool))]
public abstract class DrunkenMasterCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    //Image size:
    //Normal art: 1000x760 (Using 500x380 should also work, it will simply be scaled.)
    //Full art: 606x852
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    
    //Smaller variants of card images for efficiency:
    //Smaller variant of fullart: 250x350
    //Smaller variant of normalart: 250x190
    
    //Uses card_portraits/card_name.png as image path. These should be smaller images.
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    /// <summary>
    /// Whether the constructor gave this card an Intoxication cost. Plain model state on purpose (2026-09-15 co-op fix):
    /// CardModel.DeepCloneFields builds the clone's keyword list DURING the clone, before BaseLib's SpireField copy has run,
    /// so reading CustomResources.Cost(this) there returned null and the clone was cached without Poised whenever the
    /// canonical card's keywords had not been computed yet on that machine. Two players' Rimshots then hashed differently.
    /// A field is copied by MemberwiseClone first, so the keyword list is the same on every machine and every copy.
    /// </summary>
    private bool _hasIntoxicationCost;

    /// <summary>Give the card a fixed Intoxication cost. Call from the constructor instead of CustomResources directly.</summary>
    protected void SetIntoxicationCost(int cost)
    {
        CustomResources<IntoxicationResource>.SetCanonicalCost(this, cost);
        _hasIntoxicationCost = true;
    }

    /// <summary>Give the card an X Intoxication cost. Call from the constructor instead of CustomResources directly.</summary>
    protected void SetIntoxicationXCost()
    {
        CustomResources<IntoxicationResource>.SetXCost(this);
        _hasIntoxicationCost = true;
    }

    /// <summary>
    /// Second lock on Intoxication costs (2026-09-17). BaseLib enforces custom resource costs with one postfix on
    /// PlayerCombatState.HasEnoughResourcesFor, and its Spend only warns and spends what is there, so any other mod
    /// that forces that method's result to true lets these cards be played for nothing (a playtester's log: Hurl at 0,
    /// "Attempted to spend secondary resource IntoxicationResource with insufficient amount; Current: 0 | Required: 3").
    /// CardModel.CanPlay checks IsPlayable separately, so the same BaseLib check is repeated here; it goes through the
    /// card's own cost object, so free-this-turn, cost modifiers and X costs behave exactly as before. Only manual play
    /// consults IsPlayable: a Blackout auto-play is unaffected.
    /// </summary>
    protected override bool IsPlayable
    {
        get
        {
            if (!_hasIntoxicationCost) return true;
            var state = Owner?.PlayerCombatState;
            var cost = CustomResources<IntoxicationResource>.Cost(this);
            if (state == null || cost == null) return true;
            return cost.ResourceCheck(state, this) == UnplayableReason.None;
        }
    }

    /// <summary>
    /// Every card that costs Intoxication is Poised: Drunk never re-rolls its cost. Subclasses that override this
    /// (Ingredients, Chaser, Upper Deckie, Chug, Drink to Forget) list their keywords explicitly. Ingredients carry a
    /// canonical Intoxication cost of 0 for Shot Glass (2026-09-19) and are deliberately not Poised.
    /// </summary>
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        _hasIntoxicationCost ? new[] { DrunkenMasterKeywords.Poised } : Array.Empty<CardKeyword>();
}
