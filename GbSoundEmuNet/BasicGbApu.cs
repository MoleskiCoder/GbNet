namespace GbSoundEmuNet
{
    public class BasicGbApu
    {
        private const int FrameLength = 70224;

        private readonly GbApu apu = new GbApu();
        private readonly StereoBuffer buf = new StereoBuffer();
        private long time = 0;

        public long SampleRate
        {
            set
            {
                this.apu.Output(this.buf.Center, this.buf.Left, this.buf.Right);
                this.buf.ClockRate = 4194304;
                this.buf.SetSampleRate(value);
            }
        }

        public long SamplesAvailable => this.buf.SamplesAvailable;

        private long Clock => this.time += 4;

        public void WriteRegister(ushort addr, byte data) => this.apu.WriteRegister((int)this.Clock, addr, data);

        public byte ReadRegister(ushort addr) => this.apu.ReadRegister((int)this.Clock, addr);

        public void EndFrame()
        {
            this.time = 0;
            var stereo = this.apu.EndFrame(FrameLength);
            this.buf.EndFrame(FrameLength, stereo);
        }

        public int ReadSamples(short[] destination) => this.buf.ReadSamples(destination);
    }
}
