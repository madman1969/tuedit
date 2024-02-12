using Terminal.Gui;
using tuiedit;

Application.Init();
Application.Top.ColorScheme = Colors.Base;

try
{
    Application.Run<TabEdit>();
}
finally
{
    Application.Shutdown();
}
