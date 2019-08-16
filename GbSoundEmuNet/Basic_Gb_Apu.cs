namespace GbSoundEmuNet
{
    public class Basic_Gb_Apu
    {
        private const int FrameLength = 70224;

        private readonly Gb_Apu apu = new Gb_Apu();
        private readonly Stereo_Buffer buffer = new Stereo_Buffer();
        private int time = 0;

        public void set_sample_rate(long rate)
        {
            this.apu.output(this.buffer.Center, this.buffer.Left, this.buffer.Right);
            this.buffer.ClockRate = 4194304;
            this.buffer.SetSampleRate(rate);
        }

        public void write_register(ushort address, byte data) => this.apu.write_register(this.clock(), address, data);

        public byte read_register(ushort address) => this.apu.read_register(this.clock(), address);

        public void end_frame()
        {
            this.time = 0;
            var stereo = this.apu.end_frame(FrameLength);
            this.buffer.EndFrame(FrameLength, stereo);
        }

        public long samples_avail() => this.buffer.SamplesAvailable;

        public int read_samples(short[] destination) => this.buffer.ReadSamples(destination);

        private int clock() => this.time += 4;
    }
}
