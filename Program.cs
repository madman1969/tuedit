using tuitest;
using Terminal.Gui;

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