namespace GbNet
{
    public class Board : LR35902.Bus
    {
        private readonly Configuration configuration;
        private readonly LR35902.Disassembler disassembler;

        public Board(Configuration configuration)
        {
            this.configuration = configuration;
            this.disassembler = new LR35902.Disassembler(this);
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
                System.Console.Error.WriteLine($"{LR35902.Disassembler.State(this.CPU)} {this.disassembler.Disassemble(this.CPU)}");
            }
        }
    }
}
