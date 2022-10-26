using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Styling;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace AvaloniaMenuIssue
{
  static class MacOSHelpers
  {
    const string LIBOBJC_DYLIB = "/usr/lib/libobjc.dylib";

    [DllImport(LIBOBJC_DYLIB, EntryPoint = "sel_registerName")]
    public extern static IntPtr GetHandle(string name);

    [DllImport(LIBOBJC_DYLIB, EntryPoint = "objc_msgSend")]
    public extern static void void_objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport(LIBOBJC_DYLIB, EntryPoint = "objc_msgSend")]
    public extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport(LIBOBJC_DYLIB, EntryPoint = "objc_msgSend")]
    public extern static void void_objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport(LIBOBJC_DYLIB)]
    internal extern static IntPtr objc_getClass(string name);

    static readonly IntPtr NSApplication_class_ptr = objc_getClass("NSApplication");
    static readonly IntPtr selSharedApplicationHandle = GetHandle("sharedApplication");
    static readonly IntPtr selSetMainMenu_Handle = GetHandle("setMainMenu:");
    static readonly IntPtr selMainMenuHandle = GetHandle("mainMenu");
    static readonly IntPtr selRetain = GetHandle("retain");
    static readonly IntPtr selRelease = GetHandle("release");
    static readonly IntPtr selDelegateHandle = GetHandle("delegate");
    static readonly IntPtr selSetDelegate_Handle = GetHandle("setDelegate:");

    static IntPtr SharedApplicationPtr => IntPtr_objc_msgSend(NSApplication_class_ptr, selSharedApplicationHandle);

    public static IntPtr MainMenu
    {
      get
      {
        // get the menu ptr
        var menuPtr = IntPtr_objc_msgSend(SharedApplicationPtr, selMainMenuHandle);
        // retain it so it doesn't go away
        void_objc_msgSend(menuPtr, selRetain);
        return menuPtr;
      }
      set
      {
        void_objc_msgSend_IntPtr(SharedApplicationPtr, selSetMainMenu_Handle, value);
      }
    }

    public static IntPtr AppDelegate
    {
      get
      {
        var delegatePtr = IntPtr_objc_msgSend(SharedApplicationPtr, selDelegateHandle);
        void_objc_msgSend(delegatePtr, selRetain);
        return delegatePtr;
      }
      set
      {
        void_objc_msgSend_IntPtr(SharedApplicationPtr, selSetDelegate_Handle, value);
      }
    }

  }

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

    public static AppBuilder appBuilder;

    static IntPtr avaloniaMenuPtr = IntPtr.Zero;
    static IntPtr avaloniaDelegatePtr = IntPtr.Zero;

    public static void InitAvalonia()
    {
      if (appBuilder != null)
        return;

      try
      {
        var rhinoMenuPtr = MacOSHelpers.MainMenu;
        var rhinoDelegate = MacOSHelpers.AppDelegate;

        appBuilder = BuildAvaloniaApp().SetupWithoutStarting();

        // don't use Avalonia's AppDelegate.. not sure what consequences this might have to Avalonia functionality
        MacOSHelpers.AppDelegate = rhinoDelegate;

        Avalonia.Controls.Window.WindowClosedEvent.Raised.Subscribe(args =>
        {
          // restore menu when closed
          Eto.Forms.Application.Instance.AsyncInvoke(() => MacOSHelpers.MainMenu = rhinoMenuPtr);
          Console.WriteLine($"Closed: Setting Rhino Menu {rhinoMenuPtr}");
        });

        Avalonia.Controls.Window.IsActiveProperty.Changed.Subscribe(args =>
        {
          if (args.NewValue.GetValueOrDefault() == true)
          {
            // no way to get the menu that exists BEFORE the avalonia window is shown as it is
            // set before this method is called..

            // Ideally we should save the value for the window before the Avalonia menu is set.

            //MacOSHelpers.AppDelegate = avaloniaDelegatePtr; // needed??
          }

          if (args.NewValue.GetValueOrDefault() == false)
          {
            // no longer the active window, so reset the menu

            // note we have to invoke because Avalonia sets the menu right after this is called.
            // this has problems as switching from one Avalonia window to another won't work,
            // or switching from Avalonia to Grasshopper.

            // I don't see a way around that at this point.
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
              MacOSHelpers.MainMenu = rhinoMenuPtr;
              //MacOSHelpers.AppDelegate = rhinoDelegate; // needed??
            });
            Console.WriteLine($"Setting Rhino Menu {rhinoMenuPtr}");
          }
        });

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
      .With(new MacOSPlatformOptions { ShowInDock = false, DisableDefaultApplicationMenuItems = true })
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

