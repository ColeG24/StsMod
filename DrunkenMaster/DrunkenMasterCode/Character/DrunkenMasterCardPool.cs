using BaseLib.Abstracts;
using DrunkenMaster.DrunkenMasterCode.Extensions;
using Godot;

namespace DrunkenMaster.DrunkenMasterCode.Character;

public class DrunkenMasterCardPool : CustomCardPoolModel
{
    public override string Title => DrunkenMaster.CharacterId; //This is not a display name.
    
    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();


    /* These HSV values will determine the color of your card back.
    They are applied as a shader onto an already colored image,
    so it may take some experimentation to find a color you like.
    Generally they should be values between 0 and 1. */
    // Dark amber (2026-09-16). These feed the game's res://shaders/hsv.gdshader over the shared red frame textures, the same
    // way the base pools do (Ironclad 0.025 / 0.85 / 1, Regent 0.12 / 1.5 / 1.2); 1 / 1 / 1 left the raw red, i.e. Ironclad's look.
    public override float H => 0.10f; //Hue; changes the color.
    public override float S => 1.1f; //Saturation
    public override float V => 0.85f; //Brightness
    
    //Alternatively, leave these values at 1 and provide a custom frame image.
    /*public override Texture2D CustomFrame(CustomCardModel card)
    {
        //This will attempt to load DrunkenMaster/images/cards/frame.png
        return PreloadManager.Cache.GetTexture2D("cards/frame.png".ImagePath());
    }*/

    //Color of small card icons
    public override Color DeckEntryCardColor => new("B8650A"); // matches the frame tint; base pools use their frame colour here (Regent E36600)
    
    public override bool IsColorless => false;
}