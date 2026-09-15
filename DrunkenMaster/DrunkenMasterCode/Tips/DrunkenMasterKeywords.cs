using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace DrunkenMaster.DrunkenMasterCode.Tips;

/// <summary>
/// Custom card keywords. Localization lives in card_keywords.json under DRUNKENMASTER-POISED.
/// </summary>
public static class DrunkenMasterKeywords
{
    /// <summary>
    /// Poised (2026-09-14): the card's cost is never re-rolled by Drunk. Every card with an Intoxication cost has it
    /// (<see cref="Cards.DrunkenMasterCard.CanonicalKeywords"/>); <see cref="Resources.IntoxicationResource.RandomizeCost"/>
    /// skips it. BaseLib writes "Poised." at the front of the description.
    /// </summary>
    [CustomEnum("Poised")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Poised;
}
