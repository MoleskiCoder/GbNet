using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GbSoundEmuNet
{
    public class GbApu
    {
        public const ushort StartAddress = 0xff10;
        public const ushort EndAddress = 0xff3f;

        public void WriteRegister(int x, ushort address, byte data)
        {
        }

        public byte ReadRegister(int x, ushort address)
        {
            return 0;
        }

        public bool EndFrame(int x)
        {
            return true;
        }

        public void Output(BlipBuffer center, BlipBuffer left, BlipBuffer right)
        {
        }
    }
}
