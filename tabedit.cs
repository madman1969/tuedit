namespace tuiedit
{
    using Terminal.Gui;

    public partial class TabEdit : Window
    {
        TabView tabView;

        private int numbeOfNewTabs = 1;

        public TabEdit()
        {
            var menu = new MenuBar(
                new MenuBarItem[]
                {
                    new MenuBarItem(
                        "_File",
                        new MenuItem[]
                        {
                            new MenuItem("_New", "", () => New()),
                            new MenuItem("_Open", "", () => Open()),
                            new MenuItem("_Save", "", () => Save()),
                            new MenuItem("Save _As", "", () => SaveAs()),
                            new MenuItem("_Close", "", () => Close()),
                            new MenuItem("_Quit", "", () => Quit()),
                        }
                    )
                }
            );

            Add(menu);

            tabView = new TabView()
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill(1),
            };

            tabView.TabClicked += TabView_TabClicked;

            tabView.Style.ShowBorder = true;
            tabView.ApplyStyleChanges();

            Add(tabView);

            var lenStatusItem = new StatusItem(Key.CharMask, "Len: ", null);
            var siCursorPosition = new StatusItem(Key.Null, "", null);

            var statusBar = new StatusBar(
                new StatusItem[]
                {
                    siCursorPosition,
                    new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Quit", () => Quit()),
                    // These shortcut keys don't seem to work correctly in linux
                    //new StatusItem(Key.CtrlMask | Key.N, "~^O~ Open", () => Open()),
                    //new StatusItem(Key.CtrlMask | Key.N, "~^N~ New", () => New()),

                    new StatusItem(Key.CtrlMask | Key.S, "~^S~ Save", () => Save()),
                    new StatusItem(Key.CtrlMask | Key.W, "~^W~ Close", () => Close()),
                    lenStatusItem,
                }
            );

            tabView.SelectedTabChanged += (s, e) =>
                lenStatusItem.Title = $"Len:{(e.NewTab?.View?.Text?.Length ?? 0)}";

            Add(statusBar);

            New();
        }

        private void TabView_TabClicked(object sender, TabView.TabMouseEventArgs e)
        {
            // Reset the cursor saved cursor ...
            if (e.Tab != null)
            {
                var editor = e.Tab.View as TextView;
                var tmp = e.Tab as OpenFileTab;
                editor.CursorPosition = tmp.lastCursorPosition;
                editor.DesiredCursorVisibility = CursorVisibility.Underline;
            }

            // we are only interested in right clicks
            if (!e.MouseEvent.Flags.HasFlag(MouseFlags.Button3Clicked))
            {
                return;
            }

            MenuBarItem items;

            if (e.Tab == null)
            {
                items = new MenuBarItem(
                    new MenuItem[] { new MenuItem($"Open", "", () => Open()), }
                );
            }
            else
            {
                items = new MenuBarItem(
                    new MenuItem[]
                    {
                        new MenuItem($"Save", "", () => Save(e.Tab)),
                        new MenuItem($"Close", "", () => Close(e.Tab)),
                    }
                );
            }

            var contextMenu = new ContextMenu(e.MouseEvent.X + 1, e.MouseEvent.Y + 1, items);

            contextMenu.Show();
            e.MouseEvent.Handled = true;
        }

        private void New()
        {
            Open("", null, $"new {numbeOfNewTabs++}");
        }

        private void Close()
        {
            Close(tabView.SelectedTab);
        }

        private void Close(TabView.Tab tabToClose)
        {
            var tab = tabToClose as OpenFileTab;

            if (tab == null)
            {
                return;
            }

            if (tab.UnsavedChanges)
            {
                var result = MessageBox.Query(
                    "Save Changes",
                    $"Save changes to {tab.Text.ToString().TrimEnd('*')}",
                    "Yes",
                    "No",
                    "Cancel"
                );

                if (result == -1 || result == 2)
                {
                    // user cancelled
                    return;
                }

                if (result == 0)
                {
                    tab.Save();
                }
            }

            // close and dispose the tab
            tabView.RemoveTab(tab);
            tab.View.Dispose();
        }

        private void Open()
        {
            var open = new OpenDialog("Open", "Open a file") { AllowsMultipleSelection = true };

            Application.Run(open);

            if (!open.Canceled)
            {
                foreach (var path in open.FilePaths)
                {
                    if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    {
                        return;
                    }

                    Open(File.ReadAllText(path), new FileInfo(path), Path.GetFileName(path));
                }
            }
        }

        /// <summary>
        /// Creates a new tab with initial text
        /// </summary>
        /// <param name="initialText"></param>
        /// <param name="fileInfo">File that was read or null if a new blank document</param>
        private void Open(string initialText, FileInfo fileInfo, string tabName)
        {
            var textView = new SyntaxColoredTextView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                Text = initialText
            };

            var tab = new OpenFileTab(tabName, fileInfo, textView);
            tabView.AddTab(tab, true);

            // when user makes changes rename tab to indicate unsaved
            textView.KeyUp += (k) =>
            {
                // if current text doesn't match saved text
                var areDiff = tab.UnsavedChanges;

                if (areDiff)
                {
                    if (!tab.Text.ToString().EndsWith('*'))
                    {
                        tab.Text = tab.Text.ToString() + '*';
                        tabView.SetNeedsDisplay();
                    }
                }
                else
                {
                    if (tab.Text.ToString().EndsWith('*'))
                    {
                        tab.Text = tab.Text.ToString().TrimEnd('*');
                        tabView.SetNeedsDisplay();
                    }
                }
            };
        }

        public void Save()
        {
            Save(tabView.SelectedTab);
        }

        public void Save(TabView.Tab tabToSave)
        {
            var tab = tabToSave as OpenFileTab;

            if (tab == null)
            {
                return;
            }

            if (tab.fileBeingEdited == null)
            {
                SaveAs();
            }

            tab.Save();
            tabView.SetNeedsDisplay();
        }

        public bool SaveAs()
        {
            var tab = tabView.SelectedTab as OpenFileTab;

            if (tab == null)
            {
                return false;
            }

            var fd = new SaveDialog();
            Application.Run(fd);

            if (string.IsNullOrWhiteSpace(fd.FilePath?.ToString()))
            {
                return false;
            }

            tab.fileBeingEdited = new FileInfo(fd.FilePath.ToString());
            tab.Text = fd.FileName.ToString();
            tab.Save();

            return true;
        }

        private void Quit()
        {
            Application.RequestStop();
        }
    }
}
