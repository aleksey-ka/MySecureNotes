namespace MySecureNotes
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing && ( components != null ) ) {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( MainForm ) );
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.startButton = new System.Windows.Forms.Button();
            this.treeView = new System.Windows.Forms.TreeView();
            this.imageList = new System.Windows.Forms.ImageList( this.components );
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.autoHideCheckBox = new System.Windows.Forms.CheckBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.changePasswordButton = new System.Windows.Forms.Button();
            this.editButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.addNewButton = new System.Windows.Forms.Button();
            this.updateButton = new System.Windows.Forms.Button();
            this.updateTextBox = new System.Windows.Forms.TextBox();
            this.autoHidePromptLabel = new System.Windows.Forms.Label();
            this.groupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.passwordTextBox.Location = new System.Drawing.Point( 16, 15 );
            this.passwordTextBox.Margin = new System.Windows.Forms.Padding( 4 );
            this.passwordTextBox.MaxLength = 40;
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size( 812, 22 );
            this.passwordTextBox.TabIndex = 0;
            this.passwordTextBox.UseSystemPasswordChar = true;
            this.passwordTextBox.TextChanged += new System.EventHandler( this.passwordTextBox_TextChanged );
            // 
            // startButton
            // 
            this.startButton.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.startButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.startButton.Enabled = false;
            this.startButton.Location = new System.Drawing.Point( 838, 12 );
            this.startButton.Margin = new System.Windows.Forms.Padding( 4 );
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size( 121, 28 );
            this.startButton.TabIndex = 1;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler( this.startButton_Click );
            // 
            // treeView
            // 
            this.treeView.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                        | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.treeView.HideSelection = false;
            this.treeView.ImageIndex = 0;
            this.treeView.ImageList = this.imageList;
            this.treeView.Location = new System.Drawing.Point( 8, 17 );
            this.treeView.Margin = new System.Windows.Forms.Padding( 4 );
            this.treeView.Name = "treeView";
            this.treeView.SelectedImageIndex = 0;
            this.treeView.ShowRootLines = false;
            this.treeView.Size = new System.Drawing.Size( 926, 390 );
            this.treeView.TabIndex = 2;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler( this.treeView_AfterSelect );
            this.treeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler( this.treeView_BeforeSelect );
            // 
            // imageList
            // 
            this.imageList.ImageStream = ( (System.Windows.Forms.ImageListStreamer) ( resources.GetObject( "imageList.ImageStream" ) ) );
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName( 0, "bullet_green.ico" );
            // 
            // groupBox
            // 
            this.groupBox.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                        | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.groupBox.Controls.Add( this.autoHideCheckBox );
            this.groupBox.Controls.Add( this.treeView );
            this.groupBox.Controls.Add( this.cancelButton );
            this.groupBox.Controls.Add( this.changePasswordButton );
            this.groupBox.Controls.Add( this.editButton );
            this.groupBox.Controls.Add( this.deleteButton );
            this.groupBox.Controls.Add( this.addNewButton );
            this.groupBox.Controls.Add( this.updateButton );
            this.groupBox.Controls.Add( this.updateTextBox );
            this.groupBox.Controls.Add( this.autoHidePromptLabel );
            this.groupBox.Enabled = false;
            this.groupBox.Location = new System.Drawing.Point( 16, 47 );
            this.groupBox.Margin = new System.Windows.Forms.Padding( 4 );
            this.groupBox.Name = "groupBox";
            this.groupBox.Padding = new System.Windows.Forms.Padding( 4 );
            this.groupBox.Size = new System.Drawing.Size( 943, 516 );
            this.groupBox.TabIndex = 3;
            this.groupBox.TabStop = false;
            // 
            // autoHideCheckBox
            // 
            this.autoHideCheckBox.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.autoHideCheckBox.AutoSize = true;
            this.autoHideCheckBox.Checked = true;
            this.autoHideCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoHideCheckBox.Enabled = false;
            this.autoHideCheckBox.Location = new System.Drawing.Point( 9, 448 );
            this.autoHideCheckBox.Margin = new System.Windows.Forms.Padding( 4 );
            this.autoHideCheckBox.Name = "autoHideCheckBox";
            this.autoHideCheckBox.Size = new System.Drawing.Size( 180, 21 );
            this.autoHideCheckBox.TabIndex = 10;
            this.autoHideCheckBox.Text = "Hide note when inactive";
            this.autoHideCheckBox.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point( 39, 48 );
            this.cancelButton.Margin = new System.Windows.Forms.Padding( 4 );
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size( 113, 28 );
            this.cancelButton.TabIndex = 9;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler( this.cancelButton_Click );
            // 
            // changePasswordButton
            // 
            this.changePasswordButton.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.changePasswordButton.Location = new System.Drawing.Point( 748, 480 );
            this.changePasswordButton.Margin = new System.Windows.Forms.Padding( 4 );
            this.changePasswordButton.Name = "changePasswordButton";
            this.changePasswordButton.Size = new System.Drawing.Size( 187, 28 );
            this.changePasswordButton.TabIndex = 8;
            this.changePasswordButton.Text = "Change Password";
            this.changePasswordButton.UseVisualStyleBackColor = true;
            this.changePasswordButton.Visible = false;
            this.changePasswordButton.Click += new System.EventHandler( this.changePasswordButton_Click );
            // 
            // editButton
            // 
            this.editButton.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.editButton.Location = new System.Drawing.Point( 116, 480 );
            this.editButton.Margin = new System.Windows.Forms.Padding( 4 );
            this.editButton.Name = "editButton";
            this.editButton.Size = new System.Drawing.Size( 100, 28 );
            this.editButton.TabIndex = 7;
            this.editButton.Text = "Edit";
            this.editButton.UseVisualStyleBackColor = true;
            this.editButton.Click += new System.EventHandler( this.editButton_Click );
            // 
            // deleteButton
            // 
            this.deleteButton.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.deleteButton.Location = new System.Drawing.Point( 224, 480 );
            this.deleteButton.Margin = new System.Windows.Forms.Padding( 4 );
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size( 100, 28 );
            this.deleteButton.TabIndex = 6;
            this.deleteButton.Text = "Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler( this.deleteButton_Click );
            // 
            // addNewButton
            // 
            this.addNewButton.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.addNewButton.Location = new System.Drawing.Point( 8, 480 );
            this.addNewButton.Margin = new System.Windows.Forms.Padding( 4 );
            this.addNewButton.Name = "addNewButton";
            this.addNewButton.Size = new System.Drawing.Size( 100, 28 );
            this.addNewButton.TabIndex = 5;
            this.addNewButton.Text = "Add New";
            this.addNewButton.UseVisualStyleBackColor = true;
            this.addNewButton.Click += new System.EventHandler( this.addNewButton_Click );
            // 
            // updateButton
            // 
            this.updateButton.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.updateButton.Enabled = false;
            this.updateButton.Location = new System.Drawing.Point( 822, 412 );
            this.updateButton.Margin = new System.Windows.Forms.Padding( 4 );
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size( 113, 30 );
            this.updateButton.TabIndex = 4;
            this.updateButton.Text = "Commit";
            this.updateButton.UseVisualStyleBackColor = true;
            this.updateButton.Click += new System.EventHandler( this.updateButton_Click );
            // 
            // updateTextBox
            // 
            this.updateTextBox.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.updateTextBox.Location = new System.Drawing.Point( 8, 416 );
            this.updateTextBox.Margin = new System.Windows.Forms.Padding( 4 );
            this.updateTextBox.MaxLength = 40;
            this.updateTextBox.Name = "updateTextBox";
            this.updateTextBox.Size = new System.Drawing.Size( 804, 22 );
            this.updateTextBox.TabIndex = 3;
            // 
            // autoHidePromptLabel
            // 
            this.autoHidePromptLabel.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.autoHidePromptLabel.AutoSize = true;
            this.autoHidePromptLabel.ForeColor = System.Drawing.Color.FromArgb( ( (int) ( ( (byte) ( 255 ) ) ) ), ( (int) ( ( (byte) ( 128 ) ) ) ), ( (int) ( ( (byte) ( 0 ) ) ) ) );
            this.autoHidePromptLabel.Location = new System.Drawing.Point( 5, 420 );
            this.autoHidePromptLabel.Margin = new System.Windows.Forms.Padding( 0 );
            this.autoHidePromptLabel.Name = "autoHidePromptLabel";
            this.autoHidePromptLabel.Size = new System.Drawing.Size( 334, 17 );
            this.autoHidePromptLabel.TabIndex = 11;
            this.autoHidePromptLabel.Text = "Uncheck the checkbox below to keep the text visible";
            // 
            // MainForm
            // 
            this.AcceptButton = this.startButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF( 8F, 16F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size( 975, 577 );
            this.Controls.Add( this.groupBox );
            this.Controls.Add( this.startButton );
            this.Controls.Add( this.passwordTextBox );
            this.Icon = ( (System.Drawing.Icon) ( resources.GetObject( "$this.Icon" ) ) );
            this.Margin = new System.Windows.Forms.Padding( 4 );
            this.MinimumSize = new System.Drawing.Size( 571, 384 );
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "My Secure Notes";
            this.Deactivate += new System.EventHandler( this.MainForm_Deactivate );
            this.Shown += new System.EventHandler( this.MainForm_Shown );
            this.Activated += new System.EventHandler( this.MainForm_Activated );
            this.groupBox.ResumeLayout( false );
            this.groupBox.PerformLayout();
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.TextBox updateTextBox;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button addNewButton;
        private System.Windows.Forms.Button updateButton;
        private System.Windows.Forms.Button editButton;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Button changePasswordButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.CheckBox autoHideCheckBox;
        private System.Windows.Forms.Label autoHidePromptLabel;
    }
}

