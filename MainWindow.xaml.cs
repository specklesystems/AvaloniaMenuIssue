using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.ComponentModel;

using Avalonia.Input;

namespace AvaloniaMenuIssue
{
  public partial class MainWindow : Window
  {
    public MainWindow()
    { 
      AvaloniaXamlLoader.Load(this);


#if DEBUG
      this.AttachDevTools(KeyGesture.Parse("CTRL+R"));
#endif
    }


  }
}
