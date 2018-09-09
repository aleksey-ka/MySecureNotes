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
        }

        private void MainForm_Shown( object sender, EventArgs e )
        {
            if( !File.Exists( Properties.Settings.Default.FilePath ) ) {
                MessageBox.Show(
                    @"To start using your secure notes storage, please enter a new password when prompted and press 'Start' button. Use this password to access your secure notes later. Do not forget this password as there will be no way to restore it!",
                    "Welcome!" );
            }
        }

        private void passwordTextBox_TextChanged( object sender, EventArgs e )
        {
            startButton.Enabled = passwordTextBox.Text.Length >= 8;
        }

        private void startButton_Click( object sender, EventArgs e )
        {
            // Using RFC2898 hashing with 10000 iteration to derive key for encryption from the password
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
                // Used when changing password
                save();
            }

            load();
            
            groupBox.Enabled = true;
            AcceptButton = updateButton;
            treeView_AfterSelect( sender, null );
        }

        private void addNewButton_Click( object sender, EventArgs e )
        {
            TreeNode node = treeView.Nodes.Add( ">>> Type a new note and press 'Enter' to commit or 'Esc' to cancel..." );
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
            bool delete = ( treeView.SelectedNode.Tag == null ||
                MessageBox.Show( "Are you sure you want to delete the selected note?", this.Text,  MessageBoxButtons.OKCancel ) == DialogResult.OK );
            if( delete ) {
                int index = treeView.Nodes.IndexOf( treeView.SelectedNode );
                treeView.Nodes.RemoveAt( index );
                buffers.RemoveAt( index );
                save();

                treeView_AfterSelect( sender, null );
            }
        }

        private void editButton_Click( object sender, EventArgs e )
        {
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

            treeView.SelectedNode.Tag = updateTextBox.Text;
            treeView.SelectedNode.Text = getTitle( updateTextBox.Text );
            save();

            updateButton.Enabled = false;
            addNewButton.Enabled = true;
            updateTextBox.ReadOnly = true;
        }

        private void treeView_BeforeSelect( object sender, TreeViewCancelEventArgs e )
        {
            if( treeView.SelectedNode != null ) {
                if( treeView.SelectedNode.Tag == null ) {
                    bool discard = ( e == null || updateTextBox.Text.Length == 0 ||
                        MessageBox.Show( "Discard changes?", this.Text, MessageBoxButtons.OKCancel ) == DialogResult.OK );
                    if( discard ) {
                        treeView.Nodes.Remove( treeView.SelectedNode );
                        return;
                    }
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
            deleteButton.Enabled = treeView.SelectedNode != null;
            editButton.Enabled = treeView.SelectedNode != null;
            autoHideCheckBox.Enabled = treeView.SelectedNode != null;
            autoHideCheckBox.Checked = true;
            updateButton.Enabled = false;
            addNewButton.Enabled = true;
            updateTextBox.ReadOnly = true;
            
            if( treeView.SelectedNode != null ) {
                updateTextBox.Text = (string)treeView.SelectedNode.Tag;
            } else {
                updateTextBox.Text = "";
            }
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

        private void load()
        {
            treeView.Nodes.Clear();

            string filePath = Properties.Settings.Default.FilePath;
            if( File.Exists( filePath ) ) {
                string[] strings = File.ReadAllLines( filePath );
                buffers = new List<byte[]>( strings.Length );
                foreach( string s in strings ) {
                    if( s.Trim().Length > 0 ) {
                        byte[] buffer;
                        string value = decode( s, out buffer );
                        TreeNode newNode = treeView.Nodes.Add( getTitle( value ) );
                        newNode.Tag = value;
                        buffers.Add( buffer );
                    }
                }
            }
        }

        private void save()
        {
            List<string> strings = new List<string>();
            int index = 0;
            foreach( TreeNode node in treeView.Nodes ) {
                strings.Add( encode( (string) node.Tag, buffers[index] ) );
                strings.Add( "" );
                index++;
            }
            File.WriteAllLines( Properties.Settings.Default.FilePath, strings.ToArray() );
        }

        private string getTitle( string value )
        {
            int pos = value.IndexOf( ' ' );
            return pos != -1 ? value.Substring( 0, pos ) : value;
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
    }
}
