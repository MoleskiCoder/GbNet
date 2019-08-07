using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GbSoundEmuNet
{
    public class StereoBuffer
    {
        private long samplesAvailable;

        public long SamplesAvailable => this.samplesAvailable;

        public long ClockRate { get; set; }

        public BlipBuffer Center { get; } = new BlipBuffer();

        public BlipBuffer Left { get; } = new BlipBuffer();

        public BlipBuffer Right { get; } = new BlipBuffer();

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
