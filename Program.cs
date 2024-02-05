using Terminal.Gui;
using tuitest;

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
