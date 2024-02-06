namespace tuiedit
{
    using Terminal.Gui;

    public partial class TabEdit : Window
    {
        #region Fields and properties 

        TabView tabView;
        private int numbeOfNewTabs = 1;

        #endregion

        #region Class Constructor

        /// <summary>Initializes a new instance of the <see cref="TabEdit" /> class.</summary>
        public TabEdit()
        {
            // Create and add the application menu ...
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

            // Create and add the TabView ...
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

            // Create the 'cursor position & 'file length' status items ...
            var lenStatusItem = new StatusItem(Key.CharMask, "Len: ", null);
            var siCursorPosition = new StatusItem(Key.Null, "", null);

            // Create the status bar ...
            var statusBar = new StatusBar(
                new StatusItem[]
                {
                    siCursorPosition,

                    // Associate items with shortcut keys ...
                    new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Quit", () => Quit()),
                    new StatusItem(Key.CtrlMask | Key.S, "~^S~ Save", () => Save()),
                    new StatusItem(Key.CtrlMask | Key.W, "~^W~ Close", () => Close()),

                    lenStatusItem,
                }
            );

            // Add event handler for 'Selected Tab Changed' event ...
            tabView.SelectedTabChanged += (s, e) =>
                lenStatusItem.Title = $"Len:{(e.NewTab?.View?.Text?.Length ?? 0)}";

            Add(statusBar);

            // Create initial empty openedFile ...
            New();
        }

        #endregion

        #region Event Handlers

        /// <summary>Handles the TabClicked event of the TabView control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TabView.TabMouseEventArgs" /> instance containing the event data.</param>
        private void TabView_TabClicked(object sender, TabView.TabMouseEventArgs e)
        {
            // Reset the cursor saved cursor ...
            if (e.Tab != null)
            {
                var editor = e.Tab.View as TextView;
                var tmp = e.Tab as OpenedFile;
                editor.CursorPosition = tmp.lastCursorPosition;
                editor.DesiredCursorVisibility = CursorVisibility.Underline;
            }

            // We are only interested in right clicks ...
            if (!e.MouseEvent.Flags.HasFlag(MouseFlags.Button3Clicked))
            {
                return;
            }

            MenuBarItem items;

            // Empty openedFile ? ...
            if (e.Tab == null)
            {
                // Yep, so just add 'Open' to context menu ...
                items = new MenuBarItem(
                    new MenuItem[] { new MenuItem($"Open", "", () => Open()), }
                );
            }
            // Populated openedFile ? ...
            else
            {
                // Yep, so add 'Save' & 'Close' to context menu ...
                items = new MenuBarItem(
                    new MenuItem[]
                    {
                        new MenuItem($"Save", "", () => Save(e.Tab)),
                        new MenuItem($"Close", "", () => Close(e.Tab)),
                    }
                );
            }

            // Create display the context menu ...
            var contextMenu = new ContextMenu(e.MouseEvent.X + 1, e.MouseEvent.Y + 1, items);
            contextMenu.Show();

            // Flag mouse event as handled ...
            e.MouseEvent.Handled = true;
        }

        #endregion

        #region Private Methods 

        /// <summary>Added a new empty openedFile to the TabView.</summary>
        private void New()
        {
            Open("", null, $"new {numbeOfNewTabs++}");
        }

        /// <summary>Closes the currently selected openedFile.</summary>
        private void Close()
        {
            Close(tabView.SelectedTab);
        }

        /// <summary>Closes the specified openedFile.</summary>
        /// <param name="tabToClose">The openedFile to close.</param>
        private void Close(TabView.Tab tabToClose)
        {
            var openedFile = tabToClose as OpenedFile;

            // Bail if no file associated with editorTab ...
            if (openedFile == null)
            {
                return;
            }

            // Are the pending unsaved changes ? ...
            if (openedFile.UnsavedChanges)
            {
                // Prompt user to save changes ...
                int result = MessageBox.Query(
                    "Save Changes",
                    $"Save changes to {openedFile.Text.ToString().TrimEnd('*')}",
                    "Yes",
                    "No",
                    "Cancel"
                );

                // User cancelled ? ...
                if (result == -1 || result == 2)
                {
                    // Yep, so bail ...
                    return;
                }

                // User confirmed save ? ...
                if (result == 0)
                {
                    // Yep, so save pending changes ...
                    openedFile.Save();
                }
            }

            // Close editorTab and dispose ...
            tabView.RemoveTab(openedFile);
            openedFile.View.Dispose();
        }

        /// <summary>
        ///   <para>
        /// Display 'Open File' dialog.
        /// </para>
        ///   <para>N.B. Supports selecting multiple files for opening</para>
        /// </summary>
        private void Open()
        {
            // Display model 'Open File' dialog ...
            var open = new OpenDialog("Open", "Open a file") { AllowsMultipleSelection = true };
            Application.Run(open);

            // Do we have files to open ? ...
            if (!open.Canceled)
            {
                // Yep, so for each selected file ...
                foreach (var path in open.FilePaths)
                {
                    // Dodgy file ? ...
                    if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    {
                        // Yep, so skip ...
                        return;
                    }

                    // Open file in new TextView, with filename as editorTab title ...
                    Open(File.ReadAllText(path), new FileInfo(path), Path.GetFileName(path));
                }
            }
        }

        /// <summary>Creates a new openedFile with initial text</summary>
        /// <param name="initialText">The initial text to display in the TextView</param>
        /// <param name="fileInfo">fileBeingEdited that was read or null if a new blank document</param>
        /// <param name="tabName">The text to display as the editorTab title.</param>
        private void Open(string initialText, FileInfo fileInfo, string tabName)
        {
            // Create TextView populated with supplied initial text ...
            var textView = new TextView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                Text = initialText
            };

            // Create new editorTab containing TextView and add to TabView ...
            var editorTab = new OpenedFile(tabName, fileInfo, textView);
            tabView.AddTab(editorTab, true);

            // when user makes changes rename openedFile to indicate unsaved
            textView.KeyUp += (k) =>
            {
                // if current text doesn't match saved text
                var areDiff = editorTab.UnsavedChanges;

                // Text changed ? ...
                if (areDiff)
                {
                    // Add the 'Text Modified' marker ...
                    if (!editorTab.Text.ToString().EndsWith('*'))
                    {
                        editorTab.Text = editorTab.Text.ToString() + '*';
                        tabView.SetNeedsDisplay();
                    }
                }
                // Nope ...
                else
                {
                    // So remove 'Text Modified' marker ...
                    if (editorTab.Text.ToString().EndsWith('*'))
                    {
                        editorTab.Text = editorTab.Text.ToString().TrimEnd('*');
                        tabView.SetNeedsDisplay();
                    }
                }
            };
        }

        /// <summary>Saves the pending changes associated with the TextView for the currently selected TabView editorTab.</summary>
        public void Save()
        {
            Save(tabView.SelectedTab);
        }

        /// <summary>Saves the pending changes associated with the TextView for the specified TabView editorTab.</summary>
        /// <param name="tabToSave">The editorTab to save.</param>
        public void Save(TabView.Tab tabToSave)
        {
            var editorTab = tabToSave as OpenedFile;

            // Bail if dodgy tab ...
            if (editorTab == null)
            {
                return;
            }

            // No pre-existing file associated, so prompt user for target file ...
            if (editorTab.fileBeingEdited == null)
            {
                SaveAs();
            }

            // Save pending changes and force refresh ...
            editorTab.Save();
            tabView.SetNeedsDisplay();
        }

        /// <summary>
        ///   <para>
        /// Prompts user for target file to save current editor tab contents to.
        /// </para>
        /// </summary>
        /// <returns>
        ///   <br />
        /// </returns>
        public bool SaveAs()
        {
            var editorTab = tabView.SelectedTab as OpenedFile;

            // Bail if dodgy tab ...
            if (editorTab == null)
            {
                return false;
            }

            // Display modal 'Save Dialog' ...
            var fd = new SaveDialog();
            Application.Run(fd);

            // Bail if dodgy target file selected ...
            if (string.IsNullOrWhiteSpace(fd.FilePath?.ToString()))
            {
                return false;
            }

            // Updated editor details and save ...
            editorTab.fileBeingEdited = new FileInfo(fd.FilePath.ToString());
            editorTab.Text = fd.FileName.ToString();
            editorTab.Save();

            return true;
        }

        /// <summary>Forces the application to exit.</summary>
        private void Quit()
        {
            Application.RequestStop();
        }

        #endregion 
    }
}
