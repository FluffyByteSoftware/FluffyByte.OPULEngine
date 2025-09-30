using FluffyByte.OPULEngine.TickSystem;
using FluffyByte.OPULEngine.Tools;

namespace FluffyByte.OPULEngine.Startup;

public class Grub
{

    public static async Task Main(string[] args)
    {
        if(args.Length == 0)
        {
            Scribe.Info("No arguments provided. Starting in default mode.");
        }

        await Constellations.Instance.LoadSettings();
        Scribe.Info("Constellations settings loaded.");
        
        await Constellations.Instance.SaveSettings();
        Scribe.Info("Constellations settings saved.");
    }
}