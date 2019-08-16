using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GbSoundEmuNet
{
    public class Gb_Apu
    {
        public const ushort start_addr = 0xff10;
        public const ushort end_addr = 0xff3f;
        public const int register_count = end_addr - start_addr + 1;

        public void write_register(int x, ushort address, byte data)
        {
        }

        public byte read_register(int x, ushort address)
        {
            return 0;
        }

        public bool end_frame(int x)
        {
            return true;
        }

        public void output(Blip_Buffer center, Blip_Buffer left, Blip_Buffer right)
        {
        }
    }
}
