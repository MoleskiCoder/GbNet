﻿namespace GbNet
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using GbSoundEmuNet;

    public class Cabinet : Game
    {
        private const int DisplayScale = 2;
        private const int DisplayWidth = LR35902.DisplayCharacteristics.RasterWidth;
        private const int DisplayHeight = LR35902.DisplayCharacteristics.RasterHeight;

        private const int AudioOutputBufferSize = 4096;
        private const int AudioSampleRate = 44100;

        private readonly Configuration configuration;
        private readonly ColourPalette palette = new();
        private readonly LR35902.Display<Color> lcd;

        private readonly List<Keys> pressed = [];

        private readonly Basic_Gb_Apu apu = new();
        private readonly Sound_Queue audioQueue = new();
        private readonly short[] audioOutputBuffer = new short[AudioOutputBufferSize];

        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch? spriteBatch;
        private Texture2D? bitmapTexture;

        private bool disposed;

        public Cabinet(Configuration configuration)
        {
            this.configuration = configuration;
            this.Motherboard = new Board(configuration);
            this.lcd = new LR35902.Display<Color>(this.palette, this.Motherboard, this.Motherboard.OAMRAM, this.Motherboard.VRAM);

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
            this.InitialiseAudio();

            this.Motherboard.IO.DisplayStatusModeUpdated += this.IO_DisplayStatusModeUpdated;

            this.Motherboard.ReadingByte += this.Motherboard_ReadingByte;
            this.Motherboard.WrittenByte += this.Motherboard_WrittenByte;
        }

        private void Motherboard_WrittenByte(object? sender, EventArgs e)
        {
            var address = this.Motherboard.Address.Word;
            if (address is > Gb_Apu.start_addr and <= Gb_Apu.end_addr)
            {
                this.apu.write_register(address, this.Motherboard.Data);
            }
        }

        private void Motherboard_ReadingByte(object? sender, EventArgs e)
        {
            var address = this.Motherboard.Address.Word;
            if (address is >= Gb_Apu.start_addr and <= Gb_Apu.end_addr)
            {
                var value = this.apu.read_register(address);
                this.Motherboard.Poke(address, value);
            }
        }

        private void IO_DisplayStatusModeUpdated(object? sender, LR35902.LcdStatusModeEventArgs e)
        {
            switch (e.Mode)
            {
                case LR35902.LcdStatusMode.HBlank:
                    break;
                case LR35902.LcdStatusMode.VBlank:
                    break;
                case LR35902.LcdStatusMode.SearchingOamRam:
                    this.lcd.LoadObjectAttributes();
                    break;
                case LR35902.LcdStatusMode.TransferringDataToLcd:
                    this.lcd.Render();
                    break;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            this.CheckKeyboard();
            this.DrawFrame();
            this.EndAudioFrame();
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            this.DisplayTexture();
        }

        //protected override void OnExiting(object sender, EventArgs args)
        //{
        //    base.OnExiting(sender, args);
        //    this.Motherboard.LowerPOWER();
        //}

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

        private void CheckKeyboard()
        {
            var state = Keyboard.GetState();
            var current = new HashSet<Keys>(state.GetPressedKeys());

            var newlyReleased = this.pressed.Except(current);
            this.UpdateReleasedKeys(newlyReleased);

            var newlyPressed = current.Except(this.pressed);
            this.UpdatePressedKeys(newlyPressed);

            this.pressed.Clear();
            this.pressed.AddRange(current);
        }

        private void UpdatePressedKeys(IEnumerable<Keys> keys)
        {
            foreach (var key in keys)
            {
                switch (key)
                {
                    case Keys.Up:
                        this.Motherboard.IO.PressUp();
                        break;
                    case Keys.Down:
                        this.Motherboard.IO.PressDown();
                        break;
                    case Keys.Left:
                        this.Motherboard.IO.PressLeft();
                        break;
                    case Keys.Right:
                        this.Motherboard.IO.PressRight();
                        break;
                    case Keys.Z:
                        this.Motherboard.IO.PressB();
                        break;
                    case Keys.X:
                        this.Motherboard.IO.PressA();
                        break;
                    case Keys.Back:
                        this.Motherboard.IO.PressSelect();
                        break;
                    case Keys.Enter:
                        this.Motherboard.IO.PressStart();
                        break;
                }
            }
        }

        private void UpdateReleasedKeys(IEnumerable<Keys> keys)
        {
            foreach (var key in keys)
            {
                switch (key)
                {
                    case Keys.Up:
                        this.Motherboard.IO.ReleaseUp();
                        break;
                    case Keys.Down:
                        this.Motherboard.IO.ReleaseDown();
                        break;
                    case Keys.Left:
                        this.Motherboard.IO.ReleaseLeft();
                        break;
                    case Keys.Right:
                        this.Motherboard.IO.ReleaseRight();
                        break;
                    case Keys.Z:
                        this.Motherboard.IO.ReleaseB();
                        break;
                    case Keys.X:
                        this.Motherboard.IO.ReleaseA();
                        break;
                    case Keys.Back:
                        this.Motherboard.IO.ReleaseSelect();
                        break;
                    case Keys.Enter:
                        this.Motherboard.IO.ReleaseStart();
                        break;
                }
            }
        }

        private void DrawFrame()
        {
            this.Motherboard.RunVerticalBlankLines();
            this.Motherboard.RunRasterLines();
            this.bitmapTexture?.SetData(this.lcd.Pixels);
        }

        private void DisplayTexture()
        {
            this.spriteBatch?.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            this.spriteBatch?.Draw(this.bitmapTexture, Vector2.Zero, null, Color.White, 0.0F, Vector2.Zero, DisplayScale, SpriteEffects.None, 0.0F);
            this.spriteBatch?.End();
        }

        private void ChangeResolution(int width, int height)
        {
            this.graphics.PreferredBackBufferWidth = DisplayScale * width;
            this.graphics.PreferredBackBufferHeight = DisplayScale * height;
            this.graphics.ApplyChanges();
        }

        private void InitialiseAudio()
        {
            this.apu.set_sample_rate(AudioSampleRate);
            this.audioQueue.start(AudioSampleRate, 2);
        }

        private void EndAudioFrame()
        {
            this.apu.end_frame();
            if (this.apu.samples_avail() >= AudioOutputBufferSize)
            {
                var count = this.apu.read_samples(this.audioOutputBuffer);
                this.audioQueue.write(this.audioOutputBuffer, count);
            }
        }
    }
}
