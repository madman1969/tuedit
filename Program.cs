using Terminal.Gui;
using tuiedit;

//Console.SetBufferSize(120, 43);
//Console.SetWindowSize(120, 43);

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
