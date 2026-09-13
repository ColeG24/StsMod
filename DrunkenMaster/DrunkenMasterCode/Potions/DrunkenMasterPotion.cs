using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using DrunkenMaster.DrunkenMasterCode.Character;
using DrunkenMaster.DrunkenMasterCode.Extensions;

namespace DrunkenMaster.DrunkenMasterCode.Potions;

[Pool(typeof(DrunkenMasterPotionPool))]
public abstract class DrunkenMasterPotion : CustomPotionModel
{
	public override string? CustomPackedImagePath =>
		$"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();
	public override string? CustomPackedOutlinePath =>
		$"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionOutlineImagePath();
}