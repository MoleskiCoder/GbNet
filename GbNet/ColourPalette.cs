namespace GbNet
{
    using Microsoft.Xna.Framework;
    using EightBit.GameBoy;

    public class ColourPalette : AbstractColourPalette<Color>
    {
        public void Load()
        {
            this.Colours[(int)ColourShades.Off] = new Color(0x9C, 0xBD, 0x0F);
            this.Colours[(int)ColourShades.Light] = new Color(0x8C, 0xAD, 0x0F);
            this.Colours[(int)ColourShades.Medium] = new Color(0x30, 0x62, 0x30);
            this.Colours[(int)ColourShades.Dark] = new Color(0x0F, 0x38, 0x0F);
        }
    }
}
