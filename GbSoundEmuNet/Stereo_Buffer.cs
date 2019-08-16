using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GbSoundEmuNet
{
    public class Stereo_Buffer
    {
        private long samplesAvailable;

        public long SamplesAvailable => this.samplesAvailable;

        public long ClockRate { get; set; }

        public Blip_Buffer Center { get; } = new Blip_Buffer();

        public Blip_Buffer Left { get; } = new Blip_Buffer();

        public Blip_Buffer Right { get; } = new Blip_Buffer();

        public void SetSampleRate(long x, int msec = 0)
        {
        }

        public void EndFrame(int x, bool addedStereo = true)
        {
        }

        public int ReadSamples(short[] x)
        {
            return 0;
        }
    }
}
