using BaseLib.Abstracts;
using DrunkenMaster.DrunkenMasterCode.Extensions;
using Godot;

namespace DrunkenMaster.DrunkenMasterCode.Character;

public class DrunkenMasterRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => DrunkenMaster.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}