using FluffyByte.OPULEngine.Tools;

namespace FluffyByte.OPULEngine.Startup;

public class Grub
{

    public static void Main(string[] args)
    {
        if(args.Length == 0)
        {
            Scribe.Info("No arguments provided. Starting in default mode.");
        }

        Constellations.Instance.LoadSettings();

        Scribe.Debug("Debug test.");
        
    }
}