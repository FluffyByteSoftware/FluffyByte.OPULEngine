using FluffyByte.OPULEngine.Tools;

namespace FluffyByte.OPULEngine.Startup;

public class Grub
{

    public static async Task Main(string[] args)
    {
        if(args.Length == 0)
        {
            Scribe.Instance.Info("No arguments provided. Starting in default mode.");
        }

        await Constellations.Instance.LoadSettings();
        Scribe.Instance.Info("Constellations settings loaded.");

    }
}