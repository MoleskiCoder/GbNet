namespace GbNet
{
    using Microsoft.Xna.Framework;

    public class ColourPalette : LR35902.AbstractColourPalette<Color>
    {
        public void Load()
        {
            this.Colours.Add(new Color(0x9C, 0xBD, 0x0F));  // Off
            this.Colours.Add(new Color(0x8C, 0xAD, 0x0F));  // Light
            this.Colours.Add(new Color(0x30, 0x62, 0x30));  // Medium
            this.Colours.Add(new Color(0x0F, 0x38, 0x0F));  // Dark
        }
    }
}
