#nullable enable
using System;
using System.Drawing;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace AnimeWatchTool
{
    public class UserConfigForm : Form
    {
        private readonly TextBox txtUsername;
        private readonly TextBox txtAccessToken;
        private readonly TextBox txtLocalFolder;
        private readonly Button btnBrowseFolder;
        private readonly Button btnSave;
        private readonly Button btnCancel;

        [JsonIgnore]
        public string Username = string.Empty;
        [JsonIgnore]
        public string AccessToken = string.Empty;
        [JsonIgnore]
        public string LocalFolder = string.Empty;

        public UserConfigForm(string currentUsername, string currentAccessToken, string currentLocalFolder)
        {
            Text = "设置";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(820, 280);
            MinimumSize = new Size(820, 280);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));

            var labelUsername = new Label
            {
                Text = "Bangumi 用户名：",
                Anchor = AnchorStyles.Right,
                AutoSize = true
            };
            txtUsername = new TextBox
            {
                Text = currentUsername,
                Dock = DockStyle.Fill
            };
            layout.Controls.Add(labelUsername, 0, 0);
            layout.Controls.Add(txtUsername, 1, 0);

            var labelToken = new Label
            {
                Text = "Bangumi Token：",
                Anchor = AnchorStyles.Right,
                AutoSize = true
            };
            txtAccessToken = new TextBox
            {
                Text = currentAccessToken,
                Dock = DockStyle.Fill
            };
            layout.Controls.Add(labelToken, 0, 1);
            layout.Controls.Add(txtAccessToken, 1, 1);

            var labelLocalFolder = new Label
            {
                Text = "本地 anime 目录：",
                Anchor = AnchorStyles.Right,
                AutoSize = true
            };
            txtLocalFolder = new TextBox
            {
                Text = currentLocalFolder,
                Dock = DockStyle.Fill
            };
            btnBrowseFolder = new Button
            {
                Text = "浏览",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                CausesValidation = false
            };
            btnBrowseFolder.Click += BtnBrowseFolder_Click;
            var folderPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0),
                AutoSize = true
            };
            folderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            folderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            folderPanel.Controls.Add(txtLocalFolder, 0, 0);
            folderPanel.Controls.Add(btnBrowseFolder, 1, 0);
            layout.Controls.Add(labelLocalFolder, 0, 2);
            layout.Controls.Add(folderPanel, 1, 2);

            Controls.Add(layout);

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            btnSave = new Button
            {
                Text = "保存",
                AutoSize = true,
                CausesValidation = false
            };
            btnSave.Click += BtnSave_Click;
            buttonPanel.Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text = "取消",
                AutoSize = true,
                DialogResult = DialogResult.Cancel,
                CausesValidation = false
            };
            buttonPanel.Controls.Add(btnCancel);

            Controls.Add(buttonPanel);
            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void BtnBrowseFolder_Click(object? sender, EventArgs e)
        {
            using var browser = new FolderBrowserDialog();
            if (Directory.Exists(txtLocalFolder.Text))
            {
                browser.SelectedPath = txtLocalFolder.Text;
            }

            if (browser.ShowDialog(this) == DialogResult.OK)
            {
                txtLocalFolder.Text = browser.SelectedPath;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var username = txtUsername.Text.Trim();
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show(this, "Bangumi 用户名不能为空。", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var localFolder = txtLocalFolder.Text.Trim();
            if (string.IsNullOrWhiteSpace(localFolder) || !Directory.Exists(localFolder))
            {
                MessageBox.Show(this, "本地 anime 目录不能为空且必须存在。", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Username = username;
            AccessToken = txtAccessToken.Text.Trim();
            LocalFolder = localFolder;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
