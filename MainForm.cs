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
        private const int maxRecordLength = ( bufSize - ( ivSize + 1 + Byte.MaxValue + sizeof( Int32 ) ) ) / sizeof( char );
        private const string salt = "9F8a8,065#a3-8Rb!c0%вcу20e3f1b3^f42+59bc9Р8a8,0$5#a3-8%b!c;%bc'20d3f1(3^f]2{59Р}";
        private List<byte[]> buffers;
        private int[] magicHash;
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
            string password = passwordTextBox.Text;

            Rfc2898DeriveBytes derivedBytes = new Rfc2898DeriveBytes( password, 
                System.Text.Encoding.ASCII.GetBytes( salt ), 10000 );
            key = derivedBytes.GetBytes( 32 );

            int[] passwordHash = new int[password.Length];
            for( int i = 0; i < password.Length; i++ ) {
                for( int j = 0; j < password.Length; j++ ) {
                    if( i != j ) {
                        passwordHash[i] ^= (int) password[j] ^ j;
                    }
                }
            }

            magicHash = new int[salt.Length];
            for( int i = 0; i < salt.Length; i++ ) {
                magicHash[i] = (int) salt[i] ^ passwordHash[i % passwordHash.Length];
                magicHash[i] ^= (int) salt[magicHash[i] % salt.Length];
            }
                        
            passwordTextBox.Text = "####################################";
            passwordTextBox.Enabled = false;
            startButton.Enabled = false;
            changePasswordButton.Visible = true;

            if( treeView.Nodes.Count > 0 ) {
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
                byte[] iv = new byte[ivSize];
                Array.Copy( buf, iv, ivSize );
                m.Write( buf, 0, ivSize );

                ICryptoTransform cryptor = encrypt ? algorithm.CreateEncryptor( key, iv ) : algorithm.CreateDecryptor( key, iv );
                using( Stream c = new CryptoStream( m, cryptor, CryptoStreamMode.Write ) ) {
                    c.Write( buf, ivSize, buf.Length - ivSize );
                }
                return m.ToArray();
            }
        }

        private string encode( string s, byte[] buf )
        {
            int offset = ivSize + buf[ivSize] + 1;
            writeInt( buf, offset, s.Length );
            offset += 4;
            for( int i = 0; i < s.Length; i++ ) {
                int c = (int)s[i] ^ magicHash[i] ^ magicHash[ magicHash[offset % magicHash.Length] % magicHash.Length ];
                writeInt( buf, offset + 4 * i, c );
            }
            return Convert.ToBase64String( crypt( buf, true ) );
        }

        private string decode( string s, out byte[] buf )
        {
            buf = crypt( Convert.FromBase64String( s ), false );
            int offset = ivSize + buf[ivSize] + 1;
            int length = Math.Max( 0, Math.Min( maxRecordLength, readInt( buf, offset ) ) );
            offset += 4;
            System.Text.StringBuilder result = new System.Text.StringBuilder( length );
            for( int i = 0; i < length; i++ ) {
                int c = readInt( buf, offset + 4 * i );
                result.Append( (char) ( c ^ (int) magicHash[i] ^ magicHash[magicHash[offset % magicHash.Length] % magicHash.Length] ) );
            }
            return result.ToString();
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
