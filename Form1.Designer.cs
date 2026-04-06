#nullable disable

namespace AnimeWatchTool;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Label labelBangumiUsername;
    private System.Windows.Forms.TextBox txtBangumiUsername;
    private System.Windows.Forms.Label labelAccessToken;
    private System.Windows.Forms.TextBox txtAccessToken;
    private System.Windows.Forms.Button btnSaveConfig;
    private System.Windows.Forms.Button btnChangeUser;
    private System.Windows.Forms.Button btnConfigureUser;
    private System.Windows.Forms.Button btnSyncBangumi;
    private System.Windows.Forms.SplitContainer splitContainerMain;
    private System.Windows.Forms.Panel panelSeriesHeader;
    private System.Windows.Forms.TableLayoutPanel topBarLayout;
    private System.Windows.Forms.Label labelSeriesSearch;
    private System.Windows.Forms.TextBox txtSeriesSearch;
    private System.Windows.Forms.Label labelSeriesFilter;
    private System.Windows.Forms.ComboBox cmbSeriesFilter;
    private System.Windows.Forms.DataGridView dgvSeries;
    private System.Windows.Forms.DataGridView dgvEpisodes;
    private System.Windows.Forms.StatusStrip statusStrip;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    private System.Windows.Forms.DataGridViewTextBoxColumn colSeriesTitle;
    private System.Windows.Forms.DataGridViewTextBoxColumn colSeriesLocalCount;
    private System.Windows.Forms.DataGridViewTextBoxColumn colSeriesLocalFolder;
    private System.Windows.Forms.DataGridViewTextBoxColumn colEpisodeNumber;
    private System.Windows.Forms.DataGridViewTextBoxColumn colEpisodeTitle;
    private System.Windows.Forms.DataGridViewButtonColumn colEpisodePlay;
    private System.Windows.Forms.DataGridViewButtonColumn colEpisodeMarkWatched;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this.labelBangumiUsername = new System.Windows.Forms.Label();
        this.txtBangumiUsername = new System.Windows.Forms.TextBox();
        this.labelAccessToken = new System.Windows.Forms.Label();
        this.txtAccessToken = new System.Windows.Forms.TextBox();
        this.btnSaveConfig = new System.Windows.Forms.Button();
        this.btnChangeUser = new System.Windows.Forms.Button();
        this.btnConfigureUser = new System.Windows.Forms.Button();
        this.btnSyncBangumi = new System.Windows.Forms.Button();
        this.splitContainerMain = new System.Windows.Forms.SplitContainer();
        this.panelSeriesHeader = new System.Windows.Forms.Panel();
        this.topBarLayout = new System.Windows.Forms.TableLayoutPanel();
        this.labelSeriesSearch = new System.Windows.Forms.Label();
        this.txtSeriesSearch = new System.Windows.Forms.TextBox();
        this.labelSeriesFilter = new System.Windows.Forms.Label();
        this.cmbSeriesFilter = new System.Windows.Forms.ComboBox();
        this.dgvSeries = new System.Windows.Forms.DataGridView();
        this.colSeriesTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colSeriesLocalCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colSeriesLocalFolder = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.dgvEpisodes = new System.Windows.Forms.DataGridView();
        this.colEpisodeNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colEpisodeTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colEpisodePlay = new System.Windows.Forms.DataGridViewButtonColumn();
        this.colEpisodeMarkWatched = new System.Windows.Forms.DataGridViewButtonColumn();
        this.statusStrip = new System.Windows.Forms.StatusStrip();
        this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
        this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
        ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
        this.splitContainerMain.Panel1.SuspendLayout();
        this.splitContainerMain.Panel2.SuspendLayout();
        this.splitContainerMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvSeries)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.dgvEpisodes)).BeginInit();
        this.statusStrip.SuspendLayout();
        this.SuspendLayout();
        // 
        // labelBangumiUsername
        // 
        this.labelBangumiUsername.AutoSize = true;
        this.labelBangumiUsername.Location = new System.Drawing.Point(12, 14);
        this.labelBangumiUsername.Name = "labelBangumiUsername";
        this.labelBangumiUsername.Size = new System.Drawing.Size(112, 20);
        this.labelBangumiUsername.TabIndex = 0;
        this.labelBangumiUsername.Text = "Bangumi 用户名";
        this.labelBangumiUsername.Visible = false;
        // 
        // txtBangumiUsername
        // 
        this.txtBangumiUsername.Location = new System.Drawing.Point(136, 11);
        this.txtBangumiUsername.Name = "txtBangumiUsername";
        this.txtBangumiUsername.Size = new System.Drawing.Size(220, 27);
        this.txtBangumiUsername.TabIndex = 1;
        this.txtBangumiUsername.Visible = false;
        // 
        // labelAccessToken
        // 
        this.labelAccessToken.AutoSize = true;
        this.labelAccessToken.Location = new System.Drawing.Point(12, 52);
        this.labelAccessToken.Name = "labelAccessToken";
        this.labelAccessToken.Size = new System.Drawing.Size(150, 20);
        this.labelAccessToken.TabIndex = 2;
        this.labelAccessToken.Text = "Bangumi Access Token";
        this.labelAccessToken.Visible = false;
        // 
        // txtAccessToken
        // 
        this.txtAccessToken.Location = new System.Drawing.Point(136, 49);
        this.txtAccessToken.Name = "txtAccessToken";
        this.txtAccessToken.Size = new System.Drawing.Size(555, 27);
        this.txtAccessToken.TabIndex = 3;
        this.txtAccessToken.Visible = false;
        // 
        // btnSaveConfig
        // 
        this.btnSaveConfig.Location = new System.Drawing.Point(697, 47);
        this.btnSaveConfig.Name = "btnSaveConfig";
        this.btnSaveConfig.Size = new System.Drawing.Size(88, 29);
        this.btnSaveConfig.TabIndex = 4;
        this.btnSaveConfig.Text = "保存设置";
        this.btnSaveConfig.UseVisualStyleBackColor = true;
        this.btnSaveConfig.Visible = false;
        this.btnSaveConfig.Click += new System.EventHandler(this.btnSaveConfig_Click);
        // 
        // btnChangeUser
        // 
        this.btnChangeUser.Location = new System.Drawing.Point(791, 47);
        this.btnChangeUser.Name = "btnChangeUser";
        this.btnChangeUser.Size = new System.Drawing.Size(88, 29);
        this.btnChangeUser.TabIndex = 5;
        this.btnChangeUser.Text = "切换账号";
        this.btnChangeUser.UseVisualStyleBackColor = true;
        this.btnChangeUser.Visible = false;
        this.btnChangeUser.Click += new System.EventHandler(this.btnChangeUser_Click);
        // 
        // btnConfigureUser
        // 
        this.btnConfigureUser.Location = new System.Drawing.Point(476, 15);
        this.btnConfigureUser.Name = "btnConfigureUser";
        this.btnConfigureUser.Size = new System.Drawing.Size(108, 29);
        this.btnConfigureUser.TabIndex = 5;
        this.btnConfigureUser.Text = "设置";
        this.btnConfigureUser.UseVisualStyleBackColor = true;
        // 
        // btnSyncBangumi
        // 
        this.btnSyncBangumi.Location = new System.Drawing.Point(362, 15);
        this.btnSyncBangumi.Name = "btnSyncBangumi";
        this.btnSyncBangumi.Size = new System.Drawing.Size(108, 29);
        this.btnSyncBangumi.TabIndex = 6;
        this.btnSyncBangumi.Text = "同步 Bangumi";
        this.btnSyncBangumi.UseVisualStyleBackColor = true;
        // 
        // splitContainerMain
        // 
        this.splitContainerMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.splitContainerMain.Location = new System.Drawing.Point(12, 60);
        this.splitContainerMain.Name = "splitContainerMain";
        this.splitContainerMain.Panel1.Controls.Add(this.dgvSeries);
        this.splitContainerMain.Panel2.Controls.Add(this.dgvEpisodes);
        this.splitContainerMain.Size = new System.Drawing.Size(867, 534);
        this.splitContainerMain.SplitterDistance = 360;
        this.splitContainerMain.TabIndex = 11;
        // 
        // panelSeriesHeader
        // 
        this.panelSeriesHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.panelSeriesHeader.Controls.Add(this.topBarLayout);
        this.panelSeriesHeader.Location = new System.Drawing.Point(12, 12);
        this.panelSeriesHeader.Name = "panelSeriesHeader";
        this.panelSeriesHeader.Size = new System.Drawing.Size(867, 40);
        this.panelSeriesHeader.TabIndex = 0;
        // 
        // topBarLayout
        // 
        this.topBarLayout.ColumnCount = 7;
        this.topBarLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this.topBarLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this.topBarLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this.topBarLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.topBarLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this.topBarLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this.topBarLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this.topBarLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this.topBarLayout.Location = new System.Drawing.Point(0, 0);
        this.topBarLayout.Name = "topBarLayout";
        this.topBarLayout.RowCount = 1;
        this.topBarLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.topBarLayout.Size = new System.Drawing.Size(867, 40);
        this.topBarLayout.TabIndex = 0;
        this.topBarLayout.Controls.Add(this.btnSyncBangumi, 0, 0);
        this.topBarLayout.Controls.Add(this.btnConfigureUser, 1, 0);
        this.topBarLayout.Controls.Add(this.labelSeriesSearch, 2, 0);
        this.topBarLayout.Controls.Add(this.txtSeriesSearch, 3, 0);
        this.topBarLayout.Controls.Add(this.labelSeriesFilter, 4, 0);
        this.topBarLayout.Controls.Add(this.cmbSeriesFilter, 5, 0);
        // 
        // labelSeriesSearch
        // 
        this.labelSeriesSearch.AutoSize = true;
        this.labelSeriesSearch.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.labelSeriesSearch.Name = "labelSeriesSearch";
        this.labelSeriesSearch.Size = new System.Drawing.Size(44, 20);
        this.labelSeriesSearch.TabIndex = 2;
        this.labelSeriesSearch.Text = "搜索";
        // 
        // txtSeriesSearch
        // 
        this.txtSeriesSearch.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtSeriesSearch.Name = "txtSeriesSearch";
        this.txtSeriesSearch.Size = new System.Drawing.Size(100, 27);
        this.txtSeriesSearch.TabIndex = 3;
        this.txtSeriesSearch.TextChanged += new System.EventHandler(this.txtSeriesSearch_TextChanged);
        // 
        // labelSeriesFilter
        // 
        this.labelSeriesFilter.AutoSize = true;
        this.labelSeriesFilter.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.labelSeriesFilter.Name = "labelSeriesFilter";
        this.labelSeriesFilter.Size = new System.Drawing.Size(44, 20);
        this.labelSeriesFilter.TabIndex = 4;
        this.labelSeriesFilter.Text = "筛选";
        // 
        // cmbSeriesFilter
        // 
        this.cmbSeriesFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbSeriesFilter.Dock = System.Windows.Forms.DockStyle.Left;
        this.cmbSeriesFilter.FormattingEnabled = true;
        this.cmbSeriesFilter.Items.AddRange(new object[] {
            "全部",
            "仅有本地",
            "无本地"});
        this.cmbSeriesFilter.Name = "cmbSeriesFilter";
        this.cmbSeriesFilter.Size = new System.Drawing.Size(86, 28);
        this.cmbSeriesFilter.TabIndex = 5;
        this.cmbSeriesFilter.SelectedIndex = 0;
        this.cmbSeriesFilter.SelectedIndexChanged += new System.EventHandler(this.cmbSeriesFilter_SelectedIndexChanged);
        // 
        // btnSyncBangumi
        // 
        this.btnSyncBangumi.Dock = System.Windows.Forms.DockStyle.Fill;
        this.btnSyncBangumi.Margin = new System.Windows.Forms.Padding(0, 5, 8, 5);
        this.btnSyncBangumi.Name = "btnSyncBangumi";
        this.btnSyncBangumi.Size = new System.Drawing.Size(108, 29);
        this.btnSyncBangumi.TabIndex = 6;
        this.btnSyncBangumi.Text = "同步 Bangumi";
        this.btnSyncBangumi.UseVisualStyleBackColor = true;
        this.btnSyncBangumi.Click += new System.EventHandler(this.btnSyncBangumi_Click);
        // 
        // btnConfigureUser
        // 
        this.btnConfigureUser.Dock = System.Windows.Forms.DockStyle.Fill;
        this.btnConfigureUser.Margin = new System.Windows.Forms.Padding(0, 5, 16, 5);
        this.btnConfigureUser.Name = "btnConfigureUser";
        this.btnConfigureUser.Size = new System.Drawing.Size(108, 29);
        this.btnConfigureUser.TabIndex = 7;
        this.btnConfigureUser.Text = "设置";
        this.btnConfigureUser.UseVisualStyleBackColor = true;
        this.btnConfigureUser.Click += new System.EventHandler(this.btnConfigureUser_Click);
        // 
        // dgvSeries
        // 
        this.dgvSeries.AllowUserToAddRows = false;
        this.dgvSeries.AllowUserToDeleteRows = false;
        this.dgvSeries.AllowUserToResizeRows = false;
        this.dgvSeries.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.dgvSeries.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvSeries.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvSeries.BackgroundColor = System.Drawing.SystemColors.Window;
        this.dgvSeries.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        this.dgvSeries.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvSeries.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSeriesTitle,
            this.colSeriesLocalCount,
            this.colSeriesLocalFolder});
        this.dgvSeries.Location = new System.Drawing.Point(0, 0);
        this.dgvSeries.MultiSelect = false;
        this.dgvSeries.Name = "dgvSeries";
        this.dgvSeries.ReadOnly = true;
        this.dgvSeries.RowHeadersVisible = false;
        this.dgvSeries.RowTemplate.Height = 29;
        this.dgvSeries.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvSeries.Size = new System.Drawing.Size(360, 534);
        this.dgvSeries.TabIndex = 0;
        this.dgvSeries.SelectionChanged += new System.EventHandler(this.dgvSeries_SelectionChanged);
        // 
        // colSeriesTitle
        // 
        this.colSeriesTitle.DataPropertyName = "DisplayTitle";
        this.colSeriesTitle.HeaderText = "番剧名称";
        this.colSeriesTitle.MinimumWidth = 150;
        this.colSeriesTitle.Name = "colSeriesTitle";
        this.colSeriesTitle.ReadOnly = true;
        this.colSeriesTitle.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
        // 
        // colSeriesLocalCount
        // 
        this.colSeriesLocalCount.DataPropertyName = "LocalMatchCount";
        this.colSeriesLocalCount.HeaderText = "本地集数";
        this.colSeriesLocalCount.MinimumWidth = 70;
        this.colSeriesLocalCount.Name = "colSeriesLocalCount";
        this.colSeriesLocalCount.ReadOnly = true;
        this.colSeriesLocalCount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
        // 
        // colSeriesLocalFolder
        // 
        this.colSeriesLocalFolder.DataPropertyName = "LocalFolderDisplay";
        this.colSeriesLocalFolder.HeaderText = "本地目录";
        this.colSeriesLocalFolder.MinimumWidth = 90;
        this.colSeriesLocalFolder.Name = "colSeriesLocalFolder";
        this.colSeriesLocalFolder.ReadOnly = true;
        this.colSeriesLocalFolder.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
        // 
        // dgvEpisodes
        // 
        this.dgvEpisodes.AllowUserToAddRows = false;
        this.dgvEpisodes.AllowUserToDeleteRows = false;
        this.dgvEpisodes.AllowUserToResizeRows = false;
        this.dgvEpisodes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.dgvEpisodes.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvEpisodes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvEpisodes.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
        this.dgvEpisodes.BackgroundColor = System.Drawing.SystemColors.Window;
        this.dgvEpisodes.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        this.dgvEpisodes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvEpisodes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colEpisodeNumber,
            this.colEpisodeTitle,
            this.colEpisodePlay,
            this.colEpisodeMarkWatched});
        this.dgvEpisodes.Location = new System.Drawing.Point(0, 0);
        this.dgvEpisodes.MultiSelect = false;
        this.dgvEpisodes.Name = "dgvEpisodes";
        this.dgvEpisodes.ReadOnly = true;
        this.dgvEpisodes.RowHeadersVisible = false;
        this.dgvEpisodes.RowTemplate.Height = 29;
        this.dgvEpisodes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvEpisodes.Size = new System.Drawing.Size(503, 365);
        this.dgvEpisodes.TabIndex = 0;
        this.dgvEpisodes.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvEpisodes_CellContentClick);
        this.dgvEpisodes.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.dgvEpisodes_DataBindingComplete);
        // 
        // colEpisodeNumber
        // 
        this.colEpisodeNumber.DataPropertyName = "EpisodeNumber";
        this.colEpisodeNumber.HeaderText = "集数";
        this.colEpisodeNumber.MinimumWidth = 60;
        this.colEpisodeNumber.Name = "colEpisodeNumber";
        this.colEpisodeNumber.ReadOnly = true;
        this.colEpisodeNumber.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
        // 
        // colEpisodeTitle
        // 
        this.colEpisodeTitle.DataPropertyName = "Title";
        this.colEpisodeTitle.HeaderText = "标题";
        this.colEpisodeTitle.MinimumWidth = 180;
        this.colEpisodeTitle.Name = "colEpisodeTitle";
        this.colEpisodeTitle.ReadOnly = true;
        this.colEpisodeTitle.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
        this.colEpisodeTitle.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
        // 
        // colEpisodePlay
        // 
        this.colEpisodePlay.HeaderText = "播放";
        this.colEpisodePlay.MinimumWidth = 80;
        this.colEpisodePlay.Name = "colEpisodePlay";
        this.colEpisodePlay.ReadOnly = true;
        this.colEpisodePlay.Resizable = System.Windows.Forms.DataGridViewTriState.False;
        this.colEpisodePlay.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
        this.colEpisodePlay.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
        // 
        // colEpisodeMarkWatched
        // 
        this.colEpisodeMarkWatched.HeaderText = "操作";
        this.colEpisodeMarkWatched.MinimumWidth = 90;
        this.colEpisodeMarkWatched.Name = "colEpisodeMarkWatched";
        this.colEpisodeMarkWatched.ReadOnly = true;
        this.colEpisodeMarkWatched.Resizable = System.Windows.Forms.DataGridViewTriState.False;
        this.colEpisodeMarkWatched.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
        this.colEpisodeMarkWatched.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
        // 
        // statusStrip
        // 
        this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
        this.statusStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
        this.statusStrip.Location = new System.Drawing.Point(0, 594);
        this.statusStrip.Name = "statusStrip";
        this.statusStrip.Size = new System.Drawing.Size(891, 26);
        this.statusStrip.TabIndex = 12;
        this.statusStrip.Text = "statusStrip";
        // 
        // toolStripStatusLabel
        // 
        this.toolStripStatusLabel.Name = "toolStripStatusLabel";
        this.toolStripStatusLabel.Size = new System.Drawing.Size(89, 20);
        this.toolStripStatusLabel.Text = "准备就绪。";
        // 
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(891, 620);
        this.Controls.Add(this.statusStrip);
        this.Controls.Add(this.panelSeriesHeader);
        this.Controls.Add(this.splitContainerMain);
        this.Controls.Add(this.btnChangeUser);
        this.Controls.Add(this.btnSaveConfig);
        this.Controls.Add(this.txtAccessToken);
        this.Controls.Add(this.labelAccessToken);
        this.Controls.Add(this.txtBangumiUsername);
        this.Controls.Add(this.labelBangumiUsername);
        this.MinimumSize = new System.Drawing.Size(909, 612);
        this.Name = "Form1";
        this.Text = "Anime Watch Tracker";
        this.splitContainerMain.Panel1.ResumeLayout(false);
        this.splitContainerMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
        this.splitContainerMain.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvSeries)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.dgvEpisodes)).EndInit();
        this.statusStrip.ResumeLayout(false);
        this.statusStrip.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion
}
