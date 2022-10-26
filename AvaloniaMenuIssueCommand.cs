using System;
using System.Collections.Generic;
using Avalonia;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace AvaloniaMenuIssue
{
  public class AvaloniaMenuIssueCommand : Command
  {
    public AvaloniaMenuIssueCommand()
    {
      // Rhino only creates one instance of each command class defined in a
      // plug-in, so it is safe to store a refence in a static property.
      Instance = this;
    }

    ///<summary>The only instance of this command.</summary>
    public static AvaloniaMenuIssueCommand Instance { get; private set; }

    public static void InitAvalonia()
    {
      try
      {
        BuildAvaloniaApp().SetupWithoutStarting();
      }
      catch(Exception e)
      {

      }
      
    }

    public static AppBuilder BuildAvaloniaApp()
    {
      return AppBuilder.Configure<App>()
      .UsePlatformDetect()
      .With(new X11PlatformOptions { UseGpu = false })
      .With(new AvaloniaNativePlatformOptions { UseGpu = false, UseDeferredRendering = true })
      .With(new MacOSPlatformOptions { ShowInDock = false, DisableDefaultApplicationMenuItems = true, DisableNativeMenus = true })
      .With(new Win32PlatformOptions { AllowEglInitialization = true, EnableMultitouch = false })
      .With(new SkiaOptions { MaxGpuResourceSizeBytes = 8096000 })
      .LogToTrace();
    }

    ///<returns>The command name as it appears on the Rhino command line.</returns>
    public override string EnglishName => "AvaloniaMenuIssueCommand";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      InitAvalonia();
      var mw = new MainWindow();
      mw.Show();

      return Result.Success;
    }
  }
}

