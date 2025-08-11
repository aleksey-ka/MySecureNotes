using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace MySecureNotes
{
    public partial class MainForm : Form
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new MainForm() );
        }

        private const int bufSize = 1024;
        private const int ivSize = 16;
        private const int maxRecordLength = ( bufSize - ( ivSize + 1 + Byte.MaxValue + sizeof( Int32 ) ) ) / sizeof( Int32 );
        private List<byte[]> buffers = new List<byte[]>();
        private bool isFiltering = false; // Track if filtering is currently active
        private List<TreeNode> originalTreeSnapshot = null; // Store the original tree for filtering
        private readonly byte[] salt = Convert.FromBase64String( 
            "ru5AEd259yYP+XKk/k1hbk3E4IqVOyaw4V61bmXbGGZMYTnm6pd5kY8mddGFX+dXcFhf6sBdR9m55Bb6cbCG4ZHOiEORrXtxsbIV/LQt9ZFCXG598JYV1sjqBiqx8U2ahG2FOhAgg92nUcBp7E9DzR4meceiqRetn2qD7JcfNpS2GOWt4ZQvGsJRcUkY6G7xhJ1XZORdcbnQXIJtNpgNaOTc+oAhzaoz7p9cV/0dRZMHX80hRKYsfvNPirdCtBHCM4Ri06rv9qbzy5AHDfTWB6UrwRAl1+BjU9guVNU2IdLfSA3ISR1L73baHiJV1Lm1SbtfU2msJtxiGruOVGlYXe8pFNLjvZrcXLZTnxm5g4h2A+pw/QkltF5Iz7eCbBSgaaNkbtrChxnApRAvPmw9QxAecEt3PG+nezQrb9ym/jFvNHmXzS1nMUG2RiJartFuEzDSDmDlwDu3PIkfc5Q993VrF7GVSy9R/PRudxhYUVwcq/+xknTu8sDS56RdVjTTJVOuIuy9oPNtkyq4hJr+ZQSgUWLJwALbus/ccpBrmpg/JMigDaPnS/fY9DEHS3nGhwvK48Jiu/ma6dSdlNx94BiuqSoUp0W0b3n3tGHNHhASeuXegR+gHZOyUyGew0iDL3chICw8u2fmomeQAWQpVplugBodxlfGyNhvku0NnLE=" );
        private byte[] key;

        private RandomNumberGenerator rnd = RandomNumberGenerator.Create();
        private Aes algorithm = Aes.Create();
        
        public MainForm()
        {
            InitializeComponent();

            passwordTextBox.MaxLength = salt.Length;
            updateTextBox.MaxLength = maxRecordLength;
            updateTextBox.ReadOnly = true;

            // Set up filter textbox with prompt text
            filterTextBox.Text = "Find notes...";
            filterTextBox.ForeColor = System.Drawing.Color.Gray;

            // Add event handlers for filter textbox
            filterTextBox.Enter += filterTextBox_Enter;
            filterTextBox.Leave += filterTextBox_Leave;
            filterTextBox.TextChanged += filterTextBox_TextChanged;

            // Set up clear filter button
            setupClearFilterButton();
        }

        private void MainForm_Shown( object sender, EventArgs e )
        {
            if( !File.Exists( Properties.Settings.Default.FilePath ) ) {
                MessageBox.Show(
                    @"To start using your secure notes storage, please enter a new password when prompted and press the 'Start' button. Use this password to access your secure notes later. Do not forget this password as there will be no way to restore it!",
                    "Welcome!" );
            }
        }

        private void passwordTextBox_TextChanged( object sender, EventArgs e )
        {
            startButton.Enabled = passwordTextBox.Text.Length >= 8;
        }

        private void startButton_Click( object sender, EventArgs e )
        {
            // Using RFC2898 hashing with 10000 iterations to derive key for encryption from the password
            Rfc2898DeriveBytes derivedBytes = new Rfc2898DeriveBytes( passwordTextBox.Text, salt, 10000 );
            key = derivedBytes.GetBytes( 32 );
            
            // Explicitly set the AES encryption mode
            algorithm.Mode = CipherMode.CBC;
            algorithm.Padding = PaddingMode.None;
                        
            // Reset the plain text password and trivia
            passwordTextBox.Text = "####################################";
            passwordTextBox.Enabled = false;
            startButton.Enabled = false;
            changePasswordButton.Visible = true;

            if( treeView.Nodes.Count > 0 ) {
                // Used when changing the password
                save();
            }

            load();
            
            groupBox.Enabled = true;
            AcceptButton = updateButton;
            treeView_AfterSelect( sender, null );
        }

        private void addNewButton_Click( object sender, EventArgs e )
        {
            // Disable modifications during filtering
            if( isFilterActive() ) {
                MessageBox.Show( "Please clear the filter before adding new notes.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information );
                return;
            }

            TreeNode node = treeView.Nodes.Add( ">>> Type a new note and press the 'Enter' key to commit or the 'Esc' key to cancel..." );
            treeView.SelectedNode = node;

            // Create the random buffer to embed the real data
            buffers.Add( newRandomBuffer() );

            updateButton.Enabled = true;
            addNewButton.Enabled = false;
            updateTextBox.ReadOnly = false;
            updateTextBox.Focus();
        }

        private void deleteButton_Click( object sender, EventArgs e )
        {
            // Disable modifications during filtering
            if( isFilterActive() ) {
                MessageBox.Show( "Please clear the filter before deleting notes.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information );
                return;
            }

            if( treeView.SelectedNode.Tag == null ) {
                // This is a folder node - forbid deletion
                MessageBox.Show( "Cannot delete folders. Please delete individual notes instead.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information );
                return;
            } else {
                // This is a note node - allow deletion (folders will auto-cleanup if empty)
                bool delete = MessageBox.Show( "Are you sure you want to delete the selected note?", this.Text, MessageBoxButtons.OKCancel ) == DialogResult.OK;
                if( delete ) {
                    // Remove the buffer for this note
                    if( int.TryParse( treeView.SelectedNode.Name, out int bufferIndex ) && bufferIndex < buffers.Count ) {
                        buffers.RemoveAt( bufferIndex );
                        // Update the Name property of all subsequent nodes
                        updateBufferIndices( treeView.Nodes, bufferIndex );
                    }

                    treeView.SelectedNode.Remove();

                    // Clean up empty folders after note deletion
                    cleanupEmptyFolders();

                    save();
                }
            }

            treeView_AfterSelect( sender, null );
        }

        private void editButton_Click( object sender, EventArgs e )
        {
            // Disable modifications during filtering
            if( isFilterActive() ) {
                MessageBox.Show( "Please clear the filter before editing notes.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information );
                return;
            }

            updateButton.Enabled = true;
            updateTextBox.ReadOnly = false;
            updateTextBox.Focus();
        }

        private void updateButton_Click( object sender, EventArgs e )
        {
            if( updateTextBox.Text.Trim().Length == 0 ) {
                cancelButton_Click( sender, null );
                return;
            }

            // Disable modifications during filtering
            if( isFilterActive() ) {
                MessageBox.Show( "Please clear the filter before modifying notes.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information );
                return;
            }

            // Check if this is a new note being created
            if( treeView.SelectedNode.Text.StartsWith( ">>>" ) ) {
                // This is a new note, so we need to recreate the tree structure
                string newValue = updateTextBox.Text;

                // Remove the temporary node
                treeView.SelectedNode.Remove();

                // Add the note with proper path structure
                addNodeWithPath( newValue, buffers.Count - 1 );

                // Select the newly created note
                treeView.SelectedNode = findNoteNode( newValue );
            } else {
                // This is an existing note being edited
                string oldValue = (string)treeView.SelectedNode.Tag;
                string newValue = updateTextBox.Text;

                // Check if the path (title) has changed
                string oldTitle = getTitle( oldValue );
                string newTitle = getTitle( newValue );

                if( oldTitle != newTitle ) {
                    // Path has changed - need to reorganize the tree
                    // Store the buffer index before removing the node
                    int bufferIndex = int.Parse( treeView.SelectedNode.Name );

                    // Remove the old node
                    treeView.SelectedNode.Remove();

                    // Add the note with new path structure
                    addNodeWithPath( newValue, bufferIndex );

                    // Select the newly positioned note
                    treeView.SelectedNode = findNoteNode( newValue );
                } else {
                    // Only content changed, no path change needed
                    treeView.SelectedNode.Tag = newValue;
                    treeView.SelectedNode.Text = newTitle;
                }
            }

            save();

            updateButton.Enabled = false;
            addNewButton.Enabled = true;
            updateTextBox.ReadOnly = true;
        }

        private void treeView_BeforeSelect( object sender, TreeViewCancelEventArgs e )
        {
            // Skip change checking during filtering since modifications are disabled
            if( isFilterActive() ) {
                return;
            }

            if( treeView.SelectedNode != null ) {
                if( treeView.SelectedNode.Tag == null ) {
                    // This is a folder node, no changes to discard
                    return;
                } else {
                    bool discard = ( e == null || updateTextBox.Text == (string) treeView.SelectedNode.Tag ||
                        MessageBox.Show( "Discard changes?", this.Text, MessageBoxButtons.OKCancel ) == DialogResult.OK );
                    if( discard ) {
                        return;
                    }
                }
                e.Cancel = true;
            }
        }

        private void treeView_AfterSelect( object sender, TreeViewEventArgs e )
        {
            // Always show note content, regardless of filtering state
            if( treeView.SelectedNode != null && treeView.SelectedNode.Tag != null ) {
                updateTextBox.Text = (string)treeView.SelectedNode.Tag;
            } else {
                updateTextBox.Text = "";
            }

            // Disable all modifications during filtering
            if( isFilterActive() ) {
                deleteButton.Enabled = false;
                editButton.Enabled = false;
                autoHideCheckBox.Enabled = false;
                updateButton.Enabled = false;
                addNewButton.Enabled = false;
                updateTextBox.ReadOnly = true;
                return;
            }

            // Enable delete button for any note node (not folders)
            bool canDelete = treeView.SelectedNode != null && treeView.SelectedNode.Tag != null;
            deleteButton.Enabled = canDelete;

            editButton.Enabled = treeView.SelectedNode != null && treeView.SelectedNode.Tag != null;
            autoHideCheckBox.Enabled = treeView.SelectedNode != null && treeView.SelectedNode.Tag != null;
            autoHideCheckBox.Checked = true;
            updateButton.Enabled = false;
            addNewButton.Enabled = true;
            updateTextBox.ReadOnly = true;
        }

        private void cancelButton_Click( object sender, EventArgs e )
        {
            treeView_BeforeSelect( sender, null );
            treeView.SelectedNode = null;
            treeView_AfterSelect( sender, null );
            cancelButton.Focus();
        }

        private void changePasswordButton_Click( object sender, EventArgs e )
        {
            passwordTextBox.Enabled = true;
            startButton.Enabled = true;
            groupBox.Enabled = false;

            AcceptButton = startButton;
        }

        private void MainForm_Activated( object sender, EventArgs e )
        {
            updateTextBox.Visible = true;
        }

        private void MainForm_Deactivate( object sender, EventArgs e )
        {
            if( autoHideCheckBox.Checked ) {
                updateTextBox.Visible = false;
            }
        }

        private void filterTextBox_Enter( object sender, EventArgs e )
        {
            // When user clicks on the filter textbox, clear the prompt text and change color
            if( filterTextBox.Text == "Find notes..." ) {
                filterTextBox.Text = "";
                filterTextBox.ForeColor = System.Drawing.Color.Black;
            }
        }

        private void filterTextBox_Leave( object sender, EventArgs e )
        {
            // When user leaves the filter textbox, restore prompt text if empty
            if( string.IsNullOrEmpty( filterTextBox.Text ) || filterTextBox.Text.Trim().Length == 0 ) {
                filterTextBox.Text = "Find notes...";
                filterTextBox.ForeColor = System.Drawing.Color.Gray;
                // Also restore the original tree when filter is cleared
                restoreOriginalTree();
            }
        }

        private void filterTextBox_TextChanged( object sender, EventArgs e )
        {
            // Don't filter if the text is the prompt text
            if( filterTextBox.Text == "Find notes..." ) {
                return;
            }

            // If the text is empty or just whitespace, restore the original tree
            if( string.IsNullOrEmpty( filterTextBox.Text ) || filterTextBox.Text.Trim().Length == 0 ) {
                restoreOriginalTree();
                return;
            }

            // Apply the filter to show only matching notes
            applyFilter( filterTextBox.Text );
        }
        


        private void applyFilter( string filterText )
        {
            if( string.IsNullOrEmpty( filterText ) || filterText.Trim().Length == 0 ) {
                // If filter is empty, restore original tree
                restoreOriginalTree();
                return;
            }

            // Set filtering flag
            isFiltering = true;

            // Store the current tree structure for restoration
            if( originalTreeSnapshot == null ) {
                storeCurrentTreeForFiltering();
            }

            // Apply filter by rebuilding the tree with only matching notes
            applySimpleFilter( filterText );

            // Update UI to show that modifications are disabled
            updateUIForFiltering();
        }

        private void storeCurrentTreeForFiltering()
        {
            // Store the current tree structure in a simple format
            originalTreeSnapshot = new List<TreeNode>();
            storeTreeRecursive( treeView.Nodes, originalTreeSnapshot );
        }

        private void storeTreeRecursive( TreeNodeCollection nodes, List<TreeNode> storage )
        {
            foreach( TreeNode node in nodes ) {
                TreeNode copyNode = new TreeNode( node.Text );
                copyNode.Tag = node.Tag;
                copyNode.Name = node.Name;
                storage.Add( copyNode );

                if( node.Nodes.Count > 0 ) {
                    storeTreeRecursive( node.Nodes, storage );
                }
            }
        }

        private void applySimpleFilter( string filterText )
        {
            // Clear current tree
            treeView.Nodes.Clear();

            // Add only matching notes, maintaining their folder structure
            foreach( TreeNode storedNode in originalTreeSnapshot ) {
                if( storedNode.Tag != null ) {
                    // This is a note node - check if it matches
                    string noteTitle = getTitle( (string)storedNode.Tag );
                    if( noteTitle.ToLower().IndexOf( filterText.ToLower() ) >= 0 ) {
                        // Add the note with its path structure
                        addFilteredNote( storedNode );
                    }
                }
            }
        }

        private void addFilteredNote( TreeNode storedNode )
        {
            string noteTitle = getTitle( (string)storedNode.Tag );
            string[] pathParts = noteTitle.Split( '\\' );

            TreeNodeCollection currentLevel = treeView.Nodes;
            List<TreeNode> createdFolders = new List<TreeNode>();

            // Create folder structure if needed
            for( int i = 0; i < pathParts.Length - 1; i++ ) {
                string folderName = pathParts[i];
                TreeNode folderNode = findOrCreateFolder( currentLevel, folderName );
                createdFolders.Add( folderNode );
                currentLevel = folderNode.Nodes;
            }

            // Add the note
            TreeNode noteNode = currentLevel.Add( pathParts[pathParts.Length - 1] );
            noteNode.Tag = storedNode.Tag;
            noteNode.Name = storedNode.Name;

            // Expand all created folders to show their children
            foreach( TreeNode folder in createdFolders ) {
                folder.Expand();
            }
        }

        private TreeNode findOrCreateFolder( TreeNodeCollection parent, string folderName )
        {
            // Look for existing folder
            foreach( TreeNode node in parent ) {
                if( node.Text == folderName && node.Tag == null ) {
                    return node;
                }
            }

            // Create new folder
            TreeNode folderNode = parent.Add( folderName );
            folderNode.Tag = null; // null Tag indicates folder
            return folderNode;
        }

        // Simplified filtering system - no complex tree rebuilding needed



        // Helper methods removed - simplified filtering system

        private void restoreOriginalTree()
        {
            // Clear filtering flag
            isFiltering = false;

            // Restore the original tree from our snapshot
            if( originalTreeSnapshot != null ) {
                treeView.Nodes.Clear();
                restoreTreeFromSnapshot();
                originalTreeSnapshot = null;
            }

            // Update UI to re-enable modifications
            updateUIForNormalMode();
        }

        private void restoreTreeFromSnapshot()
        {
            foreach( TreeNode storedNode in originalTreeSnapshot ) {
                if( storedNode.Tag != null ) {
                    // This is a note node - restore it with its path structure
                    addFilteredNote( storedNode );
                }
            }

            // Expand all root level folders to show their expand/collapse indicators
            foreach( TreeNode node in treeView.Nodes ) {
                if( node.Tag == null ) { // This is a folder node
                    node.Expand();
                }
            }
        }

        private void storeOriginalTree()
        {
            // No need to store original tree structure anymore since we're using visibility
            // This method is kept for compatibility but doesn't need to do anything
        }

        private bool isFilterActive()
        {
            return isFiltering;
        }

        private Button clearFilterButton;

        private void setupClearFilterButton()
        {
            // Create a small, circular clear filter button
            clearFilterButton = new Button();
            clearFilterButton.Size = new System.Drawing.Size(filterTextBox.Height - 2, filterTextBox.Height - 2);
            clearFilterButton.Text = "×";
            clearFilterButton.Font = new System.Drawing.Font("Arial", 6, System.Drawing.FontStyle.Regular);
            clearFilterButton.FlatStyle = FlatStyle.Flat;
            clearFilterButton.FlatAppearance.BorderSize = 0;
            clearFilterButton.BackColor = System.Drawing.Color.LightCoral;
            clearFilterButton.ForeColor = System.Drawing.Color.White;
            clearFilterButton.Cursor = Cursors.Hand;
            clearFilterButton.Visible = false;

            // Position it above the filter textbox
            clearFilterButton.Location = new System.Drawing.Point(filterTextBox.Location.X + filterTextBox.Width - (filterTextBox.Height + 2), filterTextBox.Location.Y + 1);

            // Add click event
            clearFilterButton.Click += clearFilterButton_Click;

            // Add to the form
            this.Controls.Add(clearFilterButton);
            clearFilterButton.BringToFront();
        }

        private void clearFilterButton_Click( object sender, EventArgs e )
        {
            // Clear the filter textbox
            filterTextBox.Text = "";
            filterTextBox.ForeColor = System.Drawing.Color.Gray;
            filterTextBox.Text = "Find notes...";

            // Restore the original tree
            restoreOriginalTree();

            // Focus back to the filter textbox
            filterTextBox.Focus();
        }

        private void updateUIForFiltering()
        {
            // Disable all modification controls during filtering
            deleteButton.Enabled = false;
            editButton.Enabled = false;
            autoHideCheckBox.Enabled = false;
            updateButton.Enabled = false;
            addNewButton.Enabled = false;
            updateTextBox.ReadOnly = true;
            updateTextBox.Text = "";

            // Update filter textbox to show that modifications are disabled
            filterTextBox.ForeColor = System.Drawing.Color.Blue;

            // Show the clear filter button
            clearFilterButton.Visible = true;
        }

        private void updateUIForNormalMode()
        {
            // Re-enable modification controls when filtering is cleared
            // Always call treeView_AfterSelect to update the UI state
            treeView_AfterSelect( this, null );

            // Hide the clear filter button
            clearFilterButton.Visible = false;

            // Restore normal filter textbox color
            if( filterTextBox.Text == "Find notes..." ) {
                filterTextBox.ForeColor = System.Drawing.Color.Gray;
            } else {
                filterTextBox.ForeColor = System.Drawing.Color.Black;
            }
        }

        // Simplified snapshot loading - no longer needed

        private void load()
        {
            treeView.Nodes.Clear();

            string filePath = Properties.Settings.Default.FilePath;
            if( File.Exists( filePath ) ) {
                string[] strings = File.ReadAllLines( filePath );
                buffers = new List<byte[]>();

                foreach( string s in strings ) {
                    if( s.Trim().Length > 0 ) {
                        byte[] buffer;
                        string value = decode( s, out buffer );
                        buffers.Add( buffer );
                        // Use addNodeWithPath to create hierarchical structure for ALL notes
                        addNodeWithPath( value, buffers.Count - 1 );
                    }
                }

                // Expand all root level folders to show their expand/collapse indicators
                foreach( TreeNode node in treeView.Nodes ) {
                    if( node.Tag == null ) { // This is a folder node
                        node.Expand();
                    }
                }
            }
        }

        private void save()
        {
            List<string> strings = new List<string>();
            saveRecursive( treeView.Nodes, strings );
            File.WriteAllLines( Properties.Settings.Default.FilePath, strings.ToArray() );
        }

        private void saveRecursive( TreeNodeCollection nodes, List<string> strings )
        {
            foreach( TreeNode node in nodes ) {
                if( node.Tag != null ) { // This is a note node
                    int bufferIndex = int.Parse( node.Name );
                    strings.Add( encode( (string) node.Tag, buffers[bufferIndex] ) );
                    strings.Add( "" );
                } else { // This is a folder node
                    saveRecursive( node.Nodes, strings );
                }
            }
        }

        private string getTitle( string value )
        {
            int pos = value.IndexOf( ' ' );
            return pos != -1 ? value.Substring( 0, pos ) : value;
        }

        private void addNodeWithPath( string value, int bufferIndex )
        {
            string title = getTitle( value );
            string[] pathParts = title.Split( '\\' );

            TreeNodeCollection currentLevel = treeView.Nodes;
            List<TreeNode> createdFolders = new List<TreeNode>();

            // Navigate through the path, creating folders as needed
            for( int i = 0; i < pathParts.Length - 1; i++ ) {
                string folderName = pathParts[i];
                TreeNode folderNode = null;

                // Look for existing folder
                foreach( TreeNode node in currentLevel ) {
                    if( node.Text == folderName && node.Tag == null ) {
                        folderNode = node;
                        break;
                    }
                }

                // Create folder if it doesn't exist
                if( folderNode == null ) {
                    folderNode = currentLevel.Add( folderName );
                    folderNode.Tag = null; // null Tag indicates a folder node
                    createdFolders.Add( folderNode );
                }

                currentLevel = folderNode.Nodes;
            }

            // Add the actual note at the final level
            string noteName = pathParts[pathParts.Length - 1];
            TreeNode noteNode = currentLevel.Add( noteName );
            noteNode.Tag = value;
            noteNode.Name = bufferIndex.ToString();

            // Ensure all created folders are expanded to show their children
            foreach( TreeNode folder in createdFolders ) {
                folder.Expand();
            }
        }

        private TreeNode findNoteNode( string value )
        {
            return findNoteNodeRecursive( treeView.Nodes, value );
        }

        private TreeNode findNoteNodeRecursive( TreeNodeCollection nodes, string value )
        {
            foreach( TreeNode node in nodes ) {
                if( node.Tag != null && node.Tag.ToString() == value ) {
                    return node;
                }

                TreeNode found = findNoteNodeRecursive( node.Nodes, value );
                if( found != null ) {
                    return found;
                }
            }
            return null;
        }

        private void updateBufferIndices( TreeNodeCollection nodes, int removedIndex )
        {
            foreach( TreeNode node in nodes ) {
                if( node.Tag != null ) { // This is a note node
                    if( int.TryParse( node.Name, out int bufferIndex ) && bufferIndex > removedIndex ) {
                        node.Name = ( bufferIndex - 1 ).ToString();
                    }
                } else { // This is a folder node
                    updateBufferIndices( node.Nodes, removedIndex );
                }
            }
        }

        private void cleanupEmptyFolders()
        {
            cleanupEmptyFoldersRecursive( treeView.Nodes );
        }

        private void cleanupEmptyFoldersRecursive( TreeNodeCollection nodes )
        {
            // Work backwards to avoid index issues when removing nodes
            for( int i = nodes.Count - 1; i >= 0; i-- ) {
                TreeNode node = nodes[i];

                if( node.Tag == null ) { // This is a folder node
                    // Recursively clean up subfolders first
                    cleanupEmptyFoldersRecursive( node.Nodes );

                    // If the folder is now empty, remove it
                    if( node.Nodes.Count == 0 ) {
                        nodes.RemoveAt( i );
                    }
                }
            }
        }

        private byte[] crypt( byte[] buf, bool encrypt )        
        {
            using( var m = new MemoryStream() ) {
                // The prefix of the random buffer is used as IV
                byte[] iv = new byte[ivSize];
                Array.Copy( buf, iv, ivSize );
                m.Write( buf, 0, ivSize );

                // The rest is encrypted
                ICryptoTransform cryptor = encrypt ? algorithm.CreateEncryptor( key, iv ) : algorithm.CreateDecryptor( key, iv );
                using( Stream c = new CryptoStream( m, cryptor, CryptoStreamMode.Write ) ) {
                    c.Write( buf, ivSize, buf.Length - ivSize );
                }
                return m.ToArray();
            }
        }

        private string encode( string s, byte[] buf )
        {
            // Random offset
            int offset = ivSize + buf[ivSize] + 1;
            var bytes = Encoding.UTF8.GetBytes( s );
            writeInt( buf, offset, bytes.Length );
            offset += 4;
            // Homebrew hash
            for( int i = 0; i < bytes.Length; i++ ) {
                buf[offset + i] = (byte)( bytes[i] ^ salt[i % salt.Length] ^ salt[salt[offset % salt.Length] % salt.Length]);
            }
            // The true encryption
            return Convert.ToBase64String( crypt( buf, true ) );
        }

        private string decode( string s, out byte[] buf )
        {
            // Decryption
            buf = crypt( Convert.FromBase64String( s ), false );
            // Random offset
            int offset = ivSize + buf[ivSize] + 1;
            int length = Math.Min( buf.Length - offset - 4, Math.Abs( readInt( buf, offset ) ) );
            offset += 4;
            // Homebrew hash
            var bytes = new byte[length];
            for( int i = 0; i < bytes.Length; i++ ) {
                bytes[i] = (byte) ( buf[offset + i] ^ salt[i % salt.Length] ^ salt[salt[offset % salt.Length] % salt.Length] );
            }
            return Encoding.UTF8.GetString( bytes );
        }

        private byte[] newRandomBuffer()
        {
            byte[] buf = new byte[bufSize];
            rnd.GetBytes( buf );
            return buf;
        }

        private static int readInt( byte[] buf, int pos )
        {
            return buf[pos + 0] |
                ( buf[pos + 1] << 8 ) |
                ( buf[pos + 2] << 16 ) |
                ( buf[pos + 3] << 24 );
        }

        private static void writeInt( byte[] buf, int pos, int value )
        {
            buf[pos + 0] = (byte) ( value & 0x000000FF );
            buf[pos + 1] = (byte) ( ( value & 0x0000FF00 ) >> 8 );
            buf[pos + 2] = (byte) ( ( value & 0x00FF0000 ) >> 16 );
            buf[pos + 3] = (byte) ( ( value & 0xFF000000 ) >> 24 );
        }

        private void filterTextBox_KeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if( e.KeyCode == Keys.Escape) {
                clearFilterButton_Click( sender, e );
            }
        }
    }
}
