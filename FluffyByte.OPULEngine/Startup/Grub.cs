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

        Heartbeat hb = new(TimeSpan.FromMilliseconds(50));

        hb.OnTick += tick =>
        {
            if(tick % 20 == 0) // once per second at 20 Hz
                Scribe.Info($"Heartbeat tick #{tick} (~ {tick / 20} seconds elapsed)");
        };

        hb.Start();

        await Task.Delay(5000);
        await hb.StopAsync();

        await Constellations.Instance.SaveSettings();
        Scribe.Info("Constellations settings saved.");
    }
}