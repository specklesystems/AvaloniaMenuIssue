using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace AvaloniaMenuIssue
{
  public class App : Application
  {
    public override void Initialize()
    {
      AvaloniaXamlLoader.Load(this);
      this.Name = "Speckle";
    }

    public override void OnFrameworkInitializationCompleted()
    {

      base.OnFrameworkInitializationCompleted();
    }
  }
}
