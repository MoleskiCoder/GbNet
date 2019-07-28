namespace GbNet
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    using EightBit.GameBoy;
    using System;

    public class Cabinet : Game
    {
        private const int DisplayScale = 2;
        private const int DisplayWidth = DisplayCharacteristics.RasterWidth;
        private const int DisplayHeight = DisplayCharacteristics.RasterHeight;

        private readonly Configuration configuration;
        private readonly ColourPalette palette = new ColourPalette();
        private readonly EightBit.GameBoy.Display<Color> lcd;

        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D bitmapTexture;

        private int cycles = 0;

        private bool disposed = false;

        public Cabinet(Configuration configuration)
        {
            this.configuration = configuration;
            this.Motherboard = new Board(configuration);
            this.lcd = new Display<Color>(this.palette, this.Motherboard, this.Motherboard.OAMRAM, this.Motherboard.VRAM);

            this.graphics = new GraphicsDeviceManager(this)
            {
                IsFullScreen = false,
            };
        }

        public Board Motherboard { get; }

        public void Plug(string path) => this.Motherboard.Plug(path);

        protected override void Initialize()
        {
            base.Initialize();
            this.Motherboard.Initialize();
            this.Motherboard.RaisePOWER();
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.bitmapTexture = new Texture2D(this.GraphicsDevice, DisplayWidth, DisplayHeight);
            this.ChangeResolution(DisplayWidth, DisplayHeight);
            this.palette.Load();
            this.cycles = 0;

            this.Motherboard.IO.DisplayStatusModeUpdated += this.IO_DisplayStatusModeUpdated;
        }

        private void IO_DisplayStatusModeUpdated(object sender, LcdStatusModeEventArgs e)
        {
            switch (e.Mode & (LcdStatusMode)EightBit.Mask.Mask3)
            {
                case LcdStatusMode.HBlank:
                    break;
                case LcdStatusMode.VBlank:
                    break;
                case LcdStatusMode.SearchingOamRam:
                    this.lcd.LoadObjectAttributes();
                    break;
                case LcdStatusMode.TransferringDataToLcd:
                    this.lcd.Render();
                    break;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            this.DrawFrame();
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            this.DisplayTexture();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            this.Motherboard.LowerPOWER();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.bitmapTexture?.Dispose();
                    this.spriteBatch?.Dispose();
                    this.graphics?.Dispose();
                }

                this.disposed = true;
            }
        }

        private void DrawFrame()
        {
            this.cycles += EightBit.GameBoy.Bus.CyclesPerFrame;
            this.cycles -= this.Motherboard.RunRasterLines();
            this.cycles -= this.Motherboard.RunVerticalBlankLines();
        }

        private void DisplayTexture()
        {
            this.bitmapTexture.SetData(this.lcd.Pixels);

            this.spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            this.spriteBatch.Draw(this.bitmapTexture, Vector2.Zero, null, Color.White, 0.0F, Vector2.Zero, DisplayScale, SpriteEffects.None, 0.0F);
            this.spriteBatch.End();
        }

        private void ChangeResolution(int width, int height)
        {
            this.graphics.PreferredBackBufferWidth = DisplayScale * width;
            this.graphics.PreferredBackBufferHeight = DisplayScale * height;
            this.graphics.ApplyChanges();
        }
    }
}
