using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GbSoundEmuNet
{
    public class Sound_Queue
    {
        public Sound_Queue()
        {

        }

        // Initialize with specified sample rate and channel count.
        // Returns NULL on success, otherwise error string.
        public void start(long sample_rate, int chan_count = 1)
        {

        }

        // Number of samples in buffer waiting to be played
        public int sample_count()
        {
            return 0;
        }


        public void write(short[] x, int count)
        {
        }
    }
}
