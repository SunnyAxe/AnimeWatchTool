#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace AnimeWatchTool;

public partial class Form1 : Form
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://api.bgm.tv/v0/"),
        Timeout = TimeSpan.FromSeconds(30)
    };
    private readonly List<LocalSeries> _localSeries = new();
    private readonly List<WatchlistItem> _watchlist = new();
    private AppConfig _config = new();
    private BangumiCache _bangumiCache = new();
    private bool _suppressSeriesSelectionChanged;
    private string _seriesSearchText = string.Empty;
    private SeriesFilterMode _seriesFilterMode = SeriesFilterMode.All;
    private const string ConfigFileName = "config.json";
    private const string CacheFileName = "bangumi_cache.json";

    private enum SeriesFilterMode
    {
        All,
        HasLocal,
        NoLocal
    }

    public Form1()
    {
        InitializeComponent();
        // 保留 Designer 中加载的自定义图标（若存在），仅在未加载时使用系统默认图标。
        if (this.Icon == null)
        {
            this.Icon = System.Drawing.SystemIcons.Application;
        }
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AnimeWatchTool/0.1 (https://github.com/yourname/animewatchtool)");
        LoadConfig();
        LoadCache();
        txtBangumiUsername.Text = _config.Username;
        txtAccessToken.Text = _config.AccessToken;
        if (string.IsNullOrWhiteSpace(_config.LocalFolder))
        {
            _config.LocalFolder = Path.Combine(GetWorkspaceRoot(), "anime");
        }
        InitializeGrids();
        this.Load += Form1_Load;
    }

    private async Task<HttpResponseMessage> GetLoggedAsync(string requestUri, bool includeAuth = false)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        if (includeAuth && !string.IsNullOrWhiteSpace(_config.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.AccessToken.Trim());
        }

        return await SendLoggedAsync(request);
    }

    private async Task<HttpResponseMessage> SendLoggedAsync(HttpRequestMessage request)
    {
        LogHttp($"{request.Method} {request.RequestUri}");
        if (request.Content != null)
        {
            var content = await request.Content.ReadAsStringAsync();
            LogHttp($"BODY: {content}");
        }

        try
        {
            return await _httpClient.SendAsync(request);
        }
        catch (TaskCanceledException ex)
        {
            LogHttp($"HTTP request timed out: {request.Method} {request.RequestUri} - {ex.Message}");
            throw;
        }
    }

    private async void Form1_Load(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_config.Username) && _bangumiCache.Username == _config.Username && _bangumiCache.WatchlistItems.Any())
        {
            _watchlist.Clear();
            _watchlist.AddRange(_bangumiCache.WatchlistItems);
            UpdateSeriesGrid();
            ShowStatus("已加载本地缓存的 Bangumi 数据。请点击番剧查看详细集数和已看状态。");
        }

        if (Directory.Exists(_config.LocalFolder))
        {
            await ScanLocalFolderAsync(false);
        }

        if (!string.IsNullOrWhiteSpace(_config.Username))
        {
            await FetchBangumiAsync(false);
        }
    }

    private static string GetWorkspaceRoot()
    {
        var currentDir = AppDomain.CurrentDomain.BaseDirectory;
        var directory = new DirectoryInfo(currentDir);

        while (directory != null)
        {
            if (directory.GetFiles("*.csproj").Any())
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return AppDomain.CurrentDomain.BaseDirectory;
    }

    private static void LogHttp(string message)
    {
        var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        try
        {
            Console.WriteLine(logLine);
        }
        catch
        {
            // 忽略控制台写入失败
        }

        try
        {
            Debug.WriteLine(logLine);
        }
        catch
        {
            // 忽略调试输出失败
        }
    }

    private void InitializeGrids()
    {
        dgvSeries.AutoGenerateColumns = false;
        dgvSeries.ReadOnly = true;
        dgvSeries.AllowUserToAddRows = false;
        dgvSeries.AllowUserToDeleteRows = false;
        dgvSeries.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvSeries.MultiSelect = false;
        dgvSeries.RowHeadersVisible = false;

        dgvEpisodes.AutoGenerateColumns = false;
        dgvEpisodes.ReadOnly = true;
        dgvEpisodes.AllowUserToAddRows = false;
        dgvEpisodes.AllowUserToDeleteRows = false;
        dgvEpisodes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvEpisodes.MultiSelect = false;
        dgvEpisodes.RowHeadersVisible = false;

        colEpisodePlay.UseColumnTextForButtonValue = false;
        colEpisodeMarkWatched.UseColumnTextForButtonValue = false;
    }

    private void LoadConfig()
    {
        var path = Path.Combine(GetWorkspaceRoot(), ConfigFileName);
        if (!File.Exists(path))
        {
            _config = new AppConfig();
            return;
        }

        try
        {
            var json = File.ReadAllText(path, Encoding.UTF8);
            _config = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AppConfig();
        }
        catch
        {
            _config = new AppConfig();
        }
    }

    private void SaveConfig()
    {
        _config.Username = txtBangumiUsername.Text.Trim();
        _config.AccessToken = txtAccessToken.Text.Trim();
        var path = Path.Combine(GetWorkspaceRoot(), ConfigFileName);
        File.WriteAllText(path, JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true }), Encoding.UTF8);
    }

    private void LoadCache()
    {
        var path = Path.Combine(GetWorkspaceRoot(), CacheFileName);
        if (!File.Exists(path))
        {
            _bangumiCache = new BangumiCache();
            return;
        }

        try
        {
            var json = File.ReadAllText(path, Encoding.UTF8);
            _bangumiCache = JsonSerializer.Deserialize<BangumiCache>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new BangumiCache();

            foreach (var item in _bangumiCache.WatchlistItems)
            {
                item.CachedEpisodeRows.Clear();
            }

            ApplyCachedLocalFolderAssignments(_bangumiCache.WatchlistItems);
        }
        catch
        {
            _bangumiCache = new BangumiCache();
        }
    }

    private void SaveCache()
    {
        _bangumiCache.Username = _config.Username;
        _bangumiCache.WatchlistItems = _watchlist.Select(item => new WatchlistItem
        {
            Subject = item.Subject,
            LocalFolderName = item.LocalFolderName,
            LocalFolderPath = item.LocalFolderPath,
            LocalEpisodes = item.LocalEpisodes.Select(ep => new LocalEpisode
            {
                EpisodeNumber = ep.EpisodeNumber,
                FilePath = ep.FilePath,
                DisplayName = ep.DisplayName
            }).ToList(),
            Episodes = item.Episodes.Select(ep => new BangumiEpisode
            {
                Id = ep.Id,
                Sort = ep.Sort,
                Name = ep.Name,
                Airdate = ep.Airdate
            }).ToList(),
            WatchedEpisodeIds = new HashSet<int>(item.WatchedEpisodeIds),
            WatchedEpisodeTimes = new Dictionary<int, long>(item.WatchedEpisodeTimes),
            EpisodeAirdateSyncAttempted = item.EpisodeAirdateSyncAttempted
        }).ToList();

        _bangumiCache.LocalFolderAssignments = _watchlist
            .Where(item => !string.IsNullOrWhiteSpace(item.LocalFolderPath) && item.LocalEpisodes.Any())
            .ToDictionary(
                item => item.Subject.Id,
                item => new LocalFolderAssignment
                {
                    SubjectId = item.Subject.Id,
                    FolderName = item.LocalFolderName,
                    FolderPath = item.LocalFolderPath,
                    LocalEpisodes = item.LocalEpisodes.Select(ep => new LocalEpisode
                    {
                        EpisodeNumber = ep.EpisodeNumber,
                        FilePath = ep.FilePath,
                        DisplayName = ep.DisplayName
                    }).ToList()
                });

        var path = Path.Combine(GetWorkspaceRoot(), CacheFileName);
        File.WriteAllText(path, JsonSerializer.Serialize(_bangumiCache, new JsonSerializerOptions { WriteIndented = true }), Encoding.UTF8);
    }

    private void ApplyCachedLocalFolderAssignments(IEnumerable<WatchlistItem> items)
    {
        if (_bangumiCache.LocalFolderAssignments == null || !_bangumiCache.LocalFolderAssignments.Any())
        {
            return;
        }

        foreach (var item in items)
        {
            if (!_bangumiCache.LocalFolderAssignments.TryGetValue(item.Subject.Id, out var assignment))
            {
                continue;
            }

            item.LocalFolderName = assignment.FolderName;
            item.LocalFolderPath = assignment.FolderPath;
            item.LocalEpisodes = assignment.LocalEpisodes?.Select(ep => new LocalEpisode
            {
                EpisodeNumber = ep.EpisodeNumber,
                FilePath = ep.FilePath,
                DisplayName = ep.DisplayName
            }).ToList() ?? new List<LocalEpisode>();
        }
    }

    private void btnSaveConfig_Click(object sender, EventArgs e)
    {
        SaveConfig();
        ShowStatus("配置已保存，下次打开会自动同步。");
    }

    private void btnChangeUser_Click(object sender, EventArgs e)
    {
        txtBangumiUsername.ReadOnly = false;
        txtAccessToken.ReadOnly = false;
        txtBangumiUsername.Text = string.Empty;
        txtAccessToken.Text = string.Empty;
        ShowStatus("请输入新的 Bangumi 用户名和 Token，然后保存设置。");
    }

    private async void btnConfigureUser_Click(object sender, EventArgs e)
    {
        using var dialog = new UserConfigForm(_config.Username, _config.AccessToken, _config.LocalFolder);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var newUsername = dialog.Username.Trim();
        var newToken = dialog.AccessToken.Trim();
        var newLocalFolder = dialog.LocalFolder.Trim();
        var oldUsername = _config.Username;
        var oldLocalFolder = _config.LocalFolder;
        var isSwitchAccount = !string.Equals(oldUsername?.Trim(), newUsername, StringComparison.OrdinalIgnoreCase);

        _config.Username = newUsername;
        _config.AccessToken = newToken;
        _config.LocalFolder = newLocalFolder;
        txtBangumiUsername.Text = _config.Username;
        txtAccessToken.Text = _config.AccessToken;
        SaveConfig();

        await ScanLocalFolderAsync();
        ShowStatus("本地目录已扫描完成。点击同步以刷新 Bangumi 状态。");

        if (isSwitchAccount)
        {
            _watchlist.Clear();
            _bangumiCache = new BangumiCache { Username = _config.Username };
            UpdateSeriesGrid();
            dgvEpisodes.DataSource = null;
            ShowStatus("账号已切换，点击同步以拉取新的 Bangumi 列表。");
        }
        else if (string.Equals(oldLocalFolder?.Trim(), newLocalFolder, StringComparison.OrdinalIgnoreCase))
        {
            ShowStatus("用户信息已保存。点击同步以刷新已看状态。");
        }
    }

    private async void btnSyncBangumi_Click(object sender, EventArgs e)
    {
        await FetchBangumiAsync(true);
    }

    private async void btnSetLocalFolder_Click(object sender, EventArgs e)
    {
        if (dgvSeries.SelectedRows.Count == 0 || dgvSeries.SelectedRows[0].DataBoundItem is not WatchlistItem selectedItem)
        {
            MessageBox.Show(this, "请先在番剧列表中选择一个条目。", "未选择番剧", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var browser = new FolderBrowserDialog();
        if (Directory.Exists(selectedItem.LocalFolderPath))
        {
            browser.SelectedPath = selectedItem.LocalFolderPath;
        }
        else if (Directory.Exists(_config.LocalFolder))
        {
            browser.SelectedPath = _config.LocalFolder;
        }

        if (browser.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var folder = browser.SelectedPath;
        var episodes = ScanLocalEpisodesInFolder(folder);
        if (!episodes.Any())
        {
            ShowStatus("该文件夹中未检测到任何可识别的视频集数，请选择正确的番剧目录。");
            return;
        }

        selectedItem.LocalFolderName = Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        selectedItem.LocalFolderPath = folder;
        selectedItem.LocalEpisodes = episodes;
        selectedItem.CachedEpisodeRows.Clear();
        SaveCache();
        UpdateSeriesGrid(selectedItem.Subject.Id);
        RefreshSeriesRowDisplay(selectedItem);
        dgvSeries.Refresh();
        await LoadEpisodeRowsAsync(selectedItem);
        ShowStatus($"已为 {selectedItem.DisplayTitle} 指定本地目录并重新扫描。\n{episodes.Count} 集已加载。");
    }

    private async void btnClearLocalFolder_Click(object sender, EventArgs e)
    {
        if (dgvSeries.SelectedRows.Count == 0 || dgvSeries.SelectedRows[0].DataBoundItem is not WatchlistItem selectedItem)
        {
            MessageBox.Show(this, "请先在番剧列表中选择一个条目。", "未选择番剧", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        selectedItem.LocalFolderName = string.Empty;
        selectedItem.LocalFolderPath = string.Empty;
        selectedItem.LocalEpisodes = new List<LocalEpisode>();
        selectedItem.CachedEpisodeRows.Clear();
        SaveCache();
        UpdateSeriesGrid(selectedItem.Subject.Id);
        RefreshSeriesRowDisplay(selectedItem);
        await LoadEpisodeRowsAsync(selectedItem);
        ShowStatus($"已清除 {selectedItem.DisplayTitle} 的本地目录设置，并重新加载该条目。");
    }

    private LocalSeries FindLocalSeriesByTitle(WatchlistItem item)
    {
        var watchKey = NormalizeTitle(item.Subject.NameCn ?? item.Subject.Name);
        if (string.IsNullOrWhiteSpace(watchKey))
        {
            return null;
        }

        var match = _localSeries.FirstOrDefault(local =>
            NormalizeTitle(local.FolderName) == watchKey ||
            NormalizeTitle(local.FolderName).Contains(watchKey) ||
            watchKey.Contains(NormalizeTitle(local.FolderName)));

        if (match != null)
        {
            return match;
        }

        var rootFolder = _config.LocalFolder.Trim();
        if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder))
        {
            return null;
        }

        foreach (var directory in Directory.EnumerateDirectories(rootFolder, "*", SearchOption.AllDirectories))
        {
            var normalizedDirName = NormalizeTitle(Path.GetFileName(directory) ?? string.Empty);
            if (string.IsNullOrWhiteSpace(normalizedDirName))
            {
                continue;
            }

            if (normalizedDirName == watchKey || normalizedDirName.Contains(watchKey) || watchKey.Contains(normalizedDirName))
            {
                var episodes = ScanLocalEpisodesInFolder(directory);
                if (episodes.Any())
                {
                    return new LocalSeries
                    {
                        FolderName = Path.GetFileName(directory) ?? string.Empty,
                        FolderPath = directory,
                        Episodes = episodes
                    };
                }
            }
        }

        return null;
    }

    private async void btnMatchByTitle_Click(object sender, EventArgs e)
    {
        if (_localSeries.Count == 0)
        {
            await ScanLocalFolderAsync();
            if (_localSeries.Count == 0)
            {
                ShowStatus("本地目录尚未扫描或未发现可用目录。请先在设置中确认本地根目录后重试。");
                return;
            }
        }

        var matchedItems = new List<WatchlistItem>();
        foreach (var item in _watchlist.Where(i => !i.HasLocal))
        {
            var match = FindLocalSeriesByTitle(item);
            if (match == null)
            {
                continue;
            }

            item.LocalFolderName = match.FolderName;
            item.LocalFolderPath = match.FolderPath;
            item.LocalEpisodes = match.Episodes;
            item.CachedEpisodeRows.Clear();
            matchedItems.Add(item);
        }

        if (!matchedItems.Any())
        {
            ShowStatus("未找到可按片名匹配的未匹配番剧，本地匹配结果保持不变。");
            return;
        }

        SaveCache();
        UpdateSeriesGrid();

        if (dgvSeries.SelectedRows.Count > 0 && dgvSeries.SelectedRows[0].DataBoundItem is WatchlistItem selectedItem && matchedItems.Contains(selectedItem))
        {
            await LoadEpisodeRowsAsync(selectedItem);
        }

        ShowStatus($"已按片名自动匹配 {matchedItems.Count} 个未匹配番剧的本地目录。已匹配条目将保持已匹配状态。 ");
    }

    private Task ScanLocalFolderAsync(bool showStatus = true)
    {
        var folder = _config.LocalFolder.Trim();
        if (!Directory.Exists(folder))
        {
            ShowStatus("本地 anime 目录不存在，请先选择正确路径。请在设置中修改。");
            return Task.CompletedTask;
        }

        _localSeries.Clear();
        var allFiles = Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
            .Where(f => new[] { ".mp4", ".mkv", ".avi", ".flv", ".wmv", ".mov", ".rmvb" }
                .Contains(Path.GetExtension(f).ToLowerInvariant()))
            .ToList();

        var groups = allFiles.GroupBy(file => GetTopFolderPath(folder, Path.GetDirectoryName(file)!));
        foreach (var group in groups)
        {
            var folderPath = group.Key;
            var folderName = Path.GetFileName(folderPath) ?? folderPath;
            var episodes = ScanLocalEpisodesInFolder(folderPath);

            if (!episodes.Any())
            {
                continue;
            }

            _localSeries.Add(new LocalSeries
            {
                FolderName = folderName,
                FolderPath = folderPath,
                Episodes = episodes
            });
        }

        MatchLocalAndBangumiSeries();
        UpdateSeriesGrid();
        if (showStatus)
        {
            ShowStatus($"已扫描本地目录，发现 {_localSeries.Count} 个可用番剧目录。");
        }

        return Task.CompletedTask;
    }

    private static string GetTopFolderPath(string rootFolder, string directory)
    {
        var relative = Path.GetRelativePath(rootFolder, directory);
        if (string.IsNullOrWhiteSpace(relative) || relative == ".")
        {
            return rootFolder;
        }

        var parts = relative.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? rootFolder : Path.Combine(rootFolder, parts[0]);
    }

    private static List<LocalEpisode> ScanLocalEpisodesInFolder(string folder)
    {
        var files = Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
            .Where(f => new[] { ".mp4", ".mkv", ".avi", ".flv", ".wmv", ".mov", ".rmvb" }
                .Contains(Path.GetExtension(f).ToLowerInvariant()))
            .ToList();

        var episodes = new List<LocalEpisode>();
        foreach (var file in files)
        {
            var episodeNumber = ExtractEpisodeNumber(Path.GetFileName(file), out var label);
            if (episodeNumber <= 0)
            {
                continue;
            }

            episodes.Add(new LocalEpisode
            {
                EpisodeNumber = episodeNumber,
                FilePath = file,
                DisplayName = $"第 {episodeNumber} 集 - {label}"
            });
        }

        episodes.Sort((a, b) => a.EpisodeNumber.CompareTo(b.EpisodeNumber));
        return episodes;
    }

    private async Task FetchBangumiAsync(bool saveConfigAfterSync)
    {
        var username = _config.Username?.Trim();
        if (string.IsNullOrWhiteSpace(username))
        {
            ShowStatus("请先配置 Bangumi 用户名和 Token。点击 设置 按钮。");
            return;
        }

        try
        {
            btnSyncBangumi.Enabled = false;
            LogHttp($"Starting FetchBangumiAsync for user {username}");
            ShowStatus("正在从 Bangumi 拉取正在追番……");
            var subjects = (await FetchBangumiCollectionsAsync(username)).ToList();
            var previousIds = _watchlist.Select(item => item.Subject.Id).ToHashSet();
            _watchlist.Clear();
            var cachedById = _bangumiCache.WatchlistItems.ToDictionary(item => item.Subject.Id, item => item);
            foreach (var subject in subjects)
            {
                var item = new WatchlistItem
                {
                    Subject = subject
                };

                if (cachedById.TryGetValue(subject.Id, out var cachedItem))
                {
                    item.Episodes = cachedItem.Episodes;
                    item.WatchedEpisodeIds = new HashSet<int>(cachedItem.WatchedEpisodeIds);
                    item.WatchedEpisodeTimes = new Dictionary<int, long>(cachedItem.WatchedEpisodeTimes);
                    item.EpisodeAirdateSyncAttempted = cachedItem.EpisodeAirdateSyncAttempted;
                    item.WatchedStatusLoaded = cachedItem.WatchedStatusLoaded || item.WatchedEpisodeIds.Any();
                    item.LocalFolderName = cachedItem.LocalFolderName;
                    item.LocalFolderPath = cachedItem.LocalFolderPath;
                    item.LocalEpisodes = cachedItem.LocalEpisodes ?? new List<LocalEpisode>();
                }

                if (_bangumiCache.LocalFolderAssignments != null && _bangumiCache.LocalFolderAssignments.TryGetValue(subject.Id, out var folderAssignment))
                {
                    item.LocalFolderName = folderAssignment.FolderName;
                    item.LocalFolderPath = folderAssignment.FolderPath;
                    item.LocalEpisodes = folderAssignment.LocalEpisodes?.Select(ep => new LocalEpisode
                    {
                        EpisodeNumber = ep.EpisodeNumber,
                        FilePath = ep.FilePath,
                        DisplayName = ep.DisplayName
                    }).ToList() ?? new List<LocalEpisode>();
                }

                _watchlist.Add(item);
            }

            var fetchedIds = subjects.Select(subject => subject.Id).ToHashSet();
            var shouldRefreshSeries = !previousIds.SetEquals(fetchedIds);

            _bangumiCache.Username = username;
            SaveCache();
            MatchLocalAndBangumiSeries();
            if (shouldRefreshSeries)
            {
                UpdateSeriesGrid();
                toolStripStatusLabel.Text = $"已拉取 {_watchlist.Count} 个 Bangumi 在看条目。";
            }
            else
            {
                toolStripStatusLabel.Text = "Bangumi 列表与本地缓存一致，已保留当前左侧显示。";
            }

            if (saveConfigAfterSync)
            {
                SaveConfig();
            }
            _ = PrefetchMissingEpisodesAsync();
        }
        catch (Exception ex)
        {
            ShowStatus($"Bangumi 拉取失败：{ex.Message}");
        }
        finally
        {
            btnSyncBangumi.Enabled = true;
        }
    }

    private async Task<IEnumerable<BangumiSubject>> FetchBangumiCollectionsAsync(string username)
    {
        var items = new List<BangumiSubject>();
        var offset = 0;
        const int limit = 100;

        while (true)
        {
            var requestUri = $"users/{Uri.EscapeDataString(username)}/collections?status=doing&subject_type=2&limit={limit}&offset={offset}";
            using var response = await GetLoggedAsync(requestUri);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException($"Bangumi API 返回 404，可能是用户名错误。请求 URL：{response.RequestMessage?.RequestUri}");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException($"Bangumi API 返回 400，可能是参数错误。请求 URL：{response.RequestMessage?.RequestUri}");
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException($"Bangumi API 返回 {response.StatusCode}，请检查网络或权限。请求 URL：{response.RequestMessage?.RequestUri}");
            }

            response.EnsureSuccessStatusCode();
            using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            if (!document.RootElement.TryGetProperty("data", out var dataElement))
            {
                break;
            }

            foreach (var node in dataElement.EnumerateArray())
            {
                if (!node.TryGetProperty("subject", out var subjectNode))
                {
                    continue;
                }

                if (!node.TryGetProperty("type", out var typeNode) || typeNode.GetInt32() != 3)
                {
                    continue;
                }

                if (!node.TryGetProperty("subject_type", out var subjectTypeNode) || subjectTypeNode.GetInt32() != 2)
                {
                    continue;
                }

                var id = subjectNode.GetProperty("id").GetInt32();
                var name = subjectNode.GetProperty("name").GetString() ?? string.Empty;
                var nameCn = subjectNode.TryGetProperty("name_cn", out var nameCnNode) ? nameCnNode.GetString() : string.Empty;
                items.Add(new BangumiSubject { Id = id, Name = name, NameCn = nameCn });
            }

            var total = document.RootElement.TryGetProperty("total", out var totalNode) ? totalNode.GetInt32() : 0;
            offset += limit;
            if (offset >= total || dataElement.GetArrayLength() == 0)
            {
                break;
            }
        }

        return items;
    }

    private async Task<IEnumerable<BangumiEpisode>> FetchBangumiEpisodesAsync(int subjectId)
    {
        try
        {
            using var response = await GetLoggedAsync($"episodes?subject_id={subjectId}&limit=1000");
            if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.BadRequest)
            {
                return await FetchBangumiEpisodesFromWebAsync(subjectId);
            }

            response.EnsureSuccessStatusCode();
            using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            if (!document.RootElement.TryGetProperty("data", out var dataElement))
            {
                return await FetchBangumiEpisodesFromWebAsync(subjectId);
            }

            var result = new List<BangumiEpisode>();
            foreach (var node in dataElement.EnumerateArray())
            {
                var id = node.GetProperty("id").GetInt32();
                var sort = node.GetProperty("sort").GetInt32();
                var name = node.TryGetProperty("name", out var nameNode) ? nameNode.GetString() ?? string.Empty : string.Empty;
                var airdate = node.TryGetProperty("airdate", out var adNode) ? adNode.GetString() ?? string.Empty : string.Empty;
                result.Add(new BangumiEpisode { Id = id, Sort = sort, Name = name, Airdate = airdate });
            }

            if (result.Count == 0)
            {
                return await FetchBangumiEpisodesFromWebAsync(subjectId);
            }

            return result;
        }
        catch
        {
            return await FetchBangumiEpisodesFromWebAsync(subjectId);
        }
    }

    private async Task<IEnumerable<BangumiEpisode>> FetchBangumiEpisodesFromWebAsync(int subjectId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"https://bgm.tv/subject/{subjectId}/ep");
        request.Headers.UserAgent.ParseAdd("AnimeWatchTool/0.1");
        using var response = await SendLoggedAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<BangumiEpisode>();
        }

        var html = await response.Content.ReadAsStringAsync();
        var matches = Regex.Matches(html, "href=\"/ep/(\\d+)\"[^>]*>([^<]+)</a>", RegexOptions.IgnoreCase);
        var episodes = new List<BangumiEpisode>();
        foreach (Match match in matches)
        {
            if (!int.TryParse(match.Groups[1].Value, out var id))
            {
                continue;
            }

            var title = match.Groups[2].Value.Trim();
            if (episodes.Any(e => e.Id == id))
            {
                continue;
            }

            var sort = ParseEpisodeSortFromWebTitle(title);
            episodes.Add(new BangumiEpisode
            {
                Id = id,
                Sort = sort >= 0 ? sort : episodes.Count + 1,
                Name = title
            });
        }

        return episodes.OrderBy(ep => ep.Sort).ToList();
    }

    private static int ParseEpisodeSortFromWebTitle(string title)
    {
        var match = Regex.Match(title, "^(\\d+)\\.");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var sort))
        {
            return sort;
        }

        return -1;
    }

    private async Task<(bool Success, string ErrorMessage)> UpdateEpisodeWatchedStatusAsync(int subjectId, int episodeId, int type)
    {
        var token = _config.AccessToken?.Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return (false, "请先配置 Bangumi Token。点击 设置 并保存。");
        }

        using var request = new HttpRequestMessage(HttpMethod.Patch, $"users/-/collections/{subjectId}/episodes");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var payload = JsonSerializer.Serialize(new { episode_id = new[] { episodeId }, type });
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        using var response = await SendLoggedAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return (true, string.Empty);
        }

        var content = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(content) ? response.ReasonPhrase ?? "未知错误" : content);
    }

    private async Task<bool> FetchWatchedStatusForWatchlistAsync()
    {
        if (string.IsNullOrWhiteSpace(_config.AccessToken) || !_watchlist.Any())
        {
            return false;
        }

        const int concurrency = 1;
        var anyChanged = false;
        ShowStatus($"正在同步已看状态：共 {_watchlist.Count} 个番剧，当前串行请求处理...");

        using var semaphore = new SemaphoreSlim(concurrency);
        var tasks = _watchlist.Select(async item =>
        {
            await semaphore.WaitAsync();
            try
            {
                LogHttp($"Sync watched status for subject {item.Subject.Id}");
                var watchedStatusMap = await FetchWatchedEpisodeStatusMapAsync(item.Subject.Id);
                var newWatchedIds = watchedStatusMap.Keys.ToHashSet();
                if (!item.WatchedEpisodeIds.SetEquals(newWatchedIds) || !DictionaryEquals(item.WatchedEpisodeTimes, watchedStatusMap))
                {
                    item.WatchedEpisodeIds = new HashSet<int>(newWatchedIds);
                    item.WatchedEpisodeTimes = watchedStatusMap;
                    item.CachedEpisodeRows.Clear();
                    anyChanged = true;
                }
            }
            catch (Exception ex)
            {
                LogHttp($"Failed to sync watched status for subject {item.Subject.Id}: {ex.Message}");
            }
            finally
            {
                semaphore.Release();
            }
        }).ToArray();

        await Task.WhenAll(tasks);
        return anyChanged;
    }

    private async Task<bool> EnsureWatchedStatusLoadedAsync(WatchlistItem item)
    {
        if (string.IsNullOrWhiteSpace(_config.AccessToken) || item.WatchedStatusLoaded)
        {
            return false;
        }

        try
        {
            ShowStatus($"正在加载 {item.DisplayTitle} 的已看状态...");
            var watchedStatusMap = await FetchWatchedEpisodeStatusMapAsync(item.Subject.Id);
            item.WatchedEpisodeIds = watchedStatusMap.Keys.ToHashSet();
            item.WatchedEpisodeTimes = watchedStatusMap;
            item.WatchedStatusLoaded = true;
            item.CachedEpisodeRows.Clear();
            SaveCache();
            return true;
        }
        catch (Exception ex)
        {
            LogHttp($"Failed to load watched status for subject {item.Subject.Id}: {ex.Message}");
            return false;
        }
    }

    private async Task<Dictionary<int, long>> FetchWatchedEpisodeStatusMapAsync(int subjectId)
    {
        var result = new Dictionary<int, long>();
        var offset = 0;
        const int limit = 100;

        while (true)
        {
            var requestUri = $"users/-/collections/{subjectId}/episodes?limit={limit}&offset={offset}";
            using var response = await GetLoggedAsync(requestUri, includeAuth: true);
            if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.BadRequest)
            {
                return result;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                LogHttp($"Watched episode sync skipped for subject {subjectId}: {response.StatusCode}");
                return result;
            }

            response.EnsureSuccessStatusCode();
            using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            if (!document.RootElement.TryGetProperty("data", out var dataElement))
            {
                break;
            }

            foreach (var node in dataElement.EnumerateArray())
            {
                if (!node.TryGetProperty("episode", out var episodeNode))
                {
                    continue;
                }

                if (!node.TryGetProperty("type", out var typeNode) || typeNode.GetInt32() != 2)
                {
                    continue;
                }

                if (!episodeNode.TryGetProperty("id", out var idNode))
                {
                    continue;
                }

                var episodeId = idNode.GetInt32();
                var watchedAtUnix = 0L;
                if (node.TryGetProperty("updated_at", out var updatedAtNode))
                {
                    if (updatedAtNode.ValueKind == JsonValueKind.Number)
                    {
                        watchedAtUnix = updatedAtNode.GetInt64();
                    }
                    else if (updatedAtNode.ValueKind == JsonValueKind.String)
                    {
                        var raw = updatedAtNode.GetString() ?? string.Empty;
                        if (long.TryParse(raw, out var parsedUnix))
                        {
                            watchedAtUnix = parsedUnix;
                        }
                    }
                }

                result[episodeId] = watchedAtUnix;
            }

            var total = document.RootElement.TryGetProperty("total", out var totalNode) ? totalNode.GetInt32() : 0;
            offset += limit;
            if (offset >= total || dataElement.GetArrayLength() == 0)
            {
                break;
            }
        }

        return result;
    }

    private void MatchLocalAndBangumiSeries()
    {
        foreach (var item in _watchlist)
        {
            if (!string.IsNullOrWhiteSpace(item.LocalFolderPath) && Directory.Exists(item.LocalFolderPath) && item.LocalEpisodes.Any())
            {
                continue;
            }

            item.CachedEpisodeRows.Clear();
            item.LocalFolderName = string.Empty;
            item.LocalFolderPath = string.Empty;
            item.LocalEpisodes = new List<LocalEpisode>();

            var match = FindLocalSeriesByTitle(item);

            if (match != null)
            {
                item.LocalFolderName = match.FolderName;
                item.LocalFolderPath = match.FolderPath;
                item.LocalEpisodes = match.Episodes;
            }
        }
    }

    private void UpdateSeriesGrid(int? selectedSubjectId = null)
    {
        _suppressSeriesSelectionChanged = true;
        var seriesList = GetFilteredSeries()
            .OrderByDescending(item => item.HasLocal)
            .ThenBy(item => item.DisplayTitle)
            .ToList();

        dgvSeries.DataSource = null;
        dgvSeries.DataSource = seriesList;

        if (selectedSubjectId.HasValue)
        {
            var rowIndex = seriesList.FindIndex(item => item.Subject.Id == selectedSubjectId.Value);
            if (rowIndex >= 0 && rowIndex < dgvSeries.Rows.Count)
            {
                dgvSeries.ClearSelection();
                dgvSeries.CurrentCell = dgvSeries.Rows[rowIndex].Cells[0];
                dgvSeries.Rows[rowIndex].Selected = true;
                dgvSeries.Refresh();
            }
            else
            {
                dgvSeries.ClearSelection();
                dgvSeries.CurrentCell = null;
            }
        }
        else
        {
            dgvSeries.ClearSelection();
            dgvSeries.CurrentCell = null;
        }

        dgvEpisodes.DataSource = null;
        dgvSeries.Refresh();
        _suppressSeriesSelectionChanged = false;
    }

    private void RefreshSeriesRowDisplay(WatchlistItem item)
    {
        if (dgvSeries.DataSource == null)
        {
            return;
        }

        foreach (DataGridViewRow row in dgvSeries.Rows)
        {
            if (row.DataBoundItem is not WatchlistItem rowItem || rowItem.Subject.Id != item.Subject.Id)
            {
                continue;
            }

            row.Cells[colSeriesLocalCount.Index].Value = item.LocalMatchCount;
            row.Cells[colSeriesLocalFolder.Index].Value = item.LocalFolderDisplay;
            dgvSeries.InvalidateRow(row.Index);
            dgvSeries.Refresh();
            return;
        }
    }

    private IEnumerable<WatchlistItem> GetFilteredSeries()
    {
        var query = _watchlist.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(_seriesSearchText))
        {
            var search = _seriesSearchText.Trim().ToLowerInvariant();
            query = query.Where(item =>
                item.DisplayTitle.ToLowerInvariant().Contains(search) ||
                item.Subject.Name.ToLowerInvariant().Contains(search) ||
                item.Subject.NameCn.ToLowerInvariant().Contains(search) ||
                item.LocalFolderName.ToLowerInvariant().Contains(search) ||
                item.LocalFolderPath.ToLowerInvariant().Contains(search));
        }

        return _seriesFilterMode switch
        {
            SeriesFilterMode.HasLocal => query.Where(item => item.HasLocal),
            SeriesFilterMode.NoLocal => query.Where(item => !item.HasLocal),
            _ => query
        };
    }

    private void ApplySeriesFilter()
    {
        UpdateSeriesGrid();
    }

    private async void dgvSeries_SelectionChanged(object sender, EventArgs e)
    {
        if (_suppressSeriesSelectionChanged)
        {
            return;
        }

        if (dgvSeries.SelectedRows.Count == 0)
        {
            dgvEpisodes.DataSource = null;
            return;
        }

        if (dgvSeries.SelectedRows[0].DataBoundItem is not WatchlistItem item)
        {
            dgvEpisodes.DataSource = null;
            return;
        }

        await LoadEpisodeRowsAsync(item);
    }

    private void txtSeriesSearch_TextChanged(object sender, EventArgs e)
    {
        _seriesSearchText = txtSeriesSearch.Text;
        ApplySeriesFilter();
    }

    private void cmbSeriesFilter_SelectedIndexChanged(object sender, EventArgs e)
    {
        _seriesFilterMode = cmbSeriesFilter.SelectedIndex switch
        {
            1 => SeriesFilterMode.HasLocal,
            2 => SeriesFilterMode.NoLocal,
            _ => SeriesFilterMode.All,
        };

        ApplySeriesFilter();
    }

    private async Task LoadEpisodeRowsAsync(WatchlistItem item)
    {
        var needInitialLoad = !item.EpisodesLoaded;
        var needAirdateRefresh = item.EpisodesLoaded
            && !item.EpisodeAirdateSyncAttempted
            && item.Episodes.Any()
            && item.Episodes.All(ep => string.IsNullOrWhiteSpace(ep.Airdate));

        if (needInitialLoad || needAirdateRefresh)
        {
            ShowStatus("正在加载番剧集数...");
            try
            {
                item.Episodes = (await FetchBangumiEpisodesAsync(item.Subject.Id)).ToList();
                item.EpisodeAirdateSyncAttempted = true;
                SaveCache();
            }
            catch (Exception ex)
            {
                ShowStatus($"加载番剧集数失败：{ex.Message}");
                item.Episodes = new List<BangumiEpisode>();
            }
        }

        if (!item.CachedEpisodeRows.Any())
        {
            item.CachedEpisodeRows = BuildEpisodeRows(item);
        }

        dgvEpisodes.SuspendLayout();
        dgvEpisodes.DataSource = null;
        dgvEpisodes.DataSource = item.CachedEpisodeRows;
        dgvEpisodes.ResumeLayout();
        ShowStatus($"已加载 {item.CachedEpisodeRows.Count} 集，{item.CachedEpisodeRows.Count(r => r.CanPlay)} 集可本地播放。");

        if (!item.WatchedStatusLoaded && !string.IsNullOrWhiteSpace(_config.AccessToken))
        {
            _ = RefreshWatchedStatusForSelectedSeriesAsync(item);
        }
    }

    private List<EpisodeRow> BuildEpisodeRows(WatchlistItem item)
    {
        var localMap = item.LocalEpisodes
            .GroupBy(e => e.EpisodeNumber)
            .ToDictionary(g => g.Key, g => g.OrderBy(e => e.FilePath).First());

        return item.Episodes.Any()
            ? item.Episodes.Select(ep => new EpisodeRow
            {
                EpisodeId = ep.Id,
                EpisodeNumber = ep.Sort,
                Title = string.IsNullOrWhiteSpace(ep.Name) ? $"第{ep.Sort}集" : ep.Name,
                LocalFile = localMap.TryGetValue(ep.Sort, out var file) ? file.FilePath : string.Empty,
                IsWatched = item.WatchedEpisodeIds.Contains(ep.Id),
                Airdate = ep is BangumiEpisode be ? be.Airdate : string.Empty,
                WatchedAt = item.WatchedEpisodeTimes.TryGetValue(ep.Id, out var watchedAtUnix)
                    ? UnixTimestampToDisplay(watchedAtUnix)
                    : string.Empty
            }).ToList()
            : item.LocalEpisodes.OrderBy(e => e.EpisodeNumber).Select(ep => new EpisodeRow
            {
                EpisodeId = 0,
                EpisodeNumber = ep.EpisodeNumber,
                Title = ep.DisplayName,
                LocalFile = ep.FilePath,
                IsWatched = false
                ,
                Airdate = string.Empty,
                WatchedAt = string.Empty
            }).ToList();
    }

    private async Task RefreshWatchedStatusForSelectedSeriesAsync(WatchlistItem item)
    {
        if (string.IsNullOrWhiteSpace(_config.AccessToken) || item.WatchedStatusLoaded)
        {
            return;
        }

        try
        {
            var watchedStatusMap = await FetchWatchedEpisodeStatusMapAsync(item.Subject.Id);
            var newWatchedIds = watchedStatusMap.Keys.ToHashSet();
            var changed = !item.WatchedEpisodeIds.SetEquals(newWatchedIds) || !DictionaryEquals(item.WatchedEpisodeTimes, watchedStatusMap);
            item.WatchedEpisodeIds = newWatchedIds;
            item.WatchedEpisodeTimes = watchedStatusMap;
            item.WatchedStatusLoaded = true;

            if (changed)
            {
                item.CachedEpisodeRows = BuildEpisodeRows(item);
                if (dgvSeries.SelectedRows.Count > 0 && dgvSeries.SelectedRows[0].DataBoundItem is WatchlistItem selectedItem && selectedItem.Subject.Id == item.Subject.Id)
                {
                    dgvEpisodes.SuspendLayout();
                    dgvEpisodes.DataSource = null;
                    dgvEpisodes.DataSource = item.CachedEpisodeRows;
                    dgvEpisodes.ResumeLayout();
                    ShowStatus($"已刷新 {item.DisplayTitle} 的已看状态。当前显示已更新。");
                }
                else
                {
                    ShowStatus($"已加载 {item.DisplayTitle} 的已看状态。");
                }
            }
            else
            {
                ShowStatus($"已加载 {item.DisplayTitle} 的已看状态，无需刷新。\n");
            }

            SaveCache();
        }
        catch (Exception ex)
        {
            LogHttp($"Failed to refresh watched status for subject {item.Subject.Id}: {ex.Message}");
        }
    }

    private async Task PrefetchMissingEpisodesAsync()
    {
        var itemsToPrefetch = _watchlist
            .Where(item => !item.EpisodesLoaded || (!item.EpisodeAirdateSyncAttempted && item.Episodes.Any() && item.Episodes.All(ep => string.IsNullOrWhiteSpace(ep.Airdate))))
            .ToList();
        if (!itemsToPrefetch.Any())
        {
            return;
        }

        ShowStatus("后台预加载未缓存的番剧集数...");
        foreach (var item in itemsToPrefetch)
        {
            try
            {
                item.Episodes = (await FetchBangumiEpisodesAsync(item.Subject.Id)).ToList();
                item.EpisodeAirdateSyncAttempted = true;
                SaveCache();
            }
            catch
            {
                // 忽略单个番剧加载失败，用户选择时仍会重试
            }
        }

        ShowStatus("后台预加载完成。请点击番剧查看详细集数。");
    }

    private void dgvEpisodes_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
    {
        foreach (DataGridViewRow row in dgvEpisodes.Rows)
        {
            if (row.DataBoundItem is EpisodeRow episode)
            {
                if (episode.IsWatched)
                {
                    row.DefaultCellStyle.BackColor = Color.SteelBlue;
                    row.DefaultCellStyle.ForeColor = Color.White;
                }
                else if (!string.IsNullOrWhiteSpace(episode.Airdate) && DateTime.TryParseExact(episode.Airdate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var airDate))
                {
                    var today = DateTime.Today;
                    var tomorrow = today.AddDays(1);
                    if (airDate.Date == tomorrow)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                    }
                    else if (airDate.Date > tomorrow)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGray;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                    }
                    else
                    {
                        // today or past airdate都算已播
                        row.DefaultCellStyle.BackColor = Color.LightBlue;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                    }
                }
                else
                {
                    // 仅在确实无播出时间时显示白色
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }

                if (row.Cells[colEpisodePlay.Name] is DataGridViewCell playCell)
                {
                    playCell.Value = episode.CanPlay ? "播放" : "无本地";
                }

                if (row.Cells[colEpisodeMarkWatched.Name] is DataGridViewCell markCell)
                {
                    markCell.Value = episode.HasBangumiId ? (episode.IsWatched ? "撤销" : "标记已看") : "本地集";
                }
            }
        }
    }

    private async void dgvEpisodes_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        {
            return;
        }

        if (dgvEpisodes.Rows[e.RowIndex].DataBoundItem is not EpisodeRow episode)
        {
            return;
        }

        if (dgvEpisodes.Columns[e.ColumnIndex] == colEpisodePlay)
        {
            if (!episode.CanPlay)
            {
                ShowStatus("无本地文件，无法播放。");
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(episode.LocalFile) { UseShellExecute = true });
                ShowStatus($"打开本地文件：{Path.GetFileName(episode.LocalFile)}");
            }
            catch (Exception ex)
            {
                ShowStatus($"打开失败：{ex.Message}");
            }

            return;
        }

        if (dgvEpisodes.Columns[e.ColumnIndex] == colEpisodeMarkWatched)
        {
            if (episode.EpisodeId <= 0)
            {
                ShowStatus("此集仅为本地文件，未获取 Bangumi ID，无法操作。");
                return;
            }

            if (dgvSeries.SelectedRows.Count > 0 && dgvSeries.SelectedRows[0].DataBoundItem is WatchlistItem currentItem)
            {
                if (episode.IsWatched)
                {
                    var result = await UpdateEpisodeWatchedStatusAsync(currentItem.Subject.Id, episode.EpisodeId, 0);
                    if (result.Success)
                    {
                        currentItem.WatchedEpisodeIds.Remove(episode.EpisodeId);
                        currentItem.CachedEpisodeRows.Clear();
                        await LoadEpisodeRowsAsync(currentItem);
                        ShowStatus($"已撤销第 {episode.EpisodeNumber} 集的已看状态。");
                    }
                    else
                    {
                        ShowStatus($"撤销已看失败：{result.ErrorMessage}");
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(_config.AccessToken))
                    {
                        ShowStatus("请先配置 Bangumi Token 后再提交操作。");
                        return;
                    }

                    var result = await UpdateEpisodeWatchedStatusAsync(currentItem.Subject.Id, episode.EpisodeId, 2);
                    if (result.Success)
                    {
                        currentItem.WatchedEpisodeIds.Add(episode.EpisodeId);
                        currentItem.CachedEpisodeRows.Clear();
                        await LoadEpisodeRowsAsync(currentItem);
                        ShowStatus($"已标记第 {episode.EpisodeNumber} 集为已看。");
                    }
                    else
                    {
                        ShowStatus($"提交已看记录失败：{result.ErrorMessage}");
                    }
                }
            }
        }
    }

    private static int ExtractEpisodeNumber(string fileName, out string label)
    {
        label = fileName;
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var patterns = new[] { "- (\\d+)", "\\[(\\d{1,3})\\]", "E(\\d{1,3})" };
        foreach (var pattern in patterns)
        {
            var match = Regex.Match(baseName, pattern, RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var number))
            {
                label = match.Groups[1].Value;
                return number;
            }
        }

        return -1;
    }

    private static string NormalizeTitle(string title)
    {
        var normalized = title.Trim().ToLowerInvariant();
        normalized = normalized.Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("・", string.Empty)
            .Replace("：", string.Empty)
            .Replace(":", string.Empty)
            .Replace("\u3000", string.Empty);
        normalized = Regex.Replace(normalized, "[^0-9a-z\u4e00-\u9fa5]", string.Empty);
        return normalized;
    }

    private static string UnixTimestampToDisplay(long unixTimestamp)
    {
        if (unixTimestamp <= 0)
        {
            return string.Empty;
        }

        try
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool DictionaryEquals(Dictionary<int, long> left, Dictionary<int, long> right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left == null || right == null || left.Count != right.Count)
        {
            return false;
        }

        foreach (var pair in left)
        {
            if (!right.TryGetValue(pair.Key, out var value) || value != pair.Value)
            {
                return false;
            }
        }

        return true;
    }

    private async Task<(bool Success, string ErrorMessage)> MarkEpisodeAsWatchedAsync(int subjectId, int episodeId)
    {
        var token = txtAccessToken.Text.Trim();
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"users/-/collections/{subjectId}/episodes");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var payload = JsonSerializer.Serialize(new { episode_id = new[] { episodeId }, type = 2 });
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        using var response = await SendLoggedAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return (true, string.Empty);
        }

        var content = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(content) ? response.ReasonPhrase ?? "未知错误" : content);
    }

    private void ShowStatus(string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => toolStripStatusLabel.Text = message);
            return;
        }

        toolStripStatusLabel.Text = message;
    }

    private sealed class AppConfig
    {
        public string Username { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string LocalFolder { get; set; } = string.Empty;
    }

    private sealed class BangumiCache
    {
        public string Username { get; set; } = string.Empty;
        public List<WatchlistItem> WatchlistItems { get; set; } = new();
        public Dictionary<int, LocalFolderAssignment> LocalFolderAssignments { get; set; } = new();
    }

    private sealed class LocalFolderAssignment
    {
        public int SubjectId { get; set; }
        public string FolderName { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public List<LocalEpisode> LocalEpisodes { get; set; } = new();
    }

    private sealed class LocalSeries
    {
        public string FolderName { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public List<LocalEpisode> Episodes { get; set; } = new();
    }

    private sealed class WatchlistItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _localFolderName = string.Empty;
        private string _localFolderPath = string.Empty;
        private List<LocalEpisode> _localEpisodes = new();
        private List<BangumiEpisode> _episodes = new();
        private List<EpisodeRow> _cachedEpisodeRows = new();
        private HashSet<int> _watchedEpisodeIds = new();
        private Dictionary<int, long> _watchedEpisodeTimes = new();

        public BangumiSubject Subject { get; set; } = new BangumiSubject();
        public string LocalFolderName
        {
            get => _localFolderName;
            set
            {
                if (_localFolderName == value)
                {
                    return;
                }

                _localFolderName = value ?? string.Empty;
                OnPropertyChanged(nameof(LocalFolderName));
                OnPropertyChanged(nameof(LocalFolderDisplay));
            }
        }

        public string LocalFolderPath
        {
            get => _localFolderPath;
            set
            {
                if (_localFolderPath == value)
                {
                    return;
                }

                _localFolderPath = value ?? string.Empty;
                OnPropertyChanged(nameof(LocalFolderPath));
            }
        }

        public List<LocalEpisode> LocalEpisodes
        {
            get => _localEpisodes;
            set
            {
                if (_localEpisodes == value)
                {
                    return;
                }

                _localEpisodes = value ?? new List<LocalEpisode>();
                _cachedEpisodeRows.Clear();
                OnPropertyChanged(nameof(LocalEpisodes));
                OnPropertyChanged(nameof(HasLocal));
                OnPropertyChanged(nameof(LocalMatchCount));
            }
        }

        public List<BangumiEpisode> Episodes
        {
            get => _episodes;
            set
            {
                _episodes = value ?? new List<BangumiEpisode>();
                OnPropertyChanged(nameof(Episodes));
                OnPropertyChanged(nameof(EpisodesLoaded));
            }
        }

        [JsonIgnore]
        public List<EpisodeRow> CachedEpisodeRows
        {
            get => _cachedEpisodeRows;
            set => _cachedEpisodeRows = value ?? new List<EpisodeRow>();
        }

        public HashSet<int> WatchedEpisodeIds
        {
            get => _watchedEpisodeIds;
            set => _watchedEpisodeIds = value ?? new HashSet<int>();
        }

        public Dictionary<int, long> WatchedEpisodeTimes
        {
            get => _watchedEpisodeTimes;
            set => _watchedEpisodeTimes = value ?? new Dictionary<int, long>();
        }

        public bool EpisodeAirdateSyncAttempted { get; set; }

        public bool WatchedStatusLoaded { get; set; }
        public bool HasLocal => LocalEpisodes.Count > 0;
        public bool EpisodesLoaded => Episodes.Count > 0;
        public string DisplayTitle => string.IsNullOrWhiteSpace(Subject.NameCn) ? Subject.Name : Subject.NameCn;
        public int LocalMatchCount => LocalEpisodes.Count;
        public string LocalFolderDisplay => string.IsNullOrWhiteSpace(LocalFolderName) ? "未匹配" : LocalFolderName;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private sealed class LocalEpisode
    {
        public int EpisodeNumber { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    private sealed class EpisodeRow
    {
        public int EpisodeId { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string LocalFile { get; set; } = string.Empty;
        public string Airdate { get; set; } = string.Empty;
        public string WatchedAt { get; set; } = string.Empty;
        public bool CanPlay => !string.IsNullOrWhiteSpace(LocalFile);
        public bool HasBangumiId => EpisodeId > 0;
        public bool IsWatched { get; set; }
    }

    private sealed class BangumiSubject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameCn { get; set; } = string.Empty;
    }

    private sealed class BangumiEpisode
    {
        public int Id { get; set; }
        public int Sort { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Airdate { get; set; } = string.Empty;
    }
}
