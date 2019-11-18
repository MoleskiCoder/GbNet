namespace GbNet
{
    public class Board : EightBit.GameBoy.Bus
    {
        private readonly Configuration configuration;
        private readonly EightBit.GameBoy.Disassembler disassembler;

        public Board(Configuration configuration)
        {
            this.configuration = configuration;
            this.disassembler = new EightBit.GameBoy.Disassembler(this);
        }

        public override void Initialize()
        {
            if (this.configuration.DebugMode)
            {
                this.CPU.ExecutingInstruction += this.CPU_ExecutingInstruction_Debug;
            }

            this.LoadBootRom(this.configuration.RomDirectory + "/DMG_ROM.bin");
        }

        public void Plug(string path) => this.LoadGameRom(this.configuration.RomDirectory + "/" + path);

        private void CPU_ExecutingInstruction_Debug(object sender, System.EventArgs e)
        {
            if (this.IO.BootRomDisabled)
            {
                System.Console.Error.WriteLine($"{EightBit.GameBoy.Disassembler.State(this.CPU)} {this.disassembler.Disassemble(this.CPU)}");
            }
        }
    }
}
