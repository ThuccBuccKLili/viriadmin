#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;


static class AdminConfig
{
    public static string BASE_URL = "https://sjlblxrtdxkwttieqvkf.supabase.co/functions/v1/";
    public static string REST_URL = "https://sjlblxrtdxkwttieqvkf.supabase.co/rest/v1/";
    public static string ANON_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InNqbGJseHJ0ZHhrd3R0aWVxdmtmIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzI3NjA0NzcsImV4cCI6MjA4ODMzNjQ3N30.vF86o5j1kg-aQD-sewree25wqnZ-uT4t5lMgB7cxNdo";
    public static string AdminSecret = "njm4idCKQ4EiSi_";
}


static class AC
{
    public static readonly Color BG0 = Color.FromArgb(8, 8, 8);
    public static readonly Color BG1 = Color.FromArgb(14, 14, 14);
    public static readonly Color BG2 = Color.FromArgb(20, 20, 20);
    public static readonly Color BG3 = Color.FromArgb(28, 28, 28);
    public static readonly Color BDR = Color.FromArgb(40, 40, 40);
    public static readonly Color BDRH = Color.FromArgb(70, 70, 70);
    public static readonly Color RED = Color.FromArgb(210, 28, 28);
    public static readonly Color REDB = Color.FromArgb(255, 70, 70);
    public static readonly Color REDD = Color.FromArgb(90, 10, 10);
    public static readonly Color FG = Color.FromArgb(220, 220, 220);
    public static readonly Color FG2 = Color.FromArgb(130, 130, 130);
    public static readonly Color FG3 = Color.FromArgb(60, 60, 60);
    public static readonly Color GRN = Color.FromArgb(50, 200, 80);
    public static readonly Color ORG = Color.FromArgb(220, 140, 30);

    public static readonly Font CON8 = new Font("Consolas", 8f);
    public static readonly Font CON8B = new Font("Consolas", 8f, FontStyle.Bold);
    public static readonly Font CON9 = new Font("Consolas", 9f);
    public static readonly Font CON9B = new Font("Consolas", 9f, FontStyle.Bold);
    public static readonly Font CON10B = new Font("Consolas", 10f, FontStyle.Bold);
    public static readonly Font CON12B = new Font("Consolas", 12f, FontStyle.Bold);
    public static readonly Font SEG9 = new Font("Segoe UI", 9f);
}


static class UISettings
{
    public static Color AccentColor = Color.FromArgb(210, 28, 28);
    public static bool AlwaysOnTop = true;
    public static bool UseCustomFont = false;
    public static string FontFamily = "Consolas";
    public static int Transparency = 100;
    public static int PanelAlpha = 240;
    public static Image BackgroundImage = null;
    public static int BgOverlayAlpha = 180;

    public static Color AccentBright => Color.FromArgb(
        Math.Min(255, AccentColor.R + 50),
        Math.Min(255, AccentColor.G + 45),
        Math.Min(255, AccentColor.B + 45));
    public static Color AccentDark => Color.FromArgb(
        AccentColor.R / 3, AccentColor.G / 3, AccentColor.B / 3);
    public static Color AccentMid => Color.FromArgb(
        (int)(AccentColor.R * 0.68f),
        (int)(AccentColor.G * 0.68f),
        (int)(AccentColor.B * 0.68f));
}


class ScrollFreePanel : Panel
{
    [DllImport("user32.dll")] static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);
    void HideScrollBars() { if (!IsHandleCreated) return; try { ShowScrollBar(Handle, 0, false); ShowScrollBar(Handle, 1, false); } catch { } }
    protected override void OnLayout(LayoutEventArgs e) { base.OnLayout(e); HideScrollBars(); }
    protected override void OnScroll(ScrollEventArgs e) { base.OnScroll(e); HideScrollBars(); Invalidate(true); Update(); }
    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        if (m.Msg == 0x85 || m.Msg == 0x115 || m.Msg == 0x114 || m.Msg == 0x0005)
        {
            HideScrollBars();
            if (m.Msg == 0x115 || m.Msg == 0x114) { Invalidate(true); Update(); }
        }
    }
    public ScrollFreePanel()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        DoubleBuffered = true;
    }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        if (UISettings.BackgroundImage != null)
        {
            var form = FindForm();
            var ptForm = form != null ? form.PointToClient(PointToScreen(Point.Empty)) : Point.Empty;
            var img = UISettings.BackgroundImage;
            ImageAnimator.UpdateFrames(img);
            float sx = (float)ptForm.X / form.ClientSize.Width * img.Width;
            float sy = (float)ptForm.Y / form.ClientSize.Height * img.Height;
            float sw = (float)Width / form.ClientSize.Width * img.Width;
            float sh = (float)Height / form.ClientSize.Height * img.Height;
            e.Graphics.DrawImage(img, new RectangleF(0, 0, Width, Height),
                new RectangleF(sx, sy, sw, sh), GraphicsUnit.Pixel);
            using var ov = new SolidBrush(Color.FromArgb(UISettings.BgOverlayAlpha, 8, 8, 8));
            e.Graphics.FillRectangle(ov, ClientRectangle);
        }
        else
        {
            var bg = BackColor;
            e.Graphics.Clear(Color.FromArgb(255, bg.R, bg.G, bg.B));
        }
    }
}

class NoScrollPanel : Panel
{
    public readonly Panel Inner;
    int _scrollY;
    public NoScrollPanel()
    {
        AutoScroll = false; DoubleBuffered = true;
        Inner = new Panel { BackColor = AC.BG1 };
        Controls.Add(Inner);
    }
    public void Commit(int innerHeight)
    {
        _scrollY = 0;
        Inner.Size = new Size(Width - 2, innerHeight);
        Inner.Top = 0;
        Invalidate(true);
    }
    protected override CreateParams CreateParams { get { var cp = base.CreateParams; cp.Style &= ~0x00200000; cp.Style &= ~0x00100000; return cp; } }
    protected override void OnMouseWheel(MouseEventArgs e) { Scroll(e.Delta); ((HandledMouseEventArgs)e).Handled = true; }
    public new void Scroll(int delta)
    {
        int max = Math.Max(0, Inner.Height - Height + 4);
        _scrollY = Math.Max(0, Math.Min(max, _scrollY - delta));
        Inner.Top = -_scrollY;
        Invalidate(true);
    }
    protected override void OnControlAdded(ControlEventArgs e) { base.OnControlAdded(e); if (e.Control != Inner) return; HookWheel(Inner); }
    void HookWheel(Control? c)
    {
        if (c == null) return;
        c.MouseWheel += (s, e) => { Scroll(((MouseEventArgs)e).Delta); ((HandledMouseEventArgs)e).Handled = true; };
        c.ControlAdded += (s, e) => HookWheel(e.Control);
        foreach (Control ch in c.Controls) HookWheel(ch);
    }
}


static class DurationHelper
{
    public static long? ToSeconds(long years, long months, long days, long hours, long minutes, long seconds)
    {
        long total = seconds + minutes * 60 + hours * 3600 + days * 86400 + months * 2592000 + years * 31536000;
        return total <= 0 ? (long?)null : total;
    }
    public static string Describe(long? totalSeconds)
    {
        if (totalSeconds == null) return "lifetime";
        long s = totalSeconds.Value;
        var parts = new List<string>();
        long yr = s / 31536000; s %= 31536000;
        long mo = s / 2592000; s %= 2592000;
        long d = s / 86400; s %= 86400;
        long h = s / 3600; s %= 3600;
        long m = s / 60; s %= 60;
        if (yr > 0) parts.Add($"{yr} yr{(yr == 1 ? "" : "s")}");
        if (mo > 0) parts.Add($"{mo} mo");
        if (d > 0) parts.Add($"{d} day{(d == 1 ? "" : "s")}");
        if (h > 0) parts.Add($"{h} hr{(h == 1 ? "" : "s")}");
        if (m > 0) parts.Add($"{m} min");
        if (s > 0) parts.Add($"{s} sec");
        return parts.Count == 0 ? "0 sec" : string.Join("  ", parts);
    }
}


class KeyLogEntry { public string? Key, Username, Duration, Time; public bool Ok; }

class KeyRecord
{
    public string? Key, Username, Note, Hwid, ExpiresAt;
    public bool Expired;
}

class BanRecord
{
    public string? Key, Username, BanReason, BanExpiresAt, BannedAt;
    public bool IsLifetime;
}

class AnnouncementRecord
{
    public string? Id, SenderName, Subject, Topic, Message, Scope, CreatedAt;
    public string[]? TargetKeys;
}

// ── System control state — written by admin, polled by client ─────────────────
class SystemControlRecord
{
    public bool AllKeysPaused;
    public string? AllPausedAt;          // ISO timestamp when global pause started
    public bool WarningEnabled;
    public string WarningMessage = "";
    public string[] PausedKeys = Array.Empty<string>();
    public string[] LockedKeys = Array.Empty<string>();
    public Dictionary<string, string> KeyPausedAt = new(); // key → ISO timestamp
}


static class AdminApi
{
    static readonly System.Net.Http.HttpClient Http = new System.Net.Http.HttpClient();

    static string Post(string endpoint, string json)
    {
        var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, AdminConfig.BASE_URL + endpoint);
        req.Headers.Add("Authorization", "Bearer " + AdminConfig.ANON_KEY);
        req.Content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return Http.SendAsync(req).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
    }

    static string Get(string path)
    {
        var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, AdminConfig.REST_URL + path);
        req.Headers.Add("Authorization", "Bearer " + AdminConfig.ANON_KEY);
        req.Headers.Add("apikey", AdminConfig.ANON_KEY);
        return Http.SendAsync(req).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
    }


    public static (bool ok, string key, string error) CreateKey(string username, long? totalSeconds, string note, string? manualKey = null)
    {
        string secsStr = totalSeconds.HasValue ? totalSeconds.Value.ToString() : "null";
        string keyPart = string.IsNullOrEmpty(manualKey) ? "" : ",\"manual_key\":\"" + Esc(manualKey) + "\"";
        string json = "{\"admin_secret\":\"" + Esc(AdminConfig.AdminSecret) + "\",\"username\":\"" + Esc(username) + "\",\"seconds\":" + secsStr + ",\"note\":\"" + Esc(note) + "\"" + keyPart + "}";
        string resp = Post("create-key", json);
        bool ok = resp.Contains("\"ok\":true");
        return (ok, ParseJson(resp, "key") ?? "", ok ? "" : ParseJson(resp, "reason") ?? resp);
    }

    public static (bool ok, string error) RevokeKey(string key, bool revoked)
    {
        string json = "{\"admin_secret\":\"" + Esc(AdminConfig.AdminSecret) + "\",\"key\":\"" + key.Trim().ToUpper() + "\",\"revoked\":" + revoked.ToString().ToLower() + "}";
        string resp = Post("revoke-key", json);
        bool ok = resp.Contains("\"ok\":true");
        return (ok, ok ? "" : ParseJson(resp, "reason") ?? resp);
    }

    public static (bool ok, string error) ResetHwid(string key)
    {
        string json = "{\"admin_secret\":\"" + Esc(AdminConfig.AdminSecret) + "\",\"key\":\"" + key.Trim().ToUpper() + "\"}";
        string resp = Post("reset-hwid", json);
        bool ok = resp.Contains("\"ok\":true");
        return (ok, ok ? "" : ParseJson(resp, "reason") ?? resp);
    }

    public static (bool valid, string username, string reason) CheckKey(string key)
    {
        string json = "{\"key\":\"" + key.Trim().ToUpper() + "\",\"hwid\":\"ADMIN-CHECK-ONLY\"}";
        string resp = Post("validate", json);
        bool valid = resp.Contains("\"valid\":true");
        string? expiry = ParseJson(resp, "expires_at");
        string reason = ParseJson(resp, "reason") ?? "";
        if (!string.IsNullOrEmpty(expiry) && DateTime.TryParse(expiry, out var exp))
            reason = "expires " + exp.ToString("yyyy-MM-dd HH:mm");
        else if (valid) reason = "lifetime";
        return (valid, ParseJson(resp, "username") ?? "", reason);
    }

    public static (string hwid, string username) FetchHwidAndUser(string key)
    {
        try
        {
            string json = "{\"admin_secret\":\"" + Esc(AdminConfig.AdminSecret) + "\",\"key\":\"" + key.Trim().ToUpper() + "\"}";
            string resp = Post("get-key-info", json);
            bool ok = resp.Contains("\"ok\":true");
            if (!ok) return ("error: " + (ParseJson(resp, "reason") ?? "unknown"), "—");
            string? hwid = ParseJson(resp, "hwid");
            string? user = ParseJson(resp, "username");
            return (string.IsNullOrEmpty(hwid) ? "not set" : hwid, string.IsNullOrEmpty(user) ? "—" : user);
        }
        catch (Exception ex) { return ("error: " + ex.Message, "—"); }
    }

    public static (bool ok, List<KeyRecord> keys, string error) GetActiveKeys()
    {
        try
        {
            string json = "{\"admin_secret\":\"" + Esc(AdminConfig.AdminSecret) + "\"}";
            string resp = Post("get-all-keys", json);
            bool ok = resp.Contains("\"ok\":true");
            if (!ok) return (false, new List<KeyRecord>(), ParseJson(resp, "reason") ?? resp);
            int arrStart = resp.IndexOf("\"keys\":");
            if (arrStart < 0) return (false, new List<KeyRecord>(), "no keys array in response");
            arrStart += 7;
            while (arrStart < resp.Length && resp[arrStart] != '[') arrStart++;
            if (arrStart >= resp.Length) return (false, new List<KeyRecord>(), "malformed response");
            int depth = 0, arrEnd = arrStart;
            for (int i = arrStart; i < resp.Length; i++)
            {
                if (resp[i] == '[' || resp[i] == '{') depth++;
                else if (resp[i] == ']' || resp[i] == '}') { depth--; if (depth == 0) { arrEnd = i; break; } }
            }
            string arr = resp.Substring(arrStart, arrEnd - arrStart + 1);
            return (true, ParseKeyList(arr), "");
        }
        catch (Exception ex) { return (false, new List<KeyRecord>(), ex.Message); }
    }


    public static (bool ok, string error) BanKey(string key, string reason, long? durationSeconds, string notes)
    {
        string secsStr = durationSeconds.HasValue ? durationSeconds.Value.ToString() : "null";
        string json = "{\"admin_secret\":\"" + Esc(AdminConfig.AdminSecret)
                    + "\",\"key\":\"" + key.Trim().ToUpper()
                    + "\",\"reason\":\"" + Esc(reason)
                    + "\",\"seconds\":" + secsStr
                    + ",\"notes\":\"" + Esc(notes ?? "") + "\"}";
        string resp = Post("ban-key", json);
        bool ok = resp.Contains("\"ok\":true");
        return (ok, ok ? "" : ParseJson(resp, "reason") ?? resp);
    }

    public static (bool ok, string error) UnbanKey(string key)
    {
        string json = "{\"admin_secret\":\"" + Esc(AdminConfig.AdminSecret) + "\",\"key\":\"" + key.Trim().ToUpper() + "\"}";
        string resp = Post("unban-key", json);
        bool ok = resp.Contains("\"ok\":true");
        return (ok, ok ? "" : ParseJson(resp, "reason") ?? resp);
    }

    // ── Announcements ─────────────────────────────────────────────────────────

    public static (bool ok, string id, string error) SendAnnouncement(AnnouncementRecord ann)
    {
        string keysJson = "null";
        if (ann.TargetKeys != null && ann.TargetKeys.Length > 0)
        {
            var escaped = string.Join(",", ann.TargetKeys.Select(k => "\"" + Esc(k.Trim().ToUpper()) + "\""));
            keysJson = "[" + escaped + "]";
        }
        try
        {
            var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post,
                AdminConfig.REST_URL + "announcements");
            req.Headers.Add("Authorization", "Bearer " + AdminConfig.ANON_KEY);
            req.Headers.Add("apikey", AdminConfig.ANON_KEY);
            req.Headers.Add("Prefer", "return=representation");
            string body = "{\"sender_name\":\"" + Esc(ann.SenderName)
                + "\",\"subject\":\"" + Esc(ann.Subject)
                + "\",\"topic\":\"" + Esc(ann.Topic)
                + "\",\"message\":\"" + Esc(ann.Message)
                + "\",\"scope\":\"" + Esc(ann.Scope)
                + "\",\"target_keys\":" + keysJson + "}";
            req.Content = new System.Net.Http.StringContent(body, System.Text.Encoding.UTF8, "application/json");
            string resp = Http.SendAsync(req).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();

            // Supabase returns an array [...] with return=representation — unwrap it
            string parsed = resp.Trim();
            if (parsed.StartsWith("[")) parsed = parsed.TrimStart('[').TrimEnd(']').Trim();
            string? id = ParseJson(parsed, "id");
            bool ok = !string.IsNullOrEmpty(id);
            return (ok, id ?? "", ok ? "" : "no id returned — check RLS or table name. raw: " + resp.Substring(0, Math.Min(200, resp.Length)));
        }
        catch (Exception ex) { return (false, "", ex.Message); }
    }

    public static (bool ok, List<AnnouncementRecord> list, string error) GetAnnouncements()
    {
        try
        {
            string resp = Get("announcements?select=id,sender_name,subject,topic,message,scope,target_keys,created_at&order=created_at.desc&limit=100");
            return (true, ParseAnnouncementList(resp), "");
        }
        catch (Exception ex) { return (false, new List<AnnouncementRecord>(), ex.Message); }
    }

    public static (bool ok, string error) UpdateAnnouncement(AnnouncementRecord ann)
    {
        try
        {
            string keysJson = "null";
            if (ann.TargetKeys != null && ann.TargetKeys.Length > 0)
            {
                var escaped = string.Join(",", ann.TargetKeys.Select(k => "\"" + Esc(k.Trim().ToUpper()) + "\""));
                keysJson = "[" + escaped + "]";
            }
            var req = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("PATCH"),
                AdminConfig.REST_URL + "announcements?id=eq." + ann.Id);
            req.Headers.Add("Authorization", "Bearer " + AdminConfig.ANON_KEY);
            req.Headers.Add("apikey", AdminConfig.ANON_KEY);
            string body = "{\"sender_name\":\"" + Esc(ann.SenderName)
                + "\",\"subject\":\"" + Esc(ann.Subject)
                + "\",\"topic\":\"" + Esc(ann.Topic)
                + "\",\"message\":\"" + Esc(ann.Message)
                + "\",\"scope\":\"" + Esc(ann.Scope)
                + "\",\"target_keys\":" + keysJson + "}";
            req.Content = new System.Net.Http.StringContent(body, System.Text.Encoding.UTF8, "application/json");
            var res = Http.SendAsync(req).GetAwaiter().GetResult();
            return (res.IsSuccessStatusCode, res.IsSuccessStatusCode ? "" : res.StatusCode.ToString());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public static (bool ok, string error) DeleteAnnouncement(string id)
    {
        try
        {
            var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Delete,
                AdminConfig.REST_URL + "announcements?id=eq." + id);
            req.Headers.Add("Authorization", "Bearer " + AdminConfig.ANON_KEY);
            req.Headers.Add("apikey", AdminConfig.ANON_KEY);
            var res = Http.SendAsync(req).GetAwaiter().GetResult();
            return (res.IsSuccessStatusCode, res.IsSuccessStatusCode ? "" : res.StatusCode.ToString());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    // ── System Control ────────────────────────────────────────────────────────

    public static (bool ok, SystemControlRecord ctrl, string error) GetSystemControl()
    {
        try
        {
            string resp = Get("system_control?select=all_keys_paused,all_paused_at,warning_enabled,warning_message,paused_keys,locked_keys,key_paused_at&limit=1");
            var list = ParseSystemControlList(resp);
            return (true, list.Count > 0 ? list[0] : new SystemControlRecord(), "");
        }
        catch (Exception ex) { return (false, new SystemControlRecord(), ex.Message); }
    }

    public static (bool ok, string error) UpdateSystemControl(SystemControlRecord ctrl)
    {
        try
        {
            string pkArr = ctrl.PausedKeys?.Length > 0
                ? "[" + string.Join(",", ctrl.PausedKeys.Select(k => "\"" + Esc(k.Trim().ToUpper()) + "\"")) + "]"
                : "[]";
            string lkArr = ctrl.LockedKeys?.Length > 0
                ? "[" + string.Join(",", ctrl.LockedKeys.Select(k => "\"" + Esc(k.Trim().ToUpper()) + "\"")) + "]"
                : "[]";

            // Serialize key_paused_at as JSON object
            string kpaJson = "{}";
            if (ctrl.KeyPausedAt != null && ctrl.KeyPausedAt.Count > 0)
                kpaJson = "{" + string.Join(",", ctrl.KeyPausedAt.Select(kvp =>
                    "\"" + Esc(kvp.Key) + "\":\"" + Esc(kvp.Value) + "\"")) + "}";

            string allPausedAtJson = string.IsNullOrEmpty(ctrl.AllPausedAt) ? "null" : "\"" + Esc(ctrl.AllPausedAt) + "\"";

            string body = "{\"all_keys_paused\":" + ctrl.AllKeysPaused.ToString().ToLower()
                + ",\"all_paused_at\":" + allPausedAtJson
                + ",\"warning_enabled\":" + ctrl.WarningEnabled.ToString().ToLower()
                + ",\"warning_message\":\"" + Esc(ctrl.WarningMessage ?? "") + "\""
                + ",\"paused_keys\":" + pkArr
                + ",\"locked_keys\":" + lkArr
                + ",\"key_paused_at\":" + kpaJson + "}";
            var req = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("PATCH"),
                AdminConfig.REST_URL + "system_control?id=eq.1");
            req.Headers.Add("Authorization", "Bearer " + AdminConfig.ANON_KEY);
            req.Headers.Add("apikey", AdminConfig.ANON_KEY);
            req.Content = new System.Net.Http.StringContent(body, System.Text.Encoding.UTF8, "application/json");
            var res = Http.SendAsync(req).GetAwaiter().GetResult();
            return (res.IsSuccessStatusCode, res.IsSuccessStatusCode ? "" : res.StatusCode.ToString());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    // Extend a key's expires_at by the given number of seconds (adds time to existing expiry)
    public static (bool ok, string error) ExtendExpiry(string key, long extraSeconds)
    {
        try
        {
            // Fetch current expires_at
            string resp = Get("licenses?select=expires_at&key=eq." + Uri.EscapeDataString(key) + "&limit=1");
            resp = (resp ?? "").Trim().TrimStart('[').TrimEnd(']').Trim();
            if (string.IsNullOrEmpty(resp) || resp == "null") return (false, "key not found");

            string? expiresAt = ParseJson(resp, "expires_at");
            if (string.IsNullOrEmpty(expiresAt) || expiresAt == "null")
                return (true, "lifetime key, no extension needed"); // lifetime keys don't expire

            if (!DateTime.TryParse(expiresAt, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime current))
                return (false, "could not parse expires_at: " + expiresAt);

            DateTime newExpiry = current.AddSeconds(extraSeconds);
            string newExpiryStr = newExpiry.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

            var req = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("PATCH"),
                AdminConfig.REST_URL + "licenses?key=eq." + Uri.EscapeDataString(key));
            req.Headers.Add("Authorization", "Bearer " + AdminConfig.ANON_KEY);
            req.Headers.Add("apikey", AdminConfig.ANON_KEY);
            req.Content = new System.Net.Http.StringContent(
                "{\"expires_at\":\"" + newExpiryStr + "\"}",
                System.Text.Encoding.UTF8, "application/json");
            var res = Http.SendAsync(req).GetAwaiter().GetResult();
            return (res.IsSuccessStatusCode, res.IsSuccessStatusCode ? "" : res.StatusCode.ToString());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    static List<SystemControlRecord> ParseSystemControlList(string json)
    {
        var list = new List<SystemControlRecord>();
        json = (json ?? "").Trim();
        if (!json.StartsWith("[")) return list;
        int depth = 0, start = -1;
        for (int i = 0; i < json.Length; i++)
        {
            if (json[i] == '{') { if (depth == 0) start = i; depth++; }
            else if (json[i] == '}') { depth--; if (depth == 0 && start >= 0) { list.Add(ParseSystemControlRecord(json.Substring(start, i - start + 1))); start = -1; } }
        }
        return list;
    }

    static SystemControlRecord ParseSystemControlRecord(string obj)
    {
        var rec = new SystemControlRecord
        {
            AllKeysPaused = ParseBool(obj, "all_keys_paused"),
            AllPausedAt = ParseJson(obj, "all_paused_at"),
            WarningEnabled = ParseBool(obj, "warning_enabled"),
            WarningMessage = ParseJson(obj, "warning_message") ?? "",
            PausedKeys = ParseStringArray(obj, "paused_keys"),
            LockedKeys = ParseStringArray(obj, "locked_keys")
        };

        // Parse key_paused_at JSON object: {"KEY": "timestamp", ...}
        int kpaIdx = obj.IndexOf("\"key_paused_at\":");
        if (kpaIdx >= 0)
        {
            int objStart = obj.IndexOf('{', kpaIdx + 16);
            if (objStart >= 0)
            {
                int objEnd = obj.IndexOf('}', objStart);
                if (objEnd > objStart)
                {
                    string inner = obj.Substring(objStart + 1, objEnd - objStart - 1).Trim();
                    if (!string.IsNullOrEmpty(inner))
                    {
                        foreach (var part in inner.Split(','))
                        {
                            var colon = part.IndexOf(':');
                            if (colon < 0) continue;
                            string k = part.Substring(0, colon).Trim().Trim('"');
                            string v = part.Substring(colon + 1).Trim().Trim('"');
                            if (k.Length > 0 && v.Length > 0)
                                rec.KeyPausedAt[k] = v;
                        }
                    }
                }
            }
        }
        return rec;
    }

    static bool ParseBool(string json, string key)
    {
        string search = "\"" + key + "\":";
        int i = json.IndexOf(search);
        if (i < 0) return false;
        i += search.Length;
        while (i < json.Length && json[i] == ' ') i++;
        return i < json.Length && json[i] == 't';
    }

    static string[] ParseStringArray(string json, string key)
    {
        int ki = json.IndexOf("\"" + key + "\":");
        if (ki < 0) return Array.Empty<string>();
        int arrStart = json.IndexOf('[', ki);
        int nullIdx = json.IndexOf("null", ki);
        if (arrStart < 0 || (nullIdx >= 0 && nullIdx < arrStart)) return Array.Empty<string>();
        int arrEnd = json.IndexOf(']', arrStart);
        if (arrEnd <= arrStart) return Array.Empty<string>();
        string inner = json.Substring(arrStart + 1, arrEnd - arrStart - 1).Trim();
        if (string.IsNullOrEmpty(inner)) return Array.Empty<string>();
        return inner.Split(',').Select(s => s.Trim().Trim('"')).Where(s => s.Length > 0).ToArray();
    }

    // ── Parsers ────────────────────────────────────────────────────────────────

    static List<AnnouncementRecord> ParseAnnouncementList(string json)
    {
        var list = new List<AnnouncementRecord>(); json = json.Trim();
        if (!json.StartsWith("[")) return list;
        int depth = 0, start = -1;
        for (int i = 0; i < json.Length; i++)
        {
            if (json[i] == '{') { if (depth == 0) start = i; depth++; }
            else if (json[i] == '}') { depth--; if (depth == 0 && start >= 0) { list.Add(ParseAnnouncementRecord(json.Substring(start, i - start + 1))); start = -1; } }
        }
        return list;
    }

    static AnnouncementRecord ParseAnnouncementRecord(string obj)
    {
        string[] keys = null;
        int ki = obj.IndexOf("\"target_keys\":");
        if (ki >= 0)
        {
            int arrStart = obj.IndexOf('[', ki);
            int arrEnd = obj.IndexOf(']', ki);
            if (arrStart >= 0 && arrEnd > arrStart)
            {
                string arrContent = obj.Substring(arrStart + 1, arrEnd - arrStart - 1).Trim();
                if (!string.IsNullOrEmpty(arrContent))
                    keys = arrContent.Split(',').Select(s => s.Trim().Trim('"')).Where(s => s.Length > 0).ToArray();
            }
        }
        return new AnnouncementRecord
        {
            Id = ParseJson(obj, "id") ?? "",
            SenderName = ParseJson(obj, "sender_name") ?? "",
            Subject = ParseJson(obj, "subject") ?? "",
            Topic = ParseJson(obj, "topic") ?? "",
            Message = ParseJson(obj, "message") ?? "",
            Scope = ParseJson(obj, "scope") ?? "global",
            CreatedAt = ParseJson(obj, "created_at") ?? "",
            TargetKeys = keys
        };
    }

    public static (bool ok, string error) AdjustKeyDuration(string key, long deltaSeconds)
    {
        string json = "{\"admin_secret\":\"" + Esc(AdminConfig.AdminSecret)
                    + "\",\"key\":\"" + key.Trim().ToUpper()
                    + "\",\"delta_seconds\":" + deltaSeconds + "}";
        string resp = Post("adjust-key-duration", json);
        bool ok = resp.Contains("\"ok\":true");
        return (ok, ok ? "" : ParseJson(resp, "reason") ?? resp);
    }

    public static (bool ok, List<BanRecord> bans, string error) GetBannedKeys()
    {
        try
        {
            string resp = Get("licenses?select=key,username,ban_reason,ban_expires_at,banned_at&banned=eq.true&order=banned_at.desc");
            return (true, ParseBanList(resp), "");
        }
        catch (Exception ex) { return (false, new List<BanRecord>(), ex.Message); }
    }


    static List<KeyRecord> ParseKeyList(string json)
    {
        var list = new List<KeyRecord>(); json = json.Trim();
        if (!json.StartsWith("[")) return list;
        int depth = 0, start = -1;
        for (int i = 0; i < json.Length; i++)
        {
            if (json[i] == '{') { if (depth == 0) start = i; depth++; }
            else if (json[i] == '}') { depth--; if (depth == 0 && start >= 0) { list.Add(ParseKeyRecord(json.Substring(start, i - start + 1))); start = -1; } }
        }
        return list;
    }

    static KeyRecord ParseKeyRecord(string obj)
    {
        string? expiry = ParseJson(obj, "expires_at");
        bool expired = !string.IsNullOrEmpty(expiry) && DateTime.TryParse(expiry, out var exp) && exp < DateTime.UtcNow;
        return new KeyRecord { Key = ParseJson(obj, "key") ?? "", Username = ParseJson(obj, "username") ?? "", Note = ParseJson(obj, "note") ?? "", Hwid = ParseJson(obj, "hwid") ?? "", ExpiresAt = expiry ?? "", Expired = expired };
    }

    static List<BanRecord> ParseBanList(string json)
    {
        var list = new List<BanRecord>(); json = json.Trim();
        if (!json.StartsWith("[")) return list;
        int depth = 0, start = -1;
        for (int i = 0; i < json.Length; i++)
        {
            if (json[i] == '{') { if (depth == 0) start = i; depth++; }
            else if (json[i] == '}') { depth--; if (depth == 0 && start >= 0) { list.Add(ParseBanRecord(json.Substring(start, i - start + 1))); start = -1; } }
        }
        return list;
    }

    static BanRecord ParseBanRecord(string obj)
    {
        string? banExp = ParseJson(obj, "ban_expires_at");
        return new BanRecord { Key = ParseJson(obj, "key") ?? "", Username = ParseJson(obj, "username") ?? "", BanReason = ParseJson(obj, "ban_reason") ?? "", BanExpiresAt = banExp ?? "", BannedAt = ParseJson(obj, "banned_at") ?? "", IsLifetime = string.IsNullOrEmpty(banExp) };
    }

    static string Esc(string? s) => (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");

    public static string? ParseJson(string json, string key)
    {
        string search = "\"" + key + "\":\"";
        int i = json.IndexOf(search); if (i < 0) return null;
        i += search.Length; int j = json.IndexOf('"', i); return j < 0 ? null : json.Substring(i, j - i);
    }
}


class ACheck : Control
{
    bool _checked, _hover;
    public event EventHandler? CheckedChanged;
    public bool Checked { get => _checked; set { if (_checked == value) return; _checked = value; Invalidate(); CheckedChanged?.Invoke(this, EventArgs.Empty); } }
    public ACheck(string text)
    {
        Text = text; Font = AC.SEG9; Cursor = Cursors.Hand;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        MouseEnter += (s, e) => { _hover = true; Invalidate(); };
        MouseLeave += (s, e) => { _hover = false; Invalidate(); };
        Click += (s, e) => Checked = !_checked;
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        var parentSolid = Color.FromArgb(255, Parent?.BackColor.R ?? AC.BG1.R, Parent?.BackColor.G ?? AC.BG1.G, Parent?.BackColor.B ?? AC.BG1.B);
        g.Clear(parentSolid);
        int by = (Height - 13) / 2; var box = new Rectangle(1, by, 13, 13);
        g.FillRectangle(new SolidBrush(AC.BG0), box);
        Color ac = UISettings.AccentColor;
        if (_checked)
        {
            using (var fb = new SolidBrush(Color.FromArgb(50, ac.R, ac.G, ac.B))) g.FillRectangle(fb, box);
            using (var rp = new Pen(ac)) g.DrawRectangle(rp, box);
            using (var cp = new Pen(UISettings.AccentBright, 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLines(cp, new PointF[] { new PointF(box.X + 2.5f, box.Y + 6.5f), new PointF(box.X + 5.5f, box.Y + 9.5f), new PointF(box.X + 10.5f, box.Y + 3.5f) });
        }
        else { using (var bp = new Pen(_hover ? ac : AC.BDRH)) g.DrawRectangle(bp, box); }
        TextRenderer.DrawText(g, Text, Font, new Rectangle(21, 0, Width - 21, Height), _checked ? AC.FG : _hover ? AC.FG2 : AC.FG3, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.PreserveGraphicsClipping);
    }
}

class ABox : TextBox
{
    readonly string _hint;
    public ABox(string hint, bool password = false)
    {
        _hint = hint; Text = hint; ForeColor = AC.FG3; BackColor = AC.BG1;
        BorderStyle = BorderStyle.FixedSingle; Font = AC.CON9;
        if (password) PasswordChar = '\u25CF';
        GotFocus += (s, e) => { if (Text == _hint) { Text = ""; ForeColor = AC.FG; } BackColor = Color.FromArgb(16, 14, 14); };
        LostFocus += (s, e) => { if (Text == "") { Text = _hint; ForeColor = AC.FG3; } BackColor = AC.BG1; };
    }
    public string Value => Text == _hint ? "" : Text;
    public void SetValue(string v) { Text = string.IsNullOrEmpty(v) ? _hint : v; ForeColor = string.IsNullOrEmpty(v) ? AC.FG3 : AC.FG; }
    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        if (m.Msg == 0xF) { try { using var g = Graphics.FromHwnd(Handle); using var p = new Pen(Focused ? UISettings.AccentColor : AC.BDR); g.DrawRectangle(p, 0, 0, Width - 1, Height - 1); } catch { } }
    }
}

class NumBox : TextBox
{
    public NumBox()
    {
        Text = "0"; ForeColor = AC.FG3; BackColor = AC.BG1;
        BorderStyle = BorderStyle.FixedSingle; Font = AC.CON9; TextAlign = HorizontalAlignment.Center;
        GotFocus += (s, e) => { if (Text == "0") { Text = ""; ForeColor = AC.FG; } BackColor = Color.FromArgb(16, 14, 14); };
        LostFocus += (s, e) => { if (!long.TryParse(Text, out var v) || v < 0) { Text = "0"; ForeColor = AC.FG3; } else ForeColor = v == 0 ? AC.FG3 : AC.FG; BackColor = AC.BG1; };
        KeyPress += (s, e) => { if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b') e.Handled = true; };
    }
    public long Val { get => long.TryParse(Text, out var v) && v >= 0 ? v : 0; set { Text = value.ToString(); ForeColor = value == 0 ? AC.FG3 : AC.FG; } }
    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        if (m.Msg == 0xF) { try { using var g = Graphics.FromHwnd(Handle); using var p = new Pen(Focused ? UISettings.AccentColor : AC.BDR); g.DrawRectangle(p, 0, 0, Width - 1, Height - 1); } catch { } }
    }
}

class ABtn : Button
{
    bool _hov, _dn; readonly bool _accent;
    public ABtn(string txt, bool accent = false)
    {
        _accent = accent; Text = txt; FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0;
        Font = AC.CON9B; Cursor = Cursors.Hand; ForeColor = AC.FG;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
        MouseEnter += (s, e) => { _hov = true; Invalidate(); };
        MouseLeave += (s, e) => { _hov = false; Invalidate(); };
        MouseDown += (s, e) => { _dn = true; Invalidate(); };
        MouseUp += (s, e) => { _dn = false; Invalidate(); };
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        Color ac = UISettings.AccentColor;
        Color bg = _accent ? (_dn ? ac : _hov ? UISettings.AccentMid : UISettings.AccentDark) : (_dn ? AC.BG3 : _hov ? AC.BG2 : AC.BG1);
        g.FillRectangle(new SolidBrush(bg), ClientRectangle);
        using (var p = new Pen(_accent ? Color.FromArgb(_hov ? 220 : 120, ac.R, ac.G, ac.B) : (_hov ? AC.BDRH : AC.BDR)))
            g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
        TextRenderer.DrawText(g, Text, Font, ClientRectangle, Enabled ? AC.FG : AC.FG3, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping);
    }
}

class CloseBtn : Control
{
    bool _hov;
    public CloseBtn()
    {
        Cursor = Cursors.Hand; Size = new Size(38, 38);
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
        MouseEnter += (s, e) => { _hov = true; Invalidate(); };
        MouseLeave += (s, e) => { _hov = false; Invalidate(); };
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.Clear(Parent?.BackColor ?? AC.BG1);
        using var f = new Font("Segoe UI Symbol", 10f);
        TextRenderer.DrawText(g, "✕", f, ClientRectangle, _hov ? UISettings.AccentBright : UISettings.AccentColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

class ATabBtn : Control
{
    bool _hov;
    public bool Active { set { _active = value; Invalidate(); } get => _active; }
    bool _active;
    public ATabBtn(string text)
    {
        Text = text; Font = AC.CON9B; Cursor = Cursors.Hand;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        MouseEnter += (s, e) => { _hov = true; Invalidate(); };
        MouseLeave += (s, e) => { _hov = false; Invalidate(); };
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.Clear(Parent?.BackColor ?? AC.BG1);
        Color fg = _active ? AC.FG : _hov ? AC.FG2 : AC.FG3;
        TextRenderer.DrawText(g, Text, Font, ClientRectangle, fg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        if (_active) { using var rp = new Pen(UISettings.AccentColor, 2); g.DrawLine(rp, 6, Height - 2, Width - 6, Height - 2); }
    }
}


class AHeader : Control
{
    readonly string _title;
    public AHeader(string title)
    {
        _title = title;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.DoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        TabStop = false;
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        var bg = Parent?.BackColor ?? AC.BG1;
        using (var br = new SolidBrush(bg)) g.FillRectangle(br, ClientRectangle);
        int tw = (int)g.MeasureString(_title, AC.CON8B).Width + 6;
        g.DrawString(_title, AC.CON8B, new SolidBrush(UISettings.AccentColor), 0, 2);
        using var p = new Pen(AC.BDR);
        g.DrawLine(p, tw, Height / 2, Width, Height / 2);
    }
}


class AGlowSlider : Control
{
    int _value, _min, _max;
    bool _drag;
    public event EventHandler? ValueChanged;
    public int Value { get => _value; set { _value = Math.Max(_min, Math.Min(_max, value)); Invalidate(); ValueChanged?.Invoke(this, EventArgs.Empty); } }
    public int Minimum { get => _min; set { _min = value; Invalidate(); } }
    public int Maximum { get => _max; set { _max = value; Invalidate(); } }

    public AGlowSlider(int min = 0, int max = 100, int val = 0)
    {
        _min = min; _max = max; _value = val;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
        Cursor = Cursors.Hand;
        MouseDown += (s, e) => { _drag = true; SetFromX(e.X); };
        MouseMove += (s, e) => { if (_drag) SetFromX(e.X); };
        MouseUp += (s, e) => _drag = false;
        Capture = false;
    }

    void SetFromX(int x)
    {
        float r = Math.Max(0f, Math.Min(1f, (float)(x - 2) / Math.Max(1, Width - 4)));
        Value = _min + (int)(r * (_max - _min));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.None;
        g.FillRectangle(new SolidBrush(AC.BG0), ClientRectangle);
        float ratio = _max == _min ? 0 : (float)(_value - _min) / (_max - _min);
        int fillW = (int)((Width - 4) * ratio);
        if (fillW > 0)
        {
            Color ac = UISettings.AccentColor;
            g.FillRectangle(new SolidBrush(Color.FromArgb(35, ac.R, ac.G, ac.B)), new Rectangle(2, 2, fillW, Height - 4));
            g.FillRectangle(new SolidBrush(Color.FromArgb(160, ac.R, ac.G, ac.B)), new Rectangle(2, Height - 5, fillW, 3));
        }
        using (var p = new Pen(AC.BDR)) g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
        TextRenderer.DrawText(g, _value.ToString(), AC.CON8, ClientRectangle, AC.FG2, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

class AColorSwatch : Control
{
    Color _col; bool _hov; readonly bool _showPicker;
    public event EventHandler? ColorChanged;
    public Color SelectedColor { get => _col; set { _col = value; Invalidate(); } }
    public AColorSwatch(Color initial, bool showPicker = true)
    {
        _col = initial; _showPicker = showPicker;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
        Cursor = Cursors.Hand;
        MouseEnter += (s, e) => { _hov = true; Invalidate(); };
        MouseLeave += (s, e) => { _hov = false; Invalidate(); };
        Click += (s, e) =>
        {
            if (!_showPicker) { ColorChanged?.Invoke(this, EventArgs.Empty); return; }
            using var dlg = new ColorDialog { Color = _col, FullOpen = true };
            if (dlg.ShowDialog() == DialogResult.OK) { SelectedColor = dlg.Color; ColorChanged?.Invoke(this, EventArgs.Empty); }
        };
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.FillRectangle(new SolidBrush(_col), ClientRectangle);
        using var p = new Pen(_hov ? Color.White : AC.BDRH, _hov ? 2f : 1f);
        g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
        if (_hov && _showPicker) { g.SmoothingMode = SmoothingMode.AntiAlias; TextRenderer.DrawText(g, "✎", new Font("Segoe UI Symbol", 9f), ClientRectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter); }
    }
}


class BanDialog : Form
{
    public string? BanReason { get; private set; }
    public long? BanSeconds { get; private set; }
    public string? BanNotes { get; private set; }
    public bool Confirmed { get; private set; }

    NumBox? _dYears, _dMonths, _dDays, _dHours, _dMins, _dSecs;
    Label? _durPreview;
    ACheck? _lifetimeChk;

    public BanDialog(string key, string username)
    {
        FormBorderStyle = FormBorderStyle.None;
        BackColor = AC.BG0;
        ClientSize = new Size(560, 310);
        StartPosition = FormStartPosition.CenterParent;
        TopMost = true; DoubleBuffered = true;
        Build(key, username);
        KeyPreview = true;
        KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
        Paint += (s, e) => { using var bp = new Pen(AC.RED); e.Graphics.DrawRectangle(bp, 0, 0, Width - 1, Height - 1); };
    }

    void Build(string key, string username)
    {
        Controls.Add(new Panel { BackColor = AC.RED, Dock = DockStyle.Top, Height = 2 });

        var tb = new Panel { BackColor = AC.BG1, Dock = DockStyle.Top, Height = 34 };
        Point drag = Point.Empty;
        tb.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) drag = e.Location; };
        tb.MouseMove += (s, e) => { if (e.Button == MouseButtons.Left) { var sp = tb.PointToScreen(e.Location); Location = new Point(sp.X - drag.X, sp.Y - drag.Y); } };
        SL(tb, "▌", AC.CON12B, AC.RED, 10, 5);
        SL(tb, "BAN REASONING", AC.CON10B, AC.RED, 28, 7);
        string sub = !string.IsNullOrEmpty(username) && username != "—" ? $"{key}  ·  {username}" : key;
        SL(tb, sub, AC.CON8, AC.FG3, 28, 20);
        var closeBtn = new CloseBtn(); closeBtn.Dock = DockStyle.Right; closeBtn.Click += (s, e) => Close(); tb.Controls.Add(closeBtn);
        Controls.Add(tb);

        int y = 48, px = 16, w = ClientSize.Width - 32;

        SL(this, "REASON", AC.CON8B, AC.FG3, px, y); y += 17;
        var reasonBox = new ABox("why is this user / license being banned?");
        reasonBox.SetBounds(px, y, w, 26); Controls.Add(reasonBox); y += 34;

        SL(this, "DURATION", AC.CON8B, AC.FG3, px, y); y += 17;
        int bw = 68, gap = 8;
        int[] bx = { px, px + bw + gap, px + (bw + gap) * 2, px + (bw + gap) * 3, px + (bw + gap) * 4, px + (bw + gap) * 5 };
        string[] units = { "years", "months", "days", "hours", "mins", "secs" };
        _dYears = new NumBox(); _dYears.SetBounds(bx[0], y, bw, 24); Controls.Add(_dYears); SL(this, units[0], AC.CON8, AC.FG3, bx[0] + 16, y + 26);
        _dMonths = new NumBox(); _dMonths.SetBounds(bx[1], y, bw, 24); Controls.Add(_dMonths); SL(this, units[1], AC.CON8, AC.FG3, bx[1] + 10, y + 26);
        _dDays = new NumBox(); _dDays.SetBounds(bx[2], y, bw, 24); Controls.Add(_dDays); SL(this, units[2], AC.CON8, AC.FG3, bx[2] + 19, y + 26);
        _dHours = new NumBox(); _dHours.SetBounds(bx[3], y, bw, 24); Controls.Add(_dHours); SL(this, units[3], AC.CON8, AC.FG3, bx[3] + 17, y + 26);
        _dMins = new NumBox(); _dMins.SetBounds(bx[4], y, bw, 24); Controls.Add(_dMins); SL(this, units[4], AC.CON8, AC.FG3, bx[4] + 20, y + 26);
        _dSecs = new NumBox(); _dSecs.SetBounds(bx[5], y, bw, 24); Controls.Add(_dSecs); SL(this, units[5], AC.CON8, AC.FG3, bx[5] + 21, y + 26);
        y += 50;

        _lifetimeChk = new ACheck("Permanent ban  (no expiry)");
        _lifetimeChk.SetBounds(px, y, 200, 20); Controls.Add(_lifetimeChk);
        _durPreview = new Label { Text = "= 0 sec", Font = AC.CON8B, ForeColor = AC.FG3, BackColor = Color.Transparent, AutoSize = true };
        _durPreview.SetBounds(px + 210, y + 2, 310, 16); Controls.Add(_durPreview);
        y += 30;

        void UpdatePreview(object? s, EventArgs e2)
        {
            if (_lifetimeChk.Checked) return;
            long? secs = DurationHelper.ToSeconds(_dYears.Val, _dMonths.Val, _dDays.Val, _dHours.Val, _dMins.Val, _dSecs.Val);
            _durPreview.Text = "= " + DurationHelper.Describe(secs);
            _durPreview.ForeColor = secs == null ? AC.FG3 : AC.FG2;
        }
        _dYears.Leave += UpdatePreview; _dMonths.Leave += UpdatePreview; _dDays.Leave += UpdatePreview;
        _dHours.Leave += UpdatePreview; _dMins.Leave += UpdatePreview; _dSecs.Leave += UpdatePreview;
        _lifetimeChk.CheckedChanged += (s, e2) =>
        {
            bool lt = _lifetimeChk.Checked;
            _dYears.Enabled = _dMonths.Enabled = _dDays.Enabled = _dHours.Enabled = _dMins.Enabled = _dSecs.Enabled = !lt;
            _durPreview.Text = lt ? "= permanent" : "= " + DurationHelper.Describe(DurationHelper.ToSeconds(_dYears.Val, _dMonths.Val, _dDays.Val, _dHours.Val, _dMins.Val, _dSecs.Val));
            _durPreview.ForeColor = lt ? AC.REDB : AC.FG2;
        };

        SL(this, "NOTES  (optional)", AC.CON8B, AC.FG3, px, y); y += 17;
        var notesBox = new ABox("additional context, evidence, or admin notes...");
        notesBox.SetBounds(px, y, w, 26); Controls.Add(notesBox); y += 36;

        var banBtn = new ABtn("BAN", true); banBtn.SetBounds(px, y, 110, 28); Controls.Add(banBtn);
        var cancelBtn = new ABtn("Cancel", false); cancelBtn.SetBounds(px + 118, y, 80, 28); Controls.Add(cancelBtn);

        var errLbl = new Label { Text = "", Font = AC.CON8, ForeColor = AC.REDB, BackColor = Color.Transparent, AutoSize = true };
        errLbl.SetBounds(px + 206, y + 6, 320, 16); Controls.Add(errLbl);

        banBtn.Click += (s, e2) =>
        {
            string reason = reasonBox.Value;
            if (string.IsNullOrEmpty(reason)) { errLbl.Text = "a reason is required."; return; }
            long? secs = _lifetimeChk.Checked ? (long?)null : DurationHelper.ToSeconds(_dYears.Val, _dMonths.Val, _dDays.Val, _dHours.Val, _dMins.Val, _dSecs.Val);
            BanReason = reason;
            BanSeconds = secs;
            BanNotes = notesBox.Value;
            Confirmed = true;
            Close();
        };
        cancelBtn.Click += (s, e2) => Close();
    }

    static void SL(Control p, string txt, Font f, Color c, int x, int y)
    { var l = new Label { Text = txt, Font = f, ForeColor = c, BackColor = Color.Transparent, AutoSize = true }; l.SetBounds(x, y, 0, 0); p.Controls.Add(l); }
}


class AdminForm : Form
{
    Label? _statusLbl, _statsLbl;
    Panel? _logPanel;
    NoScrollPanel? _allKeysPanel, _banlandPanel;
    readonly List<KeyLogEntry> _log = new();
    List<BanRecord> _cachedBans = new();
    ABox? _banSearchBox;

    NumBox? _durYears, _durMonths, _durDays, _durHours, _durMins, _durSecs;
    Label? _durPreview;
    ACheck? _lifetimeChk;

    readonly Dictionary<string, Label> _infoLabels = new();
    ABox? _allKeysSearchBox;
    readonly List<(Panel Row, string Key, string Username)> _keyRowPanels = new();

    ScrollFreePanel? _keysTab, _banlandTab, _toolsTab, _settingsTab, _announcementsTab, _controlTab;
    ATabBtn? _keysTabBtn, _banlandTabBtn, _toolsTabBtn, _settingsTabBtn, _announcementsTabBtn, _controlTabBtn;
    Panel? _accentBar;
    Label? _titleLbl, _subtitleLbl;
    readonly List<Panel> _contentPanels = new();
    NoScrollPanel? _announcementsPanel;
    List<AnnouncementRecord> _cachedAnnouncements = new();

    // Control tab state
    SystemControlRecord _controlState = new();
    Label? _ctrlStateLbl;

    protected override CreateParams CreateParams
    {
        get { var cp = base.CreateParams; cp.ExStyle |= 0x02000000; return cp; }
    }

    public AdminForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        BackColor = AC.BG0;
        ClientSize = new Size(660, 780);
        StartPosition = FormStartPosition.CenterScreen;
        TopMost = true; DoubleBuffered = true;
        Build();
        KeyPreview = true;
        KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
        HandleCreated += (s, e) => BG(() => RefreshAllKeys());
    }

    void Build()
    {
        _accentBar = new Panel { BackColor = UISettings.AccentColor, Dock = DockStyle.Top, Height = 2 };
        Controls.Add(_accentBar);

        var tb = new Panel { BackColor = AC.BG1, Dock = DockStyle.Top, Height = 38 };
        Point drag = Point.Empty;
        tb.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) drag = e.Location; };
        tb.MouseMove += (s, e) => { if (e.Button == MouseButtons.Left) { var sp = tb.PointToScreen(e.Location); Location = new Point(sp.X - drag.X, sp.Y - drag.Y); } };
        _titleLbl = new Label { Text = "▌", Font = AC.CON12B, ForeColor = UISettings.AccentColor, BackColor = Color.Transparent, AutoSize = true };
        _titleLbl.SetBounds(10, 7, 0, 0); tb.Controls.Add(_titleLbl);
        _subtitleLbl = new Label { Text = "virinium  ·  admin", Font = AC.CON10B, ForeColor = UISettings.AccentColor, BackColor = Color.Transparent, AutoSize = true };
        _subtitleLbl.SetBounds(28, 8, 0, 0); tb.Controls.Add(_subtitleLbl);
        SL(tb, "key management", AC.CON8, AC.FG3, 28, 23);
        var closeBtn = new CloseBtn(); closeBtn.Dock = DockStyle.Right; closeBtn.Click += (s, e) => Close(); tb.Controls.Add(closeBtn);
        Controls.Add(tb);

        var tabBar = new Panel { BackColor = AC.BG1 };
        tabBar.SetBounds(0, 40, ClientSize.Width, 36);
        tabBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        tabBar.Paint += (s, e) => { using var p = new Pen(AC.BDR); e.Graphics.DrawLine(p, 0, tabBar.Height - 1, tabBar.Width, tabBar.Height - 1); };

        // Tab buttons — 6 tabs fitted across 660px
        _keysTabBtn = new ATabBtn("  KEYS  ") { Active = true }; _keysTabBtn.SetBounds(4, 1, 84, 34); tabBar.Controls.Add(_keysTabBtn);
        _banlandTabBtn = new ATabBtn("  BANLAND  "); _banlandTabBtn.SetBounds(90, 1, 98, 34); tabBar.Controls.Add(_banlandTabBtn);
        _toolsTabBtn = new ATabBtn("  TOOLS  "); _toolsTabBtn.SetBounds(190, 1, 84, 34); tabBar.Controls.Add(_toolsTabBtn);
        _settingsTabBtn = new ATabBtn("  SETTINGS  "); _settingsTabBtn.SetBounds(276, 1, 108, 34); tabBar.Controls.Add(_settingsTabBtn);
        _announcementsTabBtn = new ATabBtn("  ANNOUNCE  "); _announcementsTabBtn.SetBounds(386, 1, 118, 34); tabBar.Controls.Add(_announcementsTabBtn);
        _controlTabBtn = new ATabBtn("  CONTROL  "); _controlTabBtn.SetBounds(506, 1, 106, 34); tabBar.Controls.Add(_controlTabBtn);
        Controls.Add(tabBar);

        _keysTabBtn.Click += (s, e) => SwitchTab(0);
        _banlandTabBtn.Click += (s, e) => SwitchTab(1);
        _toolsTabBtn.Click += (s, e) => SwitchTab(2);
        _settingsTabBtn.Click += (s, e) => SwitchTab(3);
        _announcementsTabBtn.Click += (s, e) => SwitchTab(4);
        _controlTabBtn.Click += (s, e) => SwitchTab(5);

        _statusLbl = new Label { Text = "  ready.", Font = AC.CON9B, ForeColor = AC.FG3, BackColor = AC.BG1, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
        _statusLbl.SetBounds(0, ClientSize.Height - 28, ClientSize.Width, 28);
        _statusLbl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        Controls.Add(_statusLbl);

        int ct = 76, ch = ClientSize.Height - 76 - 28;

        _keysTab = new ScrollFreePanel { AutoScroll = true, BackColor = Color.FromArgb(255, 8, 8, 8) }; _keysTab.SetBounds(0, ct, ClientSize.Width, ch); _keysTab.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom; Controls.Add(_keysTab);
        _banlandTab = new ScrollFreePanel { AutoScroll = true, BackColor = Color.FromArgb(255, 8, 8, 8), Visible = false }; _banlandTab.SetBounds(0, ct, ClientSize.Width, ch); _banlandTab.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom; Controls.Add(_banlandTab);
        _toolsTab = new ScrollFreePanel { AutoScroll = true, BackColor = Color.FromArgb(255, 8, 8, 8), Visible = false }; _toolsTab.SetBounds(0, ct, ClientSize.Width, ch); _toolsTab.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom; Controls.Add(_toolsTab);
        _settingsTab = new ScrollFreePanel { AutoScroll = true, BackColor = Color.FromArgb(255, 8, 8, 8), Visible = false }; _settingsTab.SetBounds(0, ct, ClientSize.Width, ch); _settingsTab.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom; Controls.Add(_settingsTab);
        _announcementsTab = new ScrollFreePanel { AutoScroll = true, BackColor = Color.FromArgb(255, 8, 8, 8), Visible = false }; _announcementsTab.SetBounds(0, ct, ClientSize.Width, ch); _announcementsTab.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom; Controls.Add(_announcementsTab);
        _controlTab = new ScrollFreePanel { AutoScroll = true, BackColor = Color.FromArgb(255, 8, 8, 8), Visible = false }; _controlTab.SetBounds(0, ct, ClientSize.Width, ch); _controlTab.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom; Controls.Add(_controlTab);

        BuildKeysTab();
        BuildBanlandTab();
        BuildToolsTab();
        BuildSettingsTab();
        BuildAnnouncementsTab();
        BuildControlTab();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        if (UISettings.BackgroundImage != null)
        {
            ImageAnimator.UpdateFrames(UISettings.BackgroundImage);
            e.Graphics.DrawImage(UISettings.BackgroundImage,
                new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));
            using var br = new SolidBrush(Color.FromArgb(UISettings.BgOverlayAlpha, 8, 8, 8));
            e.Graphics.FillRectangle(br, ClientRectangle);
        }
        else
        {
            base.OnPaintBackground(e);
        }
    }

    void SwitchTab(int idx)
    {
        _keysTab.Visible = idx == 0;
        _banlandTab.Visible = idx == 1;
        _toolsTab.Visible = idx == 2;
        _settingsTab.Visible = idx == 3;
        _announcementsTab.Visible = idx == 4;
        _controlTab.Visible = idx == 5;
        _keysTabBtn.Active = idx == 0;
        _banlandTabBtn.Active = idx == 1;
        _toolsTabBtn.Active = idx == 2;
        _settingsTabBtn.Active = idx == 3;
        _announcementsTabBtn.Active = idx == 4;
        _controlTabBtn.Active = idx == 5;
        if (idx == 1) BG(() => RefreshBanland());
        if (idx == 4) BG(() => RefreshAnnouncements());
        if (idx == 5) BG(() => RefreshControlState());
    }


    void BuildKeysTab()
    {
        var body = _keysTab;
        int y = 14;
        const int PX = 20, W = 600;

        _statsLbl = new Label { Text = "  —", Font = AC.CON8, ForeColor = AC.FG3, BackColor = AC.BG2, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
        _statsLbl.SetBounds(PX, y, W, 26); _statsLbl.Paint += BorderPaint; body.Controls.Add(_statsLbl); y += 36;

        SectionHeader(body, "AUTHENTICATION", PX, ref y);
        var secretBox = new ABox("admin secret", true); secretBox.SetBounds(PX, y, W, 28); secretBox.SetValue(AdminConfig.AdminSecret);
        secretBox.TextChanged += (s, e) => AdminConfig.AdminSecret = secretBox.Value;
        body.Controls.Add(secretBox); y += 38;

        SectionHeader(body, "CREATE KEY", PX, ref y);
        var createBox = CP(new Panel { BackColor = AC.BG1 }); createBox.SetBounds(PX, y, W, 236); createBox.Paint += BorderPaint; body.Controls.Add(createBox);

        var userBox = new ABox("username  (optional)"); userBox.SetBounds(10, 10, 286, 28); createBox.Controls.Add(userBox);
        var noteBox = new ABox("note  (optional)"); noteBox.SetBounds(304, 10, 286, 28); createBox.Controls.Add(noteBox);
        SL(createBox, "DURATION", AC.CON8B, AC.FG3, 10, 50);

        int[] bx = { 10, 106, 202, 298, 394, 490 };
        string[] units = { "years", "months", "days", "hours", "mins", "secs" };
        _durYears = new NumBox(); _durYears.SetBounds(bx[0], 64, 82, 26); createBox.Controls.Add(_durYears); SL(createBox, units[0], AC.CON8, AC.FG3, bx[0] + 26, 93);
        _durMonths = new NumBox(); _durMonths.SetBounds(bx[1], 64, 82, 26); createBox.Controls.Add(_durMonths); SL(createBox, units[1], AC.CON8, AC.FG3, bx[1] + 20, 93);
        _durDays = new NumBox(); _durDays.SetBounds(bx[2], 64, 82, 26); createBox.Controls.Add(_durDays); SL(createBox, units[2], AC.CON8, AC.FG3, bx[2] + 29, 93);
        _durHours = new NumBox(); _durHours.SetBounds(bx[3], 64, 82, 26); createBox.Controls.Add(_durHours); SL(createBox, units[3], AC.CON8, AC.FG3, bx[3] + 27, 93);
        _durMins = new NumBox(); _durMins.SetBounds(bx[4], 64, 82, 26); createBox.Controls.Add(_durMins); SL(createBox, units[4], AC.CON8, AC.FG3, bx[4] + 30, 93);
        _durSecs = new NumBox(); _durSecs.SetBounds(bx[5], 64, 82, 26); createBox.Controls.Add(_durSecs); SL(createBox, units[5], AC.CON8, AC.FG3, bx[5] + 33, 93);

        _durPreview = new Label { Text = "= 0 sec", Font = AC.CON8B, ForeColor = AC.FG3, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = true };
        _lifetimeChk = new ACheck("Lifetime  (no expiry)");
        _lifetimeChk.SetBounds(10, 112, 180, 20); createBox.Controls.Add(_lifetimeChk);
        _durPreview.SetBounds(200, 114, 390, 16); createBox.Controls.Add(_durPreview);

        void UpdatePreview(object? s, EventArgs e)
        {
            if (_lifetimeChk.Checked) return;
            long? secs = DurationHelper.ToSeconds(_durYears.Val, _durMonths.Val, _durDays.Val, _durHours.Val, _durMins.Val, _durSecs.Val);
            _durPreview.Text = "= " + DurationHelper.Describe(secs); _durPreview.ForeColor = secs == null ? AC.FG3 : AC.FG2;
        }
        _durYears.Leave += UpdatePreview; _durMonths.Leave += UpdatePreview; _durDays.Leave += UpdatePreview;
        _durHours.Leave += UpdatePreview; _durMins.Leave += UpdatePreview; _durSecs.Leave += UpdatePreview;
        _lifetimeChk.CheckedChanged += (s, e) =>
        {
            bool lt = _lifetimeChk.Checked;
            _durYears.Enabled = _durMonths.Enabled = _durDays.Enabled = _durHours.Enabled = _durMins.Enabled = _durSecs.Enabled = !lt;
            _durPreview.Text = lt ? "= lifetime access" : "= " + DurationHelper.Describe(DurationHelper.ToSeconds(_durYears.Val, _durMonths.Val, _durDays.Val, _durHours.Val, _durMins.Val, _durSecs.Val));
            _durPreview.ForeColor = lt ? AC.GRN : AC.FG2;
        };

        var sep = new Panel { BackColor = AC.BDR }; sep.SetBounds(10, 138, W - 20, 1); createBox.Controls.Add(sep);
        var manualChk = new ACheck("Use custom key"); manualChk.SetBounds(10, 144, 130, 20); createBox.Controls.Add(manualChk);
        SL(createBox, "leave blank to auto-generate  (VIRI-XXXX-XXXX-XXXX)", AC.CON8, AC.FG3, 150, 148);
        var manualBox = new ABox("VIRI-XXXX-XXXX-XXXX"); manualBox.SetBounds(10, 166, W - 20, 28); manualBox.Enabled = false; createBox.Controls.Add(manualBox);
        manualChk.CheckedChanged += (s, e) => { manualBox.Enabled = manualChk.Checked; if (!manualChk.Checked) manualBox.SetValue(""); };

        var generateBtn = new ABtn("Generate Key", true); generateBtn.SetBounds(10, 200, 160, 28); createBox.Controls.Add(generateBtn);
        var keyOut = new TextBox { BackColor = AC.BG0, ForeColor = AC.REDB, BorderStyle = BorderStyle.FixedSingle, Font = AC.CON9B, ReadOnly = true };
        keyOut.SetBounds(178, 200, W - 188, 28); createBox.Controls.Add(keyOut);

        generateBtn.Click += (s, e) =>
        {
            string user = userBox.Value, note = noteBox.Value;
            string? manualKey = manualChk.Checked ? manualBox.Value.Trim().ToUpper() : null;
            if (!string.IsNullOrEmpty(manualKey))
            {
                var pts = manualKey.Split('-');
                if (pts.Length != 4 || pts[0] != "VIRI" || pts[1].Length != 4 || pts[2].Length != 4 || pts[3].Length != 4)
                { Status("✗  invalid key format", AC.REDB); return; }
            }
            long? totalSecs = _lifetimeChk.Checked ? (long?)null : DurationHelper.ToSeconds(_durYears.Val, _durMonths.Val, _durDays.Val, _durHours.Val, _durMins.Val, _durSecs.Val);
            string desc = DurationHelper.Describe(totalSecs);
            Status("generating key...", AC.FG3);
            BG(() =>
            {
                var (ok, key, error) = AdminApi.CreateKey(user, totalSecs, note, manualKey);
                UI(() =>
                {
                    if (ok) { keyOut.Text = key; Status("✓  key generated: " + key, AC.GRN); AddLog(new KeyLogEntry { Ok = true, Key = key, Username = user, Duration = desc, Time = DateTime.Now.ToString("HH:mm:ss") }); BG(() => RefreshAllKeys()); }
                    else Status("✗  " + error, AC.REDB);
                });
            });
        };
        y += 246;

        var keysHdrPanel = new Panel { BackColor = Color.FromArgb(255, 8, 8, 8) };
        keysHdrPanel.SetBounds(PX, y, W, 26); body.Controls.Add(keysHdrPanel);
        var keysTitleLbl = new Label { Text = "", Font = AC.CON8B, ForeColor = UISettings.AccentColor, BackColor = Color.FromArgb(255, 8, 8, 8), AutoSize = false };
        keysTitleLbl.SetBounds(0, 4, W - 200, 16);
        keysTitleLbl.Paint += (s2, e2) =>
        {
            e2.Graphics.Clear(Color.FromArgb(255, 8, 8, 8));
            int tw = (int)e2.Graphics.MeasureString("ALL ACTIVE KEYS", AC.CON8B).Width + 8;
            e2.Graphics.DrawString("ALL ACTIVE KEYS", AC.CON8B, new SolidBrush(UISettings.AccentColor), 0, 2);
            using var p = new Pen(AC.BDR); e2.Graphics.DrawLine(p, tw, 9, keysTitleLbl.Width, 9);
        };
        keysHdrPanel.Controls.Add(keysTitleLbl);
        var refreshBtn = new ABtn("Refresh", false); refreshBtn.SetBounds(W - 196, 2, 94, 22); keysHdrPanel.Controls.Add(refreshBtn);
        var fetchAllBtn = new ABtn("Fetch All", false); fetchAllBtn.SetBounds(W - 96, 2, 96, 22); keysHdrPanel.Controls.Add(fetchAllBtn);
        refreshBtn.Click += (s, e) => { Status("refreshing...", AC.FG3); BG(() => RefreshAllKeys()); };
        y += 34;

        _allKeysSearchBox = new ABox("search by key or username...");
        _allKeysSearchBox.SetBounds(PX, y, W, 28); body.Controls.Add(_allKeysSearchBox);
        _allKeysSearchBox.TextChanged += (s, e) => FilterAllKeys(_allKeysSearchBox?.Value ?? "");
        y += 36;

        _allKeysPanel = new NoScrollPanel { BackColor = AC.BG1 };
        _allKeysPanel.SetBounds(PX, y, W, 280); _allKeysPanel.Paint += BorderPaint; body.Controls.Add(_allKeysPanel);
        CP(_allKeysPanel);

        var ph = new Label { Text = "  loading...", Font = AC.CON8, ForeColor = AC.FG3, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
        ph.SetBounds(0, 0, W, 40); _allKeysPanel.Inner.Controls.Add(ph);
        y += 290;

        fetchAllBtn.Click += (s, e) =>
        {
            var snapshot = _infoLabels.ToList();
            if (snapshot.Count == 0) { Status("no keys loaded", AC.FG3); return; }
            fetchAllBtn.Enabled = false; Status($"fetching all HWIDs... (0/{snapshot.Count})", AC.FG2);
            int total = snapshot.Count, done = 0;
            foreach (var kvp in snapshot)
            {
                string k = kvp.Key; Label lbl = kvp.Value;
                BG(() =>
                {
                    var (hwid, user) = AdminApi.FetchHwidAndUser(k);
                    bool isSet = hwid != "not set" && !hwid.StartsWith("error");
                    int d = System.Threading.Interlocked.Increment(ref done);
                    UI(() =>
                    {
                        if (!lbl.IsDisposed) { lbl.Text = $"  user: {user}  ·  hwid: {hwid}"; lbl.ForeColor = isSet ? AC.GRN : AC.FG3; }
                        if (d >= total) { Status($"✓  fetched {total} HWID{(total == 1 ? "" : "s")}", AC.GRN); fetchAllBtn.Enabled = true; }
                        else Status($"fetching HWIDs... ({d}/{total})", AC.FG2);
                    });
                });
            }
        };

        SectionHeader(body, "KEY LOG  (this session)", PX, ref y);
        _logPanel = CP(new Panel { BackColor = AC.BG1 }); _logPanel.SetBounds(PX, y, W, 120); _logPanel.Paint += BorderPaint; body.Controls.Add(_logPanel);
        var emptyLbl = new Label { Text = "  no keys generated yet this session.", Font = AC.CON8, ForeColor = AC.FG3, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
        emptyLbl.SetBounds(0, 0, W, 120); _logPanel.Controls.Add(emptyLbl);
        y += 130;

        body.AutoScrollMinSize = new Size(0, y + 20);
    }


    void BuildBanlandTab()
    {
        var body = _banlandTab;
        int y = 14;
        const int PX = 20, W = 600;

        var banHdrPanel = new Panel { BackColor = Color.FromArgb(255, 8, 8, 8) };
        banHdrPanel.SetBounds(PX, y, W, 26); body.Controls.Add(banHdrPanel);

        var banTitleLbl = new Label { Text = "", Font = AC.CON8B, ForeColor = UISettings.AccentColor, BackColor = Color.FromArgb(255, 8, 8, 8), AutoSize = false };
        banTitleLbl.SetBounds(0, 4, W - 100, 16);
        banTitleLbl.Paint += (s2, e2) =>
        {
            e2.Graphics.Clear(Color.FromArgb(255, 8, 8, 8));
            int tw = (int)e2.Graphics.MeasureString("BANLAND", AC.CON8B).Width + 8;
            e2.Graphics.DrawString("BANLAND", AC.CON8B, new SolidBrush(UISettings.AccentColor), 0, 2);
            using var p = new Pen(AC.BDR); e2.Graphics.DrawLine(p, tw, 9, banTitleLbl.Width, 9);
        };
        banHdrPanel.Controls.Add(banTitleLbl);

        var refreshBtn = new ABtn("Refresh", false); refreshBtn.SetBounds(W - 96, 2, 96, 22); banHdrPanel.Controls.Add(refreshBtn);
        refreshBtn.Click += (s, e) => { Status("loading bans...", AC.FG3); BG(() => RefreshBanland()); };
        y += 34;

        _banSearchBox = new ABox("search by username, key, or reason...");
        _banSearchBox.SetBounds(PX, y, W, 28); body.Controls.Add(_banSearchBox);
        _banSearchBox.TextChanged += (s, e) => FilterBanland(_banSearchBox?.Value ?? "");
        y += 36;

        _banlandPanel = new NoScrollPanel { BackColor = AC.BG1 };
        _banlandPanel.SetBounds(PX, y, W, 490); _banlandPanel.Paint += BorderPaint; body.Controls.Add(_banlandPanel);
        CP(_banlandPanel);

        var ph = new Label { Text = "  press refresh to load bans.", Font = AC.CON8, ForeColor = AC.FG3, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
        ph.SetBounds(0, 0, W, 40); _banlandPanel.Inner.Controls.Add(ph);
        y += 500;

        body.AutoScrollMinSize = new Size(0, y + 20);
    }


    void BuildToolsTab()
    {
        var body = _toolsTab;
        int y = 14;
        const int PX = 20, W = 600;

        SectionHeader(body, "MANAGE KEY", PX, ref y);
        var managePanel = CP(new Panel { BackColor = AC.BG1 }); managePanel.SetBounds(PX, y, W, 80); managePanel.Paint += BorderPaint; body.Controls.Add(managePanel);

        var mgmtBox = new ABox("VIRI-XXXX-XXXX-XXXX"); mgmtBox.SetBounds(10, 10, W - 20, 28); managePanel.Controls.Add(mgmtBox);
        var rvkBtn = new ABtn("Revoke", true); rvkBtn.SetBounds(10, 46, 120, 24); managePanel.Controls.Add(rvkBtn);
        var urvkBtn = new ABtn("Restore", false); urvkBtn.SetBounds(138, 46, 120, 24); managePanel.Controls.Add(urvkBtn);
        var hwidBtn = new ABtn("Reset HWID", false); hwidBtn.SetBounds(266, 46, 120, 24); managePanel.Controls.Add(hwidBtn);
        var banQ = new ABtn("Ban Key", true); banQ.SetBounds(394, 46, 120, 24); managePanel.Controls.Add(banQ);

        rvkBtn.Click += (s, e) => ManageAction(mgmtBox, "revoking...", k => AdminApi.RevokeKey(k, true), "✓  key revoked", true);
        urvkBtn.Click += (s, e) => ManageAction(mgmtBox, "restoring...", k => AdminApi.RevokeKey(k, false), "✓  key restored", true);
        hwidBtn.Click += (s, e) => ManageAction(mgmtBox, "resetting...", k => AdminApi.ResetHwid(k), "✓  hwid cleared", false);
        banQ.Click += (s, e) =>
        {
            string k = mgmtBox.Value; if (k == "") { Status("enter a key", AC.REDB); return; }
            var dlg = new BanDialog(k, "");
            dlg.ShowDialog(this);
            if (!dlg.Confirmed) return;
            Status("banning " + k + "...", AC.FG3);
            BG(() => { var (ok, err) = AdminApi.BanKey(k, dlg.BanReason, dlg.BanSeconds, dlg.BanNotes); UI(() => { Status(ok ? "✓  banned: " + k : "✗  " + err, ok ? AC.GRN : AC.REDB); if (ok) BG(() => RefreshAllKeys()); }); });
        };
        y += 92;

        SectionHeader(body, "CHECK KEY", PX, ref y);
        var chkPanel = CP(new Panel { BackColor = AC.BG1 }); chkPanel.SetBounds(PX, y, W, 72); chkPanel.Paint += BorderPaint; body.Controls.Add(chkPanel);

        var chkInput = new ABox("VIRI-XXXX-XXXX-XXXX"); chkInput.SetBounds(10, 10, 416, 28); chkPanel.Controls.Add(chkInput);
        var chkBtn = new ABtn("Check", false); chkBtn.SetBounds(434, 10, 156, 28); chkPanel.Controls.Add(chkBtn);
        var chkResult = new Label { Text = "", Font = AC.CON8, ForeColor = AC.FG3, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false }; chkResult.SetBounds(10, 44, W - 20, 20); chkPanel.Controls.Add(chkResult);

        chkBtn.Click += (s, e) =>
        {
            string k = chkInput.Value; if (k == "") { Status("enter a key", AC.REDB); return; }
            Status("checking...", AC.FG3); chkResult.Text = "querying..."; chkResult.ForeColor = AC.FG3;
            BG(() =>
            {
                var (v, u, r) = AdminApi.CheckKey(k);
                var (hwid, fetchedUser) = AdminApi.FetchHwidAndUser(k);
                string usr = (!string.IsNullOrEmpty(fetchedUser) && fetchedUser != "—") ? fetchedUser : (!string.IsNullOrEmpty(u) ? u : "—");
                UI(() =>
                {
                    chkResult.Text = v ? $"✓  valid  ·  user: {usr}  ·  {r}  ·  hwid: {hwid}" : $"✗  invalid  ·  {r}";
                    chkResult.ForeColor = v ? AC.GRN : AC.REDB;
                    Status(v ? "✓  key is valid" : "✗  key is invalid", v ? AC.GRN : AC.REDB);
                });
            });
        };
        y += 84;

        SectionHeader(body, "EXTEND AND SHORTEN KEYS", PX, ref y);
        var adjPanel = CP(new Panel { BackColor = AC.BG1 }); adjPanel.SetBounds(PX, y, W, 142); adjPanel.Paint += BorderPaint; body.Controls.Add(adjPanel);

        var adjKeyBox = new ABox("VIRI-XXXX-XXXX-XXXX"); adjKeyBox.SetBounds(10, 10, W - 20, 28); adjPanel.Controls.Add(adjKeyBox);

        SL(adjPanel, "DURATION", AC.CON8B, AC.FG3, 10, 48);
        int[] adjbx = { 10, 106, 202, 298, 394, 490 };
        string[] adjUnits = { "years", "months", "days", "hours", "mins", "secs" };
        var adjYears = new NumBox(); adjYears.SetBounds(adjbx[0], 62, 82, 26); adjPanel.Controls.Add(adjYears); SL(adjPanel, adjUnits[0], AC.CON8, AC.FG3, adjbx[0] + 26, 91);
        var adjMonths = new NumBox(); adjMonths.SetBounds(adjbx[1], 62, 82, 26); adjPanel.Controls.Add(adjMonths); SL(adjPanel, adjUnits[1], AC.CON8, AC.FG3, adjbx[1] + 20, 91);
        var adjDays = new NumBox(); adjDays.SetBounds(adjbx[2], 62, 82, 26); adjPanel.Controls.Add(adjDays); SL(adjPanel, adjUnits[2], AC.CON8, AC.FG3, adjbx[2] + 29, 91);
        var adjHours = new NumBox(); adjHours.SetBounds(adjbx[3], 62, 82, 26); adjPanel.Controls.Add(adjHours); SL(adjPanel, adjUnits[3], AC.CON8, AC.FG3, adjbx[3] + 27, 91);
        var adjMins = new NumBox(); adjMins.SetBounds(adjbx[4], 62, 82, 26); adjPanel.Controls.Add(adjMins); SL(adjPanel, adjUnits[4], AC.CON8, AC.FG3, adjbx[4] + 30, 91);
        var adjSecs = new NumBox(); adjSecs.SetBounds(adjbx[5], 62, 82, 26); adjPanel.Controls.Add(adjSecs); SL(adjPanel, adjUnits[5], AC.CON8, AC.FG3, adjbx[5] + 33, 91);

        var extendBtn = new ABtn("Extend", false); extendBtn.SetBounds(10, 110, 120, 26); adjPanel.Controls.Add(extendBtn);
        var shortenBtn = new ABtn("Shorten", true); shortenBtn.SetBounds(138, 110, 120, 26); adjPanel.Controls.Add(shortenBtn);
        var adjResult = new Label { Text = "", Font = AC.CON8, ForeColor = AC.FG3, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false };
        adjResult.SetBounds(268, 114, W - 278, 18); adjPanel.Controls.Add(adjResult);

        long AdjDeltaSeconds() => DurationHelper.ToSeconds(adjYears.Val, adjMonths.Val, adjDays.Val, adjHours.Val, adjMins.Val, adjSecs.Val) ?? 0;

        extendBtn.Click += (s, e) =>
        {
            string k = adjKeyBox.Value; if (k == "") { Status("enter a key", AC.REDB); return; }
            long delta = AdjDeltaSeconds(); if (delta <= 0) { adjResult.Text = "enter a duration"; adjResult.ForeColor = AC.REDB; return; }
            adjResult.Text = "extending..."; adjResult.ForeColor = AC.FG3;
            Status("extending key...", AC.FG3);
            BG(() => { var (ok, err) = AdminApi.AdjustKeyDuration(k, delta); UI(() => { adjResult.Text = ok ? $"✓  extended by {DurationHelper.Describe(delta)}" : "✗  " + err; adjResult.ForeColor = ok ? AC.GRN : AC.REDB; Status(ok ? "✓  key extended" : "✗  " + err, ok ? AC.GRN : AC.REDB); }); });
        };

        shortenBtn.Click += (s, e) =>
        {
            string k = adjKeyBox.Value; if (k == "") { Status("enter a key", AC.REDB); return; }
            long delta = AdjDeltaSeconds(); if (delta <= 0) { adjResult.Text = "enter a duration"; adjResult.ForeColor = AC.REDB; return; }
            adjResult.Text = "shortening..."; adjResult.ForeColor = AC.FG3;
            Status("shortening key...", AC.FG3);
            BG(() => { var (ok, err) = AdminApi.AdjustKeyDuration(k, -delta); UI(() => { adjResult.Text = ok ? $"✓  shortened by {DurationHelper.Describe(delta)}" : "✗  " + err; adjResult.ForeColor = ok ? AC.GRN : AC.REDB; Status(ok ? "✓  key shortened" : "✗  " + err, ok ? AC.GRN : AC.REDB); }); });
        };

        y += 154;

        body.AutoScrollMinSize = new Size(0, y + 20);
    }

    void BuildSettingsTab()
    {
        var body = _settingsTab;
        int y = 14;
        const int PX = 20, W = 600;

        SectionHeader(body, "ACCENT COLOR", PX, ref y);
        var colorPanel = CP(new Panel { BackColor = AC.BG1 });
        colorPanel.SetBounds(PX, y, W, 48); colorPanel.Paint += BorderPaint; body.Controls.Add(colorPanel);

        var presets = new[]
        {
            Color.FromArgb(210,28,28),  Color.FromArgb(30,140,255), Color.FromArgb(50,200,80),
            Color.FromArgb(220,140,30), Color.FromArgb(150,60,220), Color.FromArgb(220,60,150),
            Color.FromArgb(0,190,190),  Color.FromArgb(200,200,200)
        };
        for (int i = 0; i < presets.Length; i++)
        {
            var pc = presets[i];
            var ps = new AColorSwatch(pc, false);
            ps.SetBounds(10 + i * 34, 8, 28, 28); colorPanel.Controls.Add(ps);
            ps.ColorChanged += (s, e) => { UISettings.AccentColor = pc; ApplyUISettings(); };
        }

        var customBtn = new ABtn("Custom…", false);
        customBtn.SetBounds(10 + presets.Length * 34 + 8, 8, 72, 28); colorPanel.Controls.Add(customBtn);
        customBtn.Click += (s, e) =>
        {
            using var dlg = new ColorDialog { Color = UISettings.AccentColor, FullOpen = true };
            if (dlg.ShowDialog() == DialogResult.OK) { UISettings.AccentColor = dlg.Color; ApplyUISettings(); }
        };
        y += 60;

        SectionHeader(body, "WINDOW", PX, ref y);
        var winPanel = CP(new Panel { BackColor = AC.BG1 });
        winPanel.SetBounds(PX, y, W, 42); winPanel.Paint += BorderPaint; body.Controls.Add(winPanel);

        var aotChk = new ACheck("Always on Top") { Checked = UISettings.AlwaysOnTop };
        aotChk.SetBounds(10, 12, 170, 20); winPanel.Controls.Add(aotChk);
        aotChk.CheckedChanged += (s, e) => { UISettings.AlwaysOnTop = aotChk.Checked; TopMost = aotChk.Checked; };
        y += 54;

        SectionHeader(body, "FONT", PX, ref y);
        var fontPanel = CP(new Panel { BackColor = AC.BG1 });
        fontPanel.SetBounds(PX, y, W, 58); fontPanel.Paint += BorderPaint; body.Controls.Add(fontPanel);

        var fontChk = new ACheck("Use custom font") { Checked = UISettings.UseCustomFont };
        fontChk.SetBounds(10, 10, 160, 20); fontPanel.Controls.Add(fontChk);

        SL(fontPanel, "font family:", AC.CON8, AC.FG3, 10, 34);
        var fontCombo = new ComboBox
        {
            FlatStyle = FlatStyle.Flat,
            Font = AC.CON9,
            BackColor = AC.BG1,
            ForeColor = AC.FG,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Enabled = UISettings.UseCustomFont
        };
        fontCombo.SetBounds(84, 31, 300, 22); fontPanel.Controls.Add(fontCombo);

        var monoFonts = new[] { "Consolas", "Courier New", "Lucida Console", "Cascadia Code", "JetBrains Mono", "Fira Code", "Source Code Pro", "Inconsolata", "Segoe UI", "Tahoma", "Verdana" };
        foreach (var f in monoFonts) fontCombo.Items.Add(f);
        fontCombo.SelectedItem = UISettings.FontFamily;
        if (fontCombo.SelectedIndex < 0 && fontCombo.Items.Count > 0) fontCombo.SelectedIndex = 0;

        fontChk.CheckedChanged += (s, e) =>
        {
            UISettings.UseCustomFont = fontChk.Checked;
            fontCombo.Enabled = fontChk.Checked;
            ApplyUISettings();
        };
        fontCombo.SelectedIndexChanged += (s, e) =>
        {
            UISettings.FontFamily = fontCombo.SelectedItem?.ToString() ?? "Consolas";
            if (UISettings.UseCustomFont) ApplyUISettings();
        };
        y += 70;

        SectionHeader(body, "UI TRANSPARENCY", PX, ref y);
        var transPanel = CP(new Panel { BackColor = AC.BG1 });
        transPanel.SetBounds(PX, y, W, 92); transPanel.Paint += BorderPaint; body.Controls.Add(transPanel);

        SL(transPanel, "UI opacity", AC.CON8, AC.FG3, 10, 8);
        var transSlider = new AGlowSlider(10, 100, UISettings.Transparency);
        transSlider.SetBounds(10, 22, W - 20, 22); transPanel.Controls.Add(transSlider);
        transSlider.ValueChanged += (s, e) => { UISettings.Transparency = transSlider.Value; Opacity = UISettings.Transparency / 100.0; };

        SL(transPanel, "element opacity  (panels, boxes, headers)", AC.CON8, AC.FG3, 10, 52);
        var elemSlider = new AGlowSlider(15, 100, (int)(UISettings.PanelAlpha / 255.0 * 100));
        elemSlider.SetBounds(10, 66, W - 20, 22); transPanel.Controls.Add(elemSlider);
        elemSlider.ValueChanged += (s, e) =>
        {
            UISettings.PanelAlpha = (int)(elemSlider.Value / 100.0 * 255);
            ApplyPanelAlpha();
        };
        y += 104;

        SectionHeader(body, "BACKGROUND IMAGE", PX, ref y);
        var bgPanel = CP(new Panel { BackColor = AC.BG1 });
        bgPanel.SetBounds(PX, y, W, 100); bgPanel.Paint += BorderPaint; body.Controls.Add(bgPanel);

        SL(bgPanel, "import a .png or .gif (max 20 MB) as the UI background", AC.CON8, AC.FG3, 10, 8);

        var bgStatusLbl = new Label { Text = "  no image loaded", Font = AC.CON8, ForeColor = AC.FG3, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
        bgStatusLbl.SetBounds(10, 28, W - 120, 22); bgPanel.Controls.Add(bgStatusLbl);

        var bgAlphaSlider = new AGlowSlider(0, 100, 60);
        bgAlphaSlider.SetBounds(10, 60, W - 120, 22); bgPanel.Controls.Add(bgAlphaSlider);
        SL(bgPanel, "overlay alpha", AC.CON8, AC.FG3, 10, 84);

        var importBtn = new ABtn("Import Image", false); importBtn.SetBounds(W - 100, 28, 92, 24); bgPanel.Controls.Add(importBtn);
        var clearBtn = new ABtn("Clear", false); clearBtn.SetBounds(W - 100, 58, 92, 22); bgPanel.Controls.Add(clearBtn);

        bgAlphaSlider.ValueChanged += (s, e) =>
        {
            UISettings.BgOverlayAlpha = (int)((100 - bgAlphaSlider.Value) / 100.0 * 230);
            Invalidate();
        };

        importBtn.Click += (s, e) =>
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select background image",
                Filter = "Images (*.png;*.gif)|*.png;*.gif",
                CheckFileExists = true
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var fi = new FileInfo(ofd.FileName);
            if (fi.Length > 20 * 1024 * 1024) { bgStatusLbl.Text = "  ✗  file exceeds 20 MB limit"; bgStatusLbl.ForeColor = AC.REDB; return; }
            try
            {
                if (UISettings.BackgroundImage != null)
                    try { ImageAnimator.StopAnimate(UISettings.BackgroundImage, OnGifFrame); } catch { }
                var img = Image.FromFile(ofd.FileName);
                UISettings.BackgroundImage = img;
                UISettings.BgOverlayAlpha = (int)((100 - bgAlphaSlider.Value) / 100.0 * 230);
                bgStatusLbl.Text = $"  ✓  {Path.GetFileName(ofd.FileName)}  ({fi.Length / 1024} KB)";
                bgStatusLbl.ForeColor = AC.GRN;
                if (ImageAnimator.CanAnimate(img))
                    ImageAnimator.Animate(img, OnGifFrame);
                else
                    Invalidate();
            }
            catch (Exception ex) { bgStatusLbl.Text = "  ✗  " + ex.Message; bgStatusLbl.ForeColor = AC.REDB; }
        };

        clearBtn.Click += (s, e) =>
        {
            if (UISettings.BackgroundImage != null)
            {
                try { ImageAnimator.StopAnimate(UISettings.BackgroundImage, OnGifFrame); } catch { }
                UISettings.BackgroundImage = null;
            }
            bgStatusLbl.Text = "  no image loaded"; bgStatusLbl.ForeColor = AC.FG3;
            Invalidate();
        };

        y += 112;
        body.AutoScrollMinSize = new Size(0, y + 20);
    }

    Panel CP(Panel p)
    {
        _contentPanels.Add(p);
        return p;
    }

    void CP(NoScrollPanel p)
    {
        _contentPanels.Add(p);
    }

    void ApplyPanelAlpha()
    {
        int a = Math.Max(0, Math.Min(255, UISettings.PanelAlpha));
        var opaqueColor = Color.FromArgb(255, 14, 14, 14);
        void UpdateChildren(Control ctrl)
        {
            foreach (Control c in ctrl.Controls)
            {
                if (c is Label lbl) lbl.BackColor = opaqueColor;
                else if (c is Panel pnl && pnl.BackColor != Color.Transparent) pnl.BackColor = opaqueColor;
                UpdateChildren(c);
            }
        }
        foreach (var p in _contentPanels)
        {
            p.BackColor = Color.FromArgb(a, 14, 14, 14);
            UpdateChildren(p);
        }
        Invalidate(true);
    }

    void OnGifFrame(object? sender, EventArgs e)
    {
        if (IsHandleCreated && !IsDisposed)
            BeginInvoke(new Action(() => { Invalidate(); Update(); }));
    }

    void ApplyUISettings()
    {
        if (_accentBar != null) _accentBar.BackColor = UISettings.AccentColor;
        if (_titleLbl != null) _titleLbl.ForeColor = UISettings.AccentColor;
        if (_subtitleLbl != null) _subtitleLbl.ForeColor = UISettings.AccentColor;
        Invalidate(true);
        if (UISettings.UseCustomFont)
        {
            try
            {
                var f8 = new Font(UISettings.FontFamily, 8f);
                var f9 = new Font(UISettings.FontFamily, 9f);
                var f9b = new Font(UISettings.FontFamily, 9f, FontStyle.Bold);
                void SetFont(Control c) { if (c is Label || c is Button || c is TextBox) { c.Font = c.Font.Bold ? f9b : (c.Font.Size < 9f ? f8 : f9); } foreach (Control ch in c.Controls) SetFont(ch); }
                foreach (Control c in Controls) SetFont(c);
            }
            catch { }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using (var bp = new Pen(AC.BDR)) e.Graphics.DrawRectangle(bp, 1, 1, Width - 3, Height - 3);
    }


    void ManageAction(ABox keyBox, string status, Func<string, (bool, string)> action, string successMsg, bool refresh)
    {
        string k = keyBox.Value; if (k == "") { Status("enter a key", AC.REDB); return; }
        Status(status, AC.FG3);
        BG(() => { var (ok, err) = action(k); UI(() => { Status(ok ? successMsg : "✗  " + err, ok ? AC.GRN : AC.REDB); if (ok && refresh) BG(() => RefreshAllKeys()); }); });
    }


    void RefreshAllKeys()
    {
        var (ok, keys, err) = AdminApi.GetActiveKeys();
        UI(() =>
        {
            _infoLabels.Clear();
            _keyRowPanels.Clear();

            if (ok)
            {
                int total = keys.Count, valid = keys.Count(k => !k.Expired), exp = keys.Count(k => k.Expired);
                int lifetime = keys.Count(k => string.IsNullOrEmpty(k.ExpiresAt));
                _statsLbl.Text = $"  {total} active  ·  {valid} valid  ·  {exp} expired  ·  {lifetime} lifetime";
                _statsLbl.ForeColor = total == 0 ? AC.FG3 : AC.FG2;
            }

            var inner = _allKeysPanel.Inner;
            inner.Controls.Clear();

            if (!ok)
            {
                var el = new Label { Text = "  ✗  " + err, Font = AC.CON8, ForeColor = AC.REDB, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
                el.SetBounds(0, 0, inner.Width, 30); inner.Controls.Add(el);
                _allKeysPanel.Commit(30); Status("✗  failed to load keys", AC.REDB); return;
            }
            if (keys.Count == 0)
            {
                var nl = new Label { Text = "  no active keys.", Font = AC.CON8, ForeColor = AC.FG3, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
                nl.SetBounds(0, 0, inner.Width, 40); inner.Controls.Add(nl);
                _allKeysPanel.Commit(40); Status("✓  0 active keys", AC.GRN); return;
            }

            int pw = _allKeysPanel.Width - 2;
            var hdr = new Label { Text = "  key                              user / hwid                   expires", Font = AC.CON8B, ForeColor = AC.FG2, BackColor = Color.FromArgb(255, AC.BG2.R, AC.BG2.G, AC.BG2.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            hdr.SetBounds(0, 0, pw, 20); inner.Controls.Add(hdr);

            int ly = 22;
            foreach (var k in keys)
            {
                string kCopy = k.Key;
                string exp = string.IsNullOrEmpty(k.ExpiresAt) ? "lifetime" : (DateTime.TryParse(k.ExpiresAt, out var dt) ? dt.ToString("yyyy-MM-dd HH:mm") : k.ExpiresAt);
                string user = string.IsNullOrEmpty(k.Username) ? "—" : k.Username;
                string note = string.IsNullOrEmpty(k.Note) ? "" : "  ·  " + k.Note;
                bool hwidUnset = string.IsNullOrEmpty(k.Hwid) || k.Hwid == "HWID-UNKNOWN" || k.Hwid == "ADMIN-CHECK-ONLY";
                string hwidTxt = hwidUnset ? "not set" : k.Hwid;

                var row = new Panel { BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), Tag = kCopy };
                row.SetBounds(0, ly, pw, 52);
                row.Paint += (s2, e2) =>
                {
                    using var p = new Pen(AC.BDR);
                    e2.Graphics.DrawLine(p, 0, 0, row.Width, 0);
                    if (k.Expired) e2.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(12, 60, 20, 20)), 0, 0, row.Width, row.Height);
                };

                var keyLbl = new Label { Text = $"  {k.Key}", Font = AC.CON8B, ForeColor = k.Expired ? AC.FG3 : AC.FG, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
                keyLbl.SetBounds(0, 0, pw - 340, 24); row.Controls.Add(keyLbl);

                var copyKeyBtn = new ABtn("Copy Key", false); copyKeyBtn.SetBounds(pw - 336, 2, 68, 20); row.Controls.Add(copyKeyBtn);
                var fetchBtn = new ABtn("Fetch", false); fetchBtn.SetBounds(pw - 262, 2, 54, 20); row.Controls.Add(fetchBtn);
                var banBtn = new ABtn("Ban", true); banBtn.SetBounds(pw - 202, 2, 54, 20); row.Controls.Add(banBtn);
                var rvkBtn2 = new ABtn("Revoke", true); rvkBtn2.SetBounds(pw - 142, 2, 68, 20); row.Controls.Add(rvkBtn2);
                var hwidRstBtn = new ABtn("HWID", false); hwidRstBtn.SetBounds(pw - 68, 2, 68, 20); row.Controls.Add(hwidRstBtn);

                var infoLbl = new Label { Text = $"  user: {user}  ·  hwid: {hwidTxt}  ·  {exp}{note}", Font = AC.CON8, ForeColor = hwidUnset ? AC.FG3 : AC.FG2, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
                infoLbl.SetBounds(0, 24, pw, 20); row.Controls.Add(infoLbl);

                _infoLabels[kCopy] = infoLbl;

                string capturedUser = user;
                string capturedExp = exp;
                string capturedNote = note;

                copyKeyBtn.Click += (s2, e2) =>
                {
                    try { Clipboard.SetText(kCopy); Status("✓  copied: " + kCopy, AC.GRN); }
                    catch { Status("✗  clipboard unavailable", AC.REDB); }
                };

                fetchBtn.Click += (s2, e2) =>
                {
                    fetchBtn.Enabled = false; infoLbl.Text = "  fetching..."; infoLbl.ForeColor = AC.FG3;
                    BG(() =>
                    {
                        var (hwid, fetchedUser) = AdminApi.FetchHwidAndUser(kCopy);
                        bool isSet = hwid != "not set" && !hwid.StartsWith("error");
                        UI(() =>
                        {
                            infoLbl.Text = $"  user: {fetchedUser}  ·  hwid: {hwid}  ·  {capturedExp}{capturedNote}";
                            infoLbl.ForeColor = isSet ? AC.GRN : AC.FG3;
                            fetchBtn.Enabled = true;
                            Status(isSet ? $"✓  fetched: {kCopy}" : $"·  no hwid set for {kCopy}", isSet ? AC.GRN : AC.FG2);
                            // Keep _keyRowPanels username in sync so search works after fetch
                            for (int ri = 0; ri < _keyRowPanels.Count; ri++)
                            {
                                if (_keyRowPanels[ri].Key == kCopy)
                                {
                                    _keyRowPanels[ri] = (_keyRowPanels[ri].Row, kCopy, fetchedUser);
                                    break;
                                }
                            }
                        });
                    });
                };

                banBtn.Click += (s2, e2) =>
                {
                    var dlg = new BanDialog(kCopy, capturedUser == "—" ? "" : capturedUser);
                    dlg.ShowDialog(this);
                    if (!dlg.Confirmed) return;
                    Status("banning " + kCopy + "...", AC.FG3);
                    BG(() => { var (banOk, banErr) = AdminApi.BanKey(kCopy, dlg.BanReason, dlg.BanSeconds, dlg.BanNotes); UI(() => { Status(banOk ? "✓  banned: " + kCopy : "✗  " + banErr, banOk ? AC.GRN : AC.REDB); if (banOk) BG(() => RefreshAllKeys()); }); });
                };

                rvkBtn2.Click += (s2, e2) =>
                {
                    if (MessageBox.Show($"Revoke {kCopy}?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                    Status("revoking...", AC.FG3);
                    BG(() => { var (rOk, rErr) = AdminApi.RevokeKey(kCopy, true); UI(() => { Status(rOk ? "✓  revoked: " + kCopy : "✗  " + rErr, rOk ? AC.GRN : AC.REDB); if (rOk) BG(() => RefreshAllKeys()); }); });
                };

                hwidRstBtn.Click += (s2, e2) =>
                {
                    if (MessageBox.Show($"Reset HWID for {kCopy}?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
                    Status("resetting hwid...", AC.FG3);
                    BG(() => { var (rOk, rErr) = AdminApi.ResetHwid(kCopy); UI(() => Status(rOk ? "✓  hwid reset for " + kCopy : "✗  " + rErr, rOk ? AC.GRN : AC.REDB)); });
                };

                inner.Controls.Add(row);
                _keyRowPanels.Add((row, kCopy, user == "—" ? "" : user));
                ly += 54;
            }

            _allKeysPanel.Commit(ly + 4);
            Status($"✓  {keys.Count} key{(keys.Count == 1 ? "" : "s")} loaded  ·  verifying HWIDs...", AC.GRN);

            // Re-apply any active search
            if (!string.IsNullOrWhiteSpace(_allKeysSearchBox?.Value))
                FilterAllKeys(_allKeysSearchBox.Value);

            var fallbackSnap = _infoLabels.ToList();
            int fallbackTotal = fallbackSnap.Count, fallbackDone = 0;
            foreach (var kvp in fallbackSnap)
            {
                string k2 = kvp.Key; Label lbl2 = kvp.Value;
                BG(() =>
                {
                    var (hwid2, user2) = AdminApi.FetchHwidAndUser(k2);
                    bool isSet2 = hwid2 != "not set" && !hwid2.StartsWith("error");
                    int d = System.Threading.Interlocked.Increment(ref fallbackDone);
                    UI(() =>
                    {
                        if (!lbl2.IsDisposed && (lbl2.Text.Contains("not set") || lbl2.Text.Contains("user: —")))
                        {
                            lbl2.Text = $"  user: {user2}  ·  hwid: {hwid2}";
                            lbl2.ForeColor = isSet2 ? AC.GRN : AC.FG3;
                            for (int ri = 0; ri < _keyRowPanels.Count; ri++)
                                if (_keyRowPanels[ri].Key == k2) { _keyRowPanels[ri] = (_keyRowPanels[ri].Row, k2, user2); break; }
                        }
                        if (d >= fallbackTotal)
                            Status($"✓  {keys.Count} key{(keys.Count == 1 ? "" : "s")} loaded", AC.GRN);
                    });
                });
            }
        });
    }


    void RefreshBanland()
    {
        var (ok, bans, err) = AdminApi.GetBannedKeys();
        UI(() =>
        {
            if (!ok)
            {
                var inner = _banlandPanel.Inner; inner.Controls.Clear();
                var el = new Label { Text = "  ✗  " + err, Font = AC.CON8, ForeColor = AC.REDB, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
                el.SetBounds(0, 0, inner.Width, 30); inner.Controls.Add(el);
                _banlandPanel.Commit(30); Status("✗  failed to load bans", AC.REDB); return;
            }
            _cachedBans = bans;
            FilterBanland(_banSearchBox?.Value ?? "");
        });
    }

    void FilterBanland(string query)
    {
        string q = (query ?? "").Trim().ToLowerInvariant();
        var filtered = string.IsNullOrEmpty(q)
            ? _cachedBans
            : _cachedBans.FindAll(b =>
                (b.Key ?? "").ToLowerInvariant().Contains(q) ||
                (b.Username ?? "").ToLowerInvariant().Contains(q) ||
                (b.BanReason ?? "").ToLowerInvariant().Contains(q));
        RenderBanland(filtered, q);
    }

    void FilterAllKeys(string query)
    {
        string q = (query ?? "").Trim().ToLowerInvariant();
        var inner = _allKeysPanel?.Inner;
        if (inner == null || _keyRowPanels.Count == 0) return;
        int ly = 22; // leave room for the header row at y=0
        foreach (var (row, key, username) in _keyRowPanels)
        {
            bool show = string.IsNullOrEmpty(q)
                || key.ToLowerInvariant().Contains(q)
                || username.ToLowerInvariant().Contains(q);
            row.Visible = show;
            if (show) { row.Top = ly; ly += 54; }
        }
        _allKeysPanel?.Commit(ly + 4);
    }

    void RenderBanland(List<BanRecord> bans, string query)
    {
        var inner = _banlandPanel.Inner;
        inner.Controls.Clear();

        if (_cachedBans.Count == 0)
        {
            var nl = new Label { Text = "  no banned keys found.", Font = AC.CON8, ForeColor = AC.FG3, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            nl.SetBounds(0, 0, inner.Width, 40); inner.Controls.Add(nl);
            _banlandPanel.Commit(40); Status("✓  banland is empty", AC.GRN); return;
        }

        if (bans.Count == 0)
        {
            var nl = new Label { Text = $"  no results for \"{query}\".", Font = AC.CON8, ForeColor = AC.FG3, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            nl.SetBounds(0, 0, inner.Width, 40); inner.Controls.Add(nl);
            _banlandPanel.Commit(40); return;
        }

        int pw = _banlandPanel.Width - 2;
        string countTxt = bans.Count == _cachedBans.Count
            ? $"  {bans.Count} banned entr{(bans.Count == 1 ? "y" : "ies")}                                              key  ·  user  ·  until  ·  reason"
            : $"  {bans.Count} of {_cachedBans.Count} matching \"{query}\"                                    key  ·  user  ·  until  ·  reason";
        var hdr = new Label { Text = countTxt, Font = AC.CON8B, ForeColor = AC.FG2, BackColor = Color.FromArgb(255, AC.BG2.R, AC.BG2.G, AC.BG2.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
        hdr.SetBounds(0, 0, pw, 20); inner.Controls.Add(hdr);

        int ly = 22;
        foreach (var b in bans)
        {
            string bCopy = b.Key;
            string user = string.IsNullOrEmpty(b.Username) ? "—" : b.Username;
            string reason = string.IsNullOrEmpty(b.BanReason) ? "no reason given" : b.BanReason;
            string until = b.IsLifetime ? "PERMANENT" : (DateTime.TryParse(b.BanExpiresAt, out var bExp) ? bExp.ToString("yyyy-MM-dd HH:mm") : b.BanExpiresAt);
            string bannedAt = DateTime.TryParse(b.BannedAt, out var bat) ? bat.ToString("yyyy-MM-dd HH:mm") : "unknown";
            bool perm = b.IsLifetime;

            var ac = UISettings.AccentColor;
            var rowBg = Color.FromArgb(255, Math.Min(255, 8 + ac.R / 10), Math.Min(255, 8 + ac.G / 10), Math.Min(255, 8 + ac.B / 10));
            var rowBorder = Color.FromArgb(255, Math.Min(255, 20 + ac.R / 6), Math.Min(255, 20 + ac.G / 6), Math.Min(255, 20 + ac.B / 6));
            var row = new Panel { BackColor = rowBg };
            row.SetBounds(0, ly, pw, 54);
            row.Paint += (s2, e2) =>
            {
                using var p = new Pen(rowBorder);
                e2.Graphics.DrawLine(p, 0, 0, row.Width, 0);
                e2.Graphics.DrawLine(p, 0, row.Height - 1, row.Width, row.Height - 1);
            };

            var line1 = new Label { Text = $"  {b.Key}   {user}   →  {until}", Font = AC.CON8B, ForeColor = perm ? UISettings.AccentBright : AC.ORG, BackColor = rowBg, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            line1.SetBounds(0, 2, pw - 100, 24); row.Controls.Add(line1);

            var unbanBtn = new ABtn("Unban", false); unbanBtn.SetBounds(pw - 96, 6, 78, 20); row.Controls.Add(unbanBtn);

            var line2 = new Label { Text = $"  reason: {reason}  ·  banned at: {bannedAt}", Font = AC.CON8, ForeColor = AC.FG3, BackColor = rowBg, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            line2.SetBounds(0, 28, pw - 100, 20); row.Controls.Add(line2);

            unbanBtn.Click += (s2, e2) =>
            {
                if (MessageBox.Show($"Unban {bCopy}?\nThis will restore the key to active status.", "Confirm Unban", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
                Status("unbanning...", AC.FG3);
                BG(() => { var (uOk, uErr) = AdminApi.UnbanKey(bCopy); UI(() => { Status(uOk ? "✓  unbanned: " + bCopy : "✗  " + uErr, uOk ? AC.GRN : AC.REDB); if (uOk) { _cachedBans.RemoveAll(x => x.Key == bCopy); FilterBanland(_banSearchBox?.Value ?? ""); } }); });
            };

            inner.Controls.Add(row);
            ly += 56;
        }

        _banlandPanel.Commit(ly + 4);
        Status($"✓  {_cachedBans.Count} banned entr{(_cachedBans.Count == 1 ? "y" : "ies")}", UISettings.AccentColor);
    }


    // ── ANNOUNCEMENTS TAB ─────────────────────────────────────────────────────

    void BuildAnnouncementsTab()
    {
        var body = _announcementsTab;
        int y = 14;
        const int PX = 20, W = 600;

        SectionHeader(body, "COMPOSE ANNOUNCEMENT", PX, ref y);
        var composePanel = CP(new Panel { BackColor = AC.BG1 });
        composePanel.SetBounds(PX, y, W, 370); composePanel.Paint += BorderPaint; body.Controls.Add(composePanel);

        // ── Scope selector ─────────────────────────────────────────────────────
        SL(composePanel, "SCOPE", AC.CON8B, AC.FG3, 10, 10);
        var globalBtn = new ABtn("  ● Global", false); globalBtn.SetBounds(10, 26, 120, 26); composePanel.Controls.Add(globalBtn);
        var localBtn = new ABtn("  ○ Local", false); localBtn.SetBounds(138, 26, 120, 26); composePanel.Controls.Add(localBtn);
        bool isGlobal = true;
        var scopeHint = new Label { Text = "sends to all active users", Font = AC.CON8, ForeColor = AC.FG3, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false };
        scopeHint.SetBounds(270, 30, 320, 18); composePanel.Controls.Add(scopeHint);

        // ── Target keys (local only) ───────────────────────────────────────────
        SL(composePanel, "TARGET KEYS  (one per line, local only)", AC.CON8B, AC.FG3, 10, 62);
        var targetBox = new RichTextBox
        {
            BackColor = AC.BG0,
            ForeColor = AC.FG2,
            BorderStyle = BorderStyle.FixedSingle,
            Font = AC.CON9,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            WordWrap = false,
            Enabled = false
        };
        targetBox.SetBounds(10, 78, W - 20, 44); composePanel.Controls.Add(targetBox);

        void UpdateScope(bool global)
        {
            isGlobal = global;
            globalBtn.Text = global ? "  ● Global" : "  ○ Global";
            localBtn.Text = global ? "  ○ Local" : "  ● Local";
            targetBox.Enabled = !global;
            targetBox.BackColor = global ? AC.BG0 : Color.FromArgb(16, 14, 14);
            scopeHint.Text = global ? "sends to all active users" : "sends only to the keys listed above";
            composePanel.Invalidate(true);
        }
        globalBtn.Click += (s, e) => UpdateScope(true);
        localBtn.Click += (s, e) => UpdateScope(false);

        // ── Sender + Subject (side by side) ───────────────────────────────────
        SL(composePanel, "SENDER NAME", AC.CON8B, AC.FG3, 10, 134);
        var senderBox = new ABox("e.g.  virinium team"); senderBox.SetBounds(10, 150, 288, 26); composePanel.Controls.Add(senderBox);

        SL(composePanel, "SUBJECT", AC.CON8B, AC.FG3, 308, 134);
        var subjectBox = new ABox("e.g.  important update"); subjectBox.SetBounds(308, 150, 282, 26); composePanel.Controls.Add(subjectBox);

        // ── Topic (full width) ────────────────────────────────────────────────
        SL(composePanel, "TOPIC", AC.CON8B, AC.FG3, 10, 186);
        var topicBox = new ABox("e.g.  maintenance  /  update  /  warning"); topicBox.SetBounds(10, 202, W - 20, 26); composePanel.Controls.Add(topicBox);

        // ── Message body (full width, taller) ─────────────────────────────────
        SL(composePanel, "MESSAGE BODY", AC.CON8B, AC.FG3, 10, 238);
        var messageBox = new RichTextBox
        {
            BackColor = AC.BG0,
            ForeColor = AC.FG,
            BorderStyle = BorderStyle.FixedSingle,
            Font = AC.CON9,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            WordWrap = true
        };
        messageBox.SetBounds(10, 254, W - 20, 84); composePanel.Controls.Add(messageBox);

        // ── Send button + result ───────────────────────────────────────────────
        var sendBtn = new ABtn("SEND ANNOUNCEMENT", true); sendBtn.SetBounds(10, 348, 220, 14); composePanel.Controls.Add(sendBtn);
        var sendResult = new Label { Text = "", Font = AC.CON8, ForeColor = AC.FG3, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false };
        sendResult.SetBounds(242, 350, W - 252, 16); composePanel.Controls.Add(sendResult);

        sendBtn.SetBounds(10, 346, 220, 16);

        sendBtn.Click += (s, e) =>
        {
            string sender = senderBox.Value;
            string subject = subjectBox.Value;
            string topic = topicBox.Value;
            string msg = messageBox.Text.Trim();
            if (string.IsNullOrEmpty(sender)) { sendResult.Text = "sender name required."; sendResult.ForeColor = AC.REDB; return; }
            if (string.IsNullOrEmpty(subject)) { sendResult.Text = "subject required."; sendResult.ForeColor = AC.REDB; return; }
            if (string.IsNullOrEmpty(msg)) { sendResult.Text = "message body required."; sendResult.ForeColor = AC.REDB; return; }

            string[] targetKeys = null;
            if (!isGlobal)
            {
                targetKeys = targetBox.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim()).Where(k => k.Length > 0).ToArray();
                if (targetKeys.Length == 0) { sendResult.Text = "add at least one target key for local."; sendResult.ForeColor = AC.REDB; return; }
            }

            var ann = new AnnouncementRecord
            {
                SenderName = sender,
                Subject = subject,
                Topic = topic,
                Message = msg,
                Scope = isGlobal ? "global" : "local",
                TargetKeys = targetKeys
            };

            sendResult.Text = "sending..."; sendResult.ForeColor = AC.FG3;
            Status("sending announcement...", AC.FG3);
            BG(() =>
            {
                var (ok, id, err) = AdminApi.SendAnnouncement(ann);
                UI(() =>
                {
                    if (ok)
                    {
                        sendResult.Text = $"✓  sent  (id: {id})"; sendResult.ForeColor = AC.GRN;
                        Status("✓  announcement sent", AC.GRN);
                        senderBox.SetValue(""); subjectBox.SetValue(""); topicBox.SetValue(""); messageBox.Text = ""; targetBox.Text = "";
                        BG(() => RefreshAnnouncements());
                    }
                    else
                    {
                        sendResult.Text = "✗  " + err; sendResult.ForeColor = AC.REDB;
                        Status("✗  failed to send: " + err, AC.REDB);
                    }
                });
            });
        };

        y += 382;

        // ── History ───────────────────────────────────────────────────────────
        var hdrPanel = new Panel { BackColor = Color.FromArgb(255, 8, 8, 8) };
        hdrPanel.SetBounds(PX, y, W, 26); body.Controls.Add(hdrPanel);

        var histTitleLbl = new Label { Font = AC.CON8B, ForeColor = UISettings.AccentColor, BackColor = Color.FromArgb(255, 8, 8, 8), AutoSize = false };
        histTitleLbl.SetBounds(0, 4, W - 100, 16);
        histTitleLbl.Paint += (s2, e2) =>
        {
            e2.Graphics.Clear(Color.FromArgb(255, 8, 8, 8));
            int tw = (int)e2.Graphics.MeasureString("SENT ANNOUNCEMENTS", AC.CON8B).Width + 8;
            e2.Graphics.DrawString("SENT ANNOUNCEMENTS", AC.CON8B, new SolidBrush(UISettings.AccentColor), 0, 2);
            using var p2 = new Pen(AC.BDR); e2.Graphics.DrawLine(p2, tw, 9, histTitleLbl.Width, 9);
        };
        hdrPanel.Controls.Add(histTitleLbl);

        var refreshAnnBtn = new ABtn("Refresh", false); refreshAnnBtn.SetBounds(W - 96, 2, 96, 22); hdrPanel.Controls.Add(refreshAnnBtn);
        refreshAnnBtn.Click += (s, e) => { Status("loading announcements...", AC.FG3); BG(() => RefreshAnnouncements()); };
        y += 34;

        _announcementsPanel = new NoScrollPanel { BackColor = AC.BG1 };
        _announcementsPanel.SetBounds(PX, y, W, 340); _announcementsPanel.Paint += BorderPaint; body.Controls.Add(_announcementsPanel);
        CP(_announcementsPanel);

        var ph2 = new Label { Text = "  press refresh to load announcements.", Font = AC.CON8, ForeColor = AC.FG3, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
        ph2.SetBounds(0, 0, W, 40); _announcementsPanel.Inner.Controls.Add(ph2);
        y += 350;

        body.AutoScrollMinSize = new Size(0, y + 20);
    }

    void RefreshAnnouncements()
    {
        var (ok, list, err) = AdminApi.GetAnnouncements();
        UI(() =>
        {
            if (!ok)
            {
                var inner = _announcementsPanel.Inner; inner.Controls.Clear();
                var el = new Label { Text = "  ✗  " + err, Font = AC.CON8, ForeColor = AC.REDB, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
                el.SetBounds(0, 0, inner.Width, 30); inner.Controls.Add(el);
                _announcementsPanel.Commit(30);
                Status("✗  failed to load announcements", AC.REDB); return;
            }
            _cachedAnnouncements = list;
            RenderAnnouncements(list);
        });
    }

    void RenderAnnouncements(List<AnnouncementRecord> list)
    {
        var inner = _announcementsPanel.Inner;
        inner.Controls.Clear();
        int pw = _announcementsPanel.Width - 2;

        if (list.Count == 0)
        {
            var nl = new Label { Text = "  no announcements found.", Font = AC.CON8, ForeColor = AC.FG3, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            nl.SetBounds(0, 0, pw, 40); inner.Controls.Add(nl);
            _announcementsPanel.Commit(40); Status("✓  no announcements", AC.GRN); return;
        }

        string countTxt = $"  {list.Count} announcement{(list.Count == 1 ? "" : "s")}                                          scope  ·  sender  ·  subject  ·  topic";
        var hdr = new Label { Text = countTxt, Font = AC.CON8B, ForeColor = AC.FG2, BackColor = Color.FromArgb(255, AC.BG2.R, AC.BG2.G, AC.BG2.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
        hdr.SetBounds(0, 0, pw, 20); inner.Controls.Add(hdr);

        int ly = 22;
        foreach (var ann in list)
        {
            var annCopy = ann;
            bool isGlobal = (ann.Scope ?? "global") == "global";
            string dateStr = DateTime.TryParse(ann.CreatedAt, out var dt) ? dt.ToString("yyyy-MM-dd HH:mm") : ann.CreatedAt;
            string targetSummary = isGlobal ? "ALL USERS" : (ann.TargetKeys != null && ann.TargetKeys.Length > 0 ? $"{ann.TargetKeys.Length} key{(ann.TargetKeys.Length == 1 ? "" : "s")}" : "local");
            Color scopeColor = isGlobal ? UISettings.AccentBright : AC.ORG;

            var ac2 = UISettings.AccentColor;
            var rowBg = Color.FromArgb(255, Math.Min(255, 8 + ac2.R / 12), Math.Min(255, 8 + ac2.G / 12), Math.Min(255, 8 + ac2.B / 12));
            var rowBorder = Color.FromArgb(255, Math.Min(255, 20 + ac2.R / 6), Math.Min(255, 20 + ac2.G / 6), Math.Min(255, 20 + ac2.B / 6));
            var row = new Panel { BackColor = rowBg };
            row.SetBounds(0, ly, pw, 70);
            row.Paint += (s2, e2) =>
            {
                using var p2 = new Pen(rowBorder);
                e2.Graphics.DrawLine(p2, 0, 0, row.Width, 0);
                e2.Graphics.DrawLine(p2, 0, row.Height - 1, row.Width, row.Height - 1);
            };

            var line1 = new Label { Text = $"  [{(isGlobal ? "GLOBAL" : "LOCAL")}]  {ann.Subject}", Font = AC.CON8B, ForeColor = scopeColor, BackColor = rowBg, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            line1.SetBounds(0, 2, pw - 200, 20); row.Controls.Add(line1);

            var line1b = new Label { Text = $"  from: {ann.SenderName}", Font = AC.CON8, ForeColor = AC.FG2, BackColor = rowBg, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            line1b.SetBounds(0, 22, pw - 200, 16); row.Controls.Add(line1b);

            var line2 = new Label { Text = $"  topic: {ann.Topic}  ·  target: {targetSummary}  ·  sent: {dateStr}", Font = AC.CON8, ForeColor = AC.FG3, BackColor = rowBg, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            line2.SetBounds(0, 40, pw - 200, 16); row.Controls.Add(line2);

            string msgPreview = (ann.Message ?? "").Replace('\n', ' ').Replace('\r', ' ');
            if (msgPreview.Length > 80) msgPreview = msgPreview.Substring(0, 77) + "…";
            var line3 = new Label { Text = $"  {msgPreview}", Font = AC.CON8, ForeColor = AC.FG3, BackColor = rowBg, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            line3.SetBounds(0, 54, pw - 200, 14); row.Controls.Add(line3);

            var editBtn = new ABtn("Edit", false); editBtn.SetBounds(pw - 194, 14, 88, 22); row.Controls.Add(editBtn);
            var deleteBtn = new ABtn("Delete", true); deleteBtn.SetBounds(pw - 100, 14, 88, 22); row.Controls.Add(deleteBtn);

            editBtn.Click += (s2, e2) =>
            {
                var dlg = new AnnouncementEditDialog(annCopy);
                dlg.ShowDialog(this);
                if (!dlg.Saved || dlg.Result == null) return;
                var result = dlg.Result;
                Status("saving...", AC.FG3);
                BG(() =>
                {
                    var (ok, err) = AdminApi.UpdateAnnouncement(result);
                    UI(() =>
                    {
                        Status(ok ? "✓  announcement updated" : "✗  " + err, ok ? AC.GRN : AC.REDB);
                        if (ok) BG(() => RefreshAnnouncements());
                    });
                });
            };

            deleteBtn.Click += (s2, e2) =>
            {
                if (MessageBox.Show($"Delete this announcement?\n\n\"{ann.Subject}\" from {ann.SenderName}", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                Status("deleting...", AC.FG3);
                BG(() =>
                {
                    var (ok, err) = AdminApi.DeleteAnnouncement(annCopy.Id);
                    UI(() =>
                    {
                        Status(ok ? "✓  announcement deleted" : "✗  " + err, ok ? AC.GRN : AC.REDB);
                        if (ok) { _cachedAnnouncements.RemoveAll(a => a.Id == annCopy.Id); RenderAnnouncements(_cachedAnnouncements); }
                    });
                });
            };

            inner.Controls.Add(row);
            ly += 72;
        }

        _announcementsPanel.Commit(ly + 4);
        Status($"✓  {list.Count} announcement{(list.Count == 1 ? "" : "s")} loaded", UISettings.AccentColor);
    }


    // ── CONTROL TAB ───────────────────────────────────────────────────────────

    void BuildControlTab()
    {
        var body = _controlTab;
        int y = 14;
        const int PX = 20, W = 600;

        // ── Warning Notification ──────────────────────────────────────────────
        SectionHeader(body, "WARNING NOTIFICATION  (viri: warning popup)", PX, ref y);
        var warnPanel = CP(new Panel { BackColor = AC.BG1 }); warnPanel.SetBounds(PX, y, W, 120); warnPanel.Paint += BorderPaint; body.Controls.Add(warnPanel);

        var warnChk = new ACheck("Show warning popup on .exe open  (stays until cleared)");
        warnChk.SetBounds(10, 10, 560, 20); warnPanel.Controls.Add(warnChk);
        SL(warnPanel, "MESSAGE  (shown in the notification)", AC.CON8B, AC.FG3, 10, 36);
        var warnMsgBox = new RichTextBox
        {
            BackColor = AC.BG0,
            ForeColor = AC.FG,
            BorderStyle = BorderStyle.FixedSingle,
            Font = AC.CON9,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            WordWrap = true
        };
        warnMsgBox.SetBounds(10, 52, W - 20, 46); warnPanel.Controls.Add(warnMsgBox);
        y += 132;

        // ── Global Pause ──────────────────────────────────────────────────────
        SectionHeader(body, "GLOBAL PAUSE  (disables all macros for all users)", PX, ref y);
        var pausePanel = CP(new Panel { BackColor = AC.BG1 }); pausePanel.SetBounds(PX, y, W, 52); pausePanel.Paint += BorderPaint; body.Controls.Add(pausePanel);

        var pauseAllChk = new ACheck("Pause ALL keys  — disables macros globally until unchecked");
        pauseAllChk.SetBounds(10, 16, 440, 20); pausePanel.Controls.Add(pauseAllChk);
        y += 64;

        // ── Apply Changes ─────────────────────────────────────────────────────
        SectionHeader(body, "APPLY", PX, ref y);
        var applyPanel = CP(new Panel { BackColor = AC.BG1 }); applyPanel.SetBounds(PX, y, W, 42); applyPanel.Paint += BorderPaint; body.Controls.Add(applyPanel);

        var applyBtn = new ABtn("PUSH CHANGES", true); applyBtn.SetBounds(10, 8, 280, 26); applyPanel.Controls.Add(applyBtn);
        var applyResult = new Label { Text = "", Font = AC.CON8, ForeColor = AC.FG3, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false };
        applyResult.SetBounds(298, 12, W - 308, 18); applyPanel.Controls.Add(applyResult);
        y += 54;

        applyBtn.Click += (s, e) =>
        {
            bool globalPauseNowOn = pauseAllChk.Checked;
            bool wasOn = _controlState.AllKeysPaused;

            // Determine AllPausedAt: set timestamp when turning on, clear when turning off
            string? newAllPausedAt = _controlState.AllPausedAt;
            if (globalPauseNowOn && !wasOn)
                newAllPausedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            else if (!globalPauseNowOn)
                newAllPausedAt = null;

            var ctrl = new SystemControlRecord
            {
                WarningEnabled = warnChk.Checked,
                WarningMessage = warnMsgBox.Text.Trim(),
                AllKeysPaused = globalPauseNowOn,
                AllPausedAt = newAllPausedAt,
                PausedKeys = _controlState.PausedKeys,
                LockedKeys = _controlState.LockedKeys,
                KeyPausedAt = _controlState.KeyPausedAt
            };
            applyResult.Text = "pushing..."; applyResult.ForeColor = AC.FG3;
            Status("updating system control...", AC.FG3);

            // If turning global pause OFF, compute elapsed and extend all non-lifetime keys
            bool doExtend = wasOn && !globalPauseNowOn && !string.IsNullOrEmpty(_controlState.AllPausedAt);
            long extraSecs = 0;
            if (doExtend && DateTime.TryParse(_controlState.AllPausedAt, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out DateTime pausedDt))
                extraSecs = (long)(DateTime.UtcNow - pausedDt).TotalSeconds;

            var allKeys = _controlState.PausedKeys.ToList();
            long capturedExtra = extraSecs;

            BG(() =>
            {
                var (ok, err) = AdminApi.UpdateSystemControl(ctrl);
                if (ok && doExtend && capturedExtra > 0)
                    foreach (var k2 in allKeys)
                        AdminApi.ExtendExpiry(k2, capturedExtra);
                UI(() =>
                {
                    string extMsg = (doExtend && capturedExtra > 0) ? $"  (+{DurationHelper.Describe(capturedExtra)} added to all)" : "";
                    applyResult.Text = ok ? $"✓  pushed{extMsg}" : "✗  " + err;
                    applyResult.ForeColor = ok ? AC.GRN : AC.REDB;
                    Status(ok ? $"✓  control state updated{extMsg}" : "✗  " + err, ok ? AC.GRN : AC.REDB);
                    if (ok) { _controlState = ctrl; RenderControlKeys(body, PX, W); }
                });
            });
        };

        // ── Per-key Pause / Lock ──────────────────────────────────────────────
        SectionHeader(body, "PER-KEY CONTROL", PX, ref y);
        var keyCtrlPanel = CP(new Panel { BackColor = AC.BG1 }); keyCtrlPanel.SetBounds(PX, y, W, 86); keyCtrlPanel.Paint += BorderPaint; body.Controls.Add(keyCtrlPanel);

        SL(keyCtrlPanel, "Enter a key then choose an action.  Pause = disable macros.  Lock = boot + warning popup.  Release = undo.", AC.CON8, AC.FG3, 10, 8);
        var perKeyBox = new ABox("VIRI-XXXX-XXXX-XXXX"); perKeyBox.SetBounds(10, 24, W - 20, 26); keyCtrlPanel.Controls.Add(perKeyBox);

        var pauseKeyBtn = new ABtn("Pause Key", false); pauseKeyBtn.SetBounds(10, 56, 130, 22); keyCtrlPanel.Controls.Add(pauseKeyBtn);
        var lockKeyBtn = new ABtn("Lock Key", true); lockKeyBtn.SetBounds(148, 56, 130, 22); keyCtrlPanel.Controls.Add(lockKeyBtn);
        var releaseKeyBtn = new ABtn("Release Key", false); releaseKeyBtn.SetBounds(286, 56, 130, 22); keyCtrlPanel.Controls.Add(releaseKeyBtn);
        var keyCtrlResult = new Label { Text = "", Font = AC.CON8, ForeColor = AC.FG3, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false };
        keyCtrlResult.SetBounds(424, 60, W - 434, 16); keyCtrlPanel.Controls.Add(keyCtrlResult);

        pauseKeyBtn.Click += (s, e) =>
        {
            string k = perKeyBox.Value.Trim().ToUpper(); if (k == "") { keyCtrlResult.Text = "enter a key"; keyCtrlResult.ForeColor = AC.REDB; return; }
            var pk = _controlState.PausedKeys.ToList(); if (!pk.Contains(k)) pk.Add(k);
            var lk = _controlState.LockedKeys.Where(x => x != k).ToArray();
            var kpa = new Dictionary<string, string>(_controlState.KeyPausedAt);
            if (!kpa.ContainsKey(k)) kpa[k] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var ctrl = new SystemControlRecord { WarningEnabled = _controlState.WarningEnabled, WarningMessage = _controlState.WarningMessage, AllKeysPaused = _controlState.AllKeysPaused, AllPausedAt = _controlState.AllPausedAt, PausedKeys = pk.ToArray(), LockedKeys = lk, KeyPausedAt = kpa };
            PushControl(ctrl, keyCtrlResult, $"✓  {k} paused");
        };

        lockKeyBtn.Click += (s, e) =>
        {
            string k = perKeyBox.Value.Trim().ToUpper(); if (k == "") { keyCtrlResult.Text = "enter a key"; keyCtrlResult.ForeColor = AC.REDB; return; }
            var lk = _controlState.LockedKeys.ToList(); if (!lk.Contains(k)) lk.Add(k);
            var pk = _controlState.PausedKeys.Where(x => x != k).ToArray();
            // Remove from pause tracking if moving to locked (lock doesn't extend expiry)
            var kpa = new Dictionary<string, string>(_controlState.KeyPausedAt);
            kpa.Remove(k);
            var ctrl = new SystemControlRecord { WarningEnabled = _controlState.WarningEnabled, WarningMessage = _controlState.WarningMessage, AllKeysPaused = _controlState.AllKeysPaused, AllPausedAt = _controlState.AllPausedAt, PausedKeys = pk, LockedKeys = lk.ToArray(), KeyPausedAt = kpa };
            PushControl(ctrl, keyCtrlResult, $"✓  {k} locked");
        };

        releaseKeyBtn.Click += (s, e) =>
        {
            string k = perKeyBox.Value.Trim().ToUpper(); if (k == "") { keyCtrlResult.Text = "enter a key"; keyCtrlResult.ForeColor = AC.REDB; return; }
            var pk = _controlState.PausedKeys.Where(x => x != k).ToArray();
            var lk = _controlState.LockedKeys.Where(x => x != k).ToArray();
            var kpa = new Dictionary<string, string>(_controlState.KeyPausedAt);

            // Compute how long this key was paused and extend its expiry
            long extraSecs = 0;
            if (kpa.TryGetValue(k, out string? pausedAt) && !string.IsNullOrEmpty(pausedAt))
            {
                if (DateTime.TryParse(pausedAt, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime pausedDt))
                    extraSecs = (long)(DateTime.UtcNow - pausedDt).TotalSeconds;
            }
            kpa.Remove(k);

            var ctrl = new SystemControlRecord { WarningEnabled = _controlState.WarningEnabled, WarningMessage = _controlState.WarningMessage, AllKeysPaused = _controlState.AllKeysPaused, AllPausedAt = _controlState.AllPausedAt, PausedKeys = pk, LockedKeys = lk, KeyPausedAt = kpa };

            keyCtrlResult.Text = "releasing..."; keyCtrlResult.ForeColor = AC.FG3;
            Status("releasing " + k + "...", AC.FG3);
            long capturedExtra = extraSecs; string capturedKey = k;
            BG(() =>
            {
                var (ok, err) = AdminApi.UpdateSystemControl(ctrl);
                if (ok && capturedExtra > 0)
                    AdminApi.ExtendExpiry(capturedKey, capturedExtra);
                UI(() =>
                {
                    string extMsg = capturedExtra > 0 ? $"  (+{DurationHelper.Describe(capturedExtra)} added)" : "";
                    keyCtrlResult.Text = ok ? $"✓  {capturedKey} released{extMsg}" : "✗  " + err;
                    keyCtrlResult.ForeColor = ok ? AC.GRN : AC.REDB;
                    Status(ok ? $"✓  {capturedKey} released{extMsg}" : "✗  " + err, ok ? AC.GRN : AC.REDB);
                    if (ok) { _controlState = ctrl; _onControlStateLoaded?.Invoke(); }
                });
            });
        };
        y += 98;

        // ── Current State ─────────────────────────────────────────────────────
        SectionHeader(body, "CURRENT STATE", PX, ref y);
        var stateHdr = new Panel { BackColor = Color.FromArgb(255, 8, 8, 8) }; stateHdr.SetBounds(PX, y, W, 24); body.Controls.Add(stateHdr);
        var refreshCtrlBtn = new ABtn("Refresh", false); refreshCtrlBtn.SetBounds(W - 96, 2, 96, 20); stateHdr.Controls.Add(refreshCtrlBtn);
        refreshCtrlBtn.Click += (s, e) => BG(() => RefreshControlState());
        y += 30;

        _ctrlStateLbl = new Label { Text = "  press refresh or switch to this tab to load.", Font = AC.CON8, ForeColor = AC.FG3, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false, TextAlign = ContentAlignment.TopLeft };
        _ctrlStateLbl.SetBounds(PX, y, W, 120); body.Controls.Add(_ctrlStateLbl);
        y += 130;

        // Store refs so warnChk / pauseAllChk stay in sync when state loads
        HandleCreated += (s, e) =>
        {
            void SyncFromState()
            {
                warnChk.Checked = _controlState.WarningEnabled;
                warnMsgBox.Text = _controlState.WarningMessage;
                pauseAllChk.Checked = _controlState.AllKeysPaused;
                RenderControlKeys(body, PX, W);
            }
            // Hook: every time RefreshControlState finishes it calls SyncFromState
            // We do this via a stored action delegate
            _onControlStateLoaded = SyncFromState;
        };

        body.AutoScrollMinSize = new Size(0, y + 20);
    }

    Action? _onControlStateLoaded;
    Panel? _ctrlKeysPanel;

    void PushControl(SystemControlRecord ctrl, Label resultLbl, string successMsg)
    {
        resultLbl.Text = "pushing..."; resultLbl.ForeColor = AC.FG3;
        Status("updating control...", AC.FG3);
        BG(() =>
        {
            var (ok, err) = AdminApi.UpdateSystemControl(ctrl);
            UI(() =>
            {
                resultLbl.Text = ok ? successMsg : "✗  " + err;
                resultLbl.ForeColor = ok ? AC.GRN : AC.REDB;
                Status(ok ? "✓  control updated" : "✗  " + err, ok ? AC.GRN : AC.REDB);
                if (ok) { _controlState = ctrl; _onControlStateLoaded?.Invoke(); }
            });
        });
    }

    void RefreshControlState()
    {
        var (ok, ctrl, err) = AdminApi.GetSystemControl();
        UI(() =>
        {
            if (!ok)
            {
                if (_ctrlStateLbl != null) { _ctrlStateLbl.Text = "  ✗  " + err; _ctrlStateLbl.ForeColor = AC.REDB; }
                Status("✗  failed to load control state: " + err, AC.REDB); return;
            }
            _controlState = ctrl;
            _onControlStateLoaded?.Invoke();
            if (_ctrlStateLbl != null)
            {
                string pk = ctrl.PausedKeys.Length > 0 ? string.Join(", ", ctrl.PausedKeys) : "none";
                string lk = ctrl.LockedKeys.Length > 0 ? string.Join(", ", ctrl.LockedKeys) : "none";
                _ctrlStateLbl.Text =
                    $"  all_keys_paused:  {ctrl.AllKeysPaused}\n" +
                    $"  warning_enabled:  {ctrl.WarningEnabled}\n" +
                    $"  warning_message:  {(string.IsNullOrEmpty(ctrl.WarningMessage) ? "(empty)" : ctrl.WarningMessage.Replace("\n", " ").Substring(0, Math.Min(60, ctrl.WarningMessage.Length)))}\n" +
                    $"  paused_keys:      {pk}\n" +
                    $"  locked_keys:      {lk}";
                _ctrlStateLbl.ForeColor = AC.FG2;
            }
            Status("✓  control state loaded", AC.GRN);
        });
    }

    void RenderControlKeys(ScrollFreePanel body, int PX, int W)
    {
        // This is a lightweight redraw placeholder — keys are shown in _ctrlStateLbl
        // A more elaborate NoScrollPanel list can be added here if desired
    }


    void SectionHeader(Control parent, string title, int x, ref int y)
    {
        var hdr = new AHeader(title);
        hdr.SetBounds(x, y, parent.Width - x * 2, 18);
        parent.Controls.Add(hdr);
        y += 24;
    }

    void BorderPaint(object? s, PaintEventArgs e) { var c = s as Control; if (c == null) return; using var p = new Pen(AC.BDR); e.Graphics.DrawRectangle(p, 0, 0, c.Width - 1, c.Height - 1); }

    static void SL(Control parent, string text, Font font, Color color, int x, int y)
    { var bg = parent.BackColor; var l = new Label { Text = text, Font = font, ForeColor = color, BackColor = Color.FromArgb(255, bg.R, bg.G, bg.B), AutoSize = true }; l.SetBounds(x, y, 0, 0); parent.Controls.Add(l); }

    void AddLog(KeyLogEntry entry)
    {
        if (_logPanel == null) return;
        _log.Insert(0, entry); _logPanel.Controls.Clear(); int ly = 6;
        foreach (var e in _log)
        {
            var row = new Label { Text = $"  {e.Time}   {e.Key}   {(string.IsNullOrEmpty(e.Username) ? "—" : e.Username)}   {e.Duration}", Font = AC.CON8, ForeColor = e.Ok ? AC.GRN : AC.REDB, BackColor = Color.FromArgb(255, AC.BG1.R, AC.BG1.G, AC.BG1.B), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            row.SetBounds(0, ly, _logPanel.Width, 20); _logPanel.Controls.Add(row); ly += 22;
        }
        _logPanel.Height = Math.Max(120, ly + 8);
    }

    void Status(string msg, Color col) { if (InvokeRequired) { BeginInvoke(new Action(() => Status(msg, col))); return; } if (_statusLbl == null) return; _statusLbl.Text = "  " + msg; _statusLbl.ForeColor = col; }
    void BG(Action a) => System.Threading.ThreadPool.QueueUserWorkItem(_ => { try { a(); } catch (Exception ex) { UI(() => Status("✗  " + ex.Message, AC.REDB)); } });
    void UI(Action a) => BeginInvoke(a);
}


class AnnouncementEditDialog : Form
{
    public bool Saved { get; private set; }
    public AnnouncementRecord? Result { get; private set; }

    public AnnouncementEditDialog(AnnouncementRecord existing)
    {
        FormBorderStyle = FormBorderStyle.None;
        BackColor = AC.BG0;
        ClientSize = new Size(580, 430);
        StartPosition = FormStartPosition.CenterParent;
        TopMost = true; DoubleBuffered = true;
        Build(existing);
        KeyPreview = true;
        KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
        Paint += (s, e) => { using var bp = new Pen(AC.BDR); e.Graphics.DrawRectangle(bp, 0, 0, Width - 1, Height - 1); };
    }

    void Build(AnnouncementRecord existing)
    {
        Controls.Add(new Panel { BackColor = UISettings.AccentColor, Dock = DockStyle.Top, Height = 2 });

        var tb = new Panel { BackColor = AC.BG1, Dock = DockStyle.Top, Height = 34 };
        Point drag = Point.Empty;
        tb.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) drag = e.Location; };
        tb.MouseMove += (s, e) => { if (e.Button == MouseButtons.Left) { var sp = tb.PointToScreen(e.Location); Location = new Point(sp.X - drag.X, sp.Y - drag.Y); } };
        SL(tb, "▌", AC.CON12B, UISettings.AccentColor, 10, 5);
        SL(tb, "EDIT ANNOUNCEMENT", AC.CON10B, UISettings.AccentColor, 28, 7);
        SL(tb, existing.Id, AC.CON8, AC.FG3, 28, 20);
        var closeBtn = new CloseBtn(); closeBtn.Dock = DockStyle.Right; closeBtn.Click += (s, e) => Close(); tb.Controls.Add(closeBtn);
        Controls.Add(tb);

        int y = 48, px = 16, w = ClientSize.Width - 32;

        // Scope
        SL(this, "SCOPE", AC.CON8B, AC.FG3, px, y); y += 17;
        bool isGlobal = (existing.Scope ?? "global") == "global";
        var globalBtn = new ABtn(isGlobal ? "  ● Global" : "  ○ Global", false); globalBtn.SetBounds(px, y, 120, 26); Controls.Add(globalBtn);
        var localBtn = new ABtn(isGlobal ? "  ○ Local" : "  ● Local", false); localBtn.SetBounds(px + 128, y, 120, 26); Controls.Add(localBtn);
        var scopeStore = new System.Windows.Forms.Label { Text = existing.Scope ?? "global", Visible = false }; Controls.Add(scopeStore);
        globalBtn.Click += (s, e) => { scopeStore.Text = "global"; globalBtn.Text = "  ● Global"; localBtn.Text = "  ○ Local"; };
        localBtn.Click += (s, e) => { scopeStore.Text = "local"; globalBtn.Text = "  ○ Global"; localBtn.Text = "  ● Local"; };
        y += 36;

        // Target keys
        SL(this, "TARGET KEYS  (one per line — local only)", AC.CON8B, AC.FG3, px, y); y += 17;
        var targetBox = new RichTextBox
        {
            BackColor = AC.BG0,
            ForeColor = AC.FG2,
            BorderStyle = BorderStyle.FixedSingle,
            Font = AC.CON9,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            WordWrap = false,
            Text = existing.TargetKeys != null ? string.Join("\n", existing.TargetKeys) : ""
        };
        targetBox.SetBounds(px, y, w, 44); Controls.Add(targetBox); y += 54;

        // Sender + Subject side by side
        SL(this, "SENDER NAME", AC.CON8B, AC.FG3, px, y); y += 17;
        var senderBox = new ABox("sender name"); senderBox.SetBounds(px, y, (w - 8) / 2, 26); senderBox.SetValue(existing.SenderName); Controls.Add(senderBox);
        SL(this, "SUBJECT", AC.CON8B, AC.FG3, px + (w - 8) / 2 + 8, y - 17);
        var subjectBox = new ABox("subject"); subjectBox.SetBounds(px + (w - 8) / 2 + 8, y, (w - 8) / 2, 26); subjectBox.SetValue(existing.Subject); Controls.Add(subjectBox);
        y += 36;

        // Topic full width
        SL(this, "TOPIC", AC.CON8B, AC.FG3, px, y); y += 17;
        var topicBox = new ABox("topic"); topicBox.SetBounds(px, y, w, 26); topicBox.SetValue(existing.Topic); Controls.Add(topicBox); y += 36;

        // Message body full width, taller
        SL(this, "MESSAGE BODY", AC.CON8B, AC.FG3, px, y); y += 17;
        var messageBox = new RichTextBox
        {
            BackColor = AC.BG0,
            ForeColor = AC.FG,
            BorderStyle = BorderStyle.FixedSingle,
            Font = AC.CON9,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            WordWrap = true,
            Text = existing.Message ?? ""
        };
        messageBox.SetBounds(px, y, w, 80); Controls.Add(messageBox); y += 90;

        var saveBtn = new ABtn("SAVE CHANGES", true); saveBtn.SetBounds(px, y, 160, 28); Controls.Add(saveBtn);
        var cancelBtn = new ABtn("Cancel", false); cancelBtn.SetBounds(px + 168, y, 80, 28); Controls.Add(cancelBtn);
        var errLbl = new Label { Text = "", Font = AC.CON8, ForeColor = AC.REDB, BackColor = Color.Transparent, AutoSize = true };
        errLbl.SetBounds(px + 256, y + 6, 300, 16); Controls.Add(errLbl);

        saveBtn.Click += (s, e) =>
        {
            if (string.IsNullOrEmpty(senderBox.Value)) { errLbl.Text = "sender name required."; return; }
            if (string.IsNullOrEmpty(subjectBox.Value)) { errLbl.Text = "subject required."; return; }
            if (string.IsNullOrEmpty(messageBox.Text.Trim())) { errLbl.Text = "message body required."; return; }
            string[] keys = null;
            if (scopeStore.Text == "local")
            {
                keys = targetBox.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim()).Where(k => k.Length > 0).ToArray();
            }
            Result = new AnnouncementRecord
            {
                Id = existing.Id,
                SenderName = senderBox.Value,
                Subject = subjectBox.Value,
                Topic = topicBox.Value,
                Message = messageBox.Text.Trim(),
                Scope = scopeStore.Text,
                TargetKeys = keys
            };
            Saved = true;
            Close();
        };
        cancelBtn.Click += (s, e) => Close();
    }

    static void SL(Control p, string txt, Font f, Color c, int x, int y)
    { var l = new Label { Text = txt, Font = f, ForeColor = c, BackColor = Color.Transparent, AutoSize = true }; l.SetBounds(x, y, 0, 0); p.Controls.Add(l); }
}


static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new AdminForm());
    }
}