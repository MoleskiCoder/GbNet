namespace GbSoundEmuNet
{
    public class BasicGbApu
    {
        private const int FrameLength = 70224;

        private readonly GbApu apu = new GbApu();
        private readonly StereoBuffer buffer = new StereoBuffer();
        private int time = 0;

        public long SampleRate
        {
            set
            {
                this.apu.Output(this.buffer.Center, this.buffer.Left, this.buffer.Right);
                this.buffer.ClockRate = 4194304;
                this.buffer.SetSampleRate(value);
            }
        }

        public long SamplesAvailable => this.buffer.SamplesAvailable;

        private int Clock => this.time += 4;

        public void WriteRegister(ushort address, byte data) => this.apu.WriteRegister(this.Clock, address, data);

        public byte ReadRegister(ushort address) => this.apu.ReadRegister(this.Clock, address);

        public void EndFrame()
        {
            this.time = 0;
            var stereo = this.apu.EndFrame(FrameLength);
            this.buffer.EndFrame(FrameLength, stereo);
        }

        public int ReadSamples(short[] destination) => this.buffer.ReadSamples(destination);
    }
}
