namespace tuiedit
{
    using Terminal.Gui;

    public partial class TabEdit
    {
        /// <summary>Wrapper class for file being edited. Derives from TabView.Tab</summary>
        private class OpenedFile : TabView.Tab
        {
            #region Fields and properties

            /// <summary>The last recorded cursor position</summary>
            public Point lastCursorPosition;

            /// <summary>Gets or sets the fileToEdit being edited.</summary>
            /// <value>The fileToEdit being edited.</value>
            public FileInfo fileBeingEdited { get; set; }

            /// <summary>
            /// The text of the tab the last time it was saved
            /// </summary>
            /// <value></value>
            public string? SavedText { get; set; }

            /// <summary>Gets a value indicating whether there are unsaved changes.</summary>
            /// <value>
            ///   <c>true</c> if unsaved changes otherwise, <c>false</c>.</value>
            public bool UnsavedChanges => !string.Equals(SavedText, View.Text.ToString());

            #endregion 

            /// <summary>Initializes a new instance of the <see cref="OpenedFile" /> class.</summary>
            /// <param tabTitle="tabTitle">The tab title.</param>
            /// <param tabTitle="fileToEdit">The file to edit.</param>
            /// <param tabTitle="control">The associated TextView control.</param>
            public OpenedFile(string tabTitle, FileInfo fileToEdit, TextView control)
                : base(tabTitle, control)
            {
                fileBeingEdited = fileToEdit;
                SavedText = control.Text.ToString();

                control.DesiredCursorVisibility = CursorVisibility.Underline;

                // Ensure cursor position updated in status bar ...
                control.UnwrappedCursorPosition += (e) =>
                {
                    lastCursorPosition.X = e.X;
                    lastCursorPosition.Y = e.Y;
                    Application.Top.StatusBar.Items[0].Title = $"Ln {e.Y + 1}, Col {e.X + 1}";
                    Application.Top.StatusBar.SetNeedsDisplay();
                };
            }
             
            /// <summary>Saves the fileToEdit being edited in the TextView back to fileToEdit system</summary>
            internal void Save()
            {
                // Grab a copy of the current editor contents ...
                var currentText = View.Text.ToString();

                // Save the current editor contents and take a copy ...
                File.WriteAllText(fileBeingEdited.FullName, currentText);
                SavedText = currentText;

                // Remove the 'fileToEdit modified' marker ...
                Text = Text.ToString().TrimEnd('*');
            }
        }
    }
}
