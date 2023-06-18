using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace QuickFullscreenBrowser;

public partial class Form1 : Form
{
    readonly WebView2 _WebView;
    bool _EnableGoLinks;

    public Form1()
    {
        InitializeComponent();

        _WebView = new()
        {
            Dock = DockStyle.Fill
        };
        Controls.Add(_WebView);

        _WebView.NavigationStarting += NavigationStarting;
        _WebView.NavigationCompleted += NavigationCompleted;
    }

    private void NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (e.Uri.StartsWith("go:"))
        {
            if (_EnableGoLinks)
            {
                string? goUrl = null;
                if (e.Uri == "go://gnu")
                {
                    goUrl = "https://www.gnu.org/licenses";
                }
                else if (e.Uri == "go://miff")
                {
                    goUrl = "https://miffthefox.info/";
                }
                else if (e.Uri == "go://close")
                {
                    Close();
                }

                if (!string.IsNullOrEmpty(goUrl))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = goUrl,
                        UseShellExecute = true
                    });
                }
            }
            e.Cancel = true;
        }
    }

    private void NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        Text = _WebView.CoreWebView2.DocumentTitle;
    }

    private async void Form1_Load(object sender, EventArgs e)
    {
        //_WebView.Source = new("https://www.miffthefox.info/");
        var parameters = new Dictionary<string, string>();

        foreach (string arg in Environment.GetCommandLineArgs())
        {
            if (arg.StartsWith('/'))
            {
                int i = arg.IndexOf('=');
                if (i == -1)
                {
                    parameters[arg[1..]] = arg[1..];
                }
                else
                {
                    parameters[arg[1..i].ToLowerInvariant()] = arg[(i + 1)..];
                }
            }
        }

        int monitorX = parameters.ContainsKey("x") && int.TryParse(parameters["x"], out int n) ? n : 10;
        int monitorY = parameters.ContainsKey("y") && int.TryParse(parameters["y"], out n) ? n : 10;

        Bounds = Screen.FromPoint(new Point(monitorX, monitorY)).Bounds;
        WindowState = FormWindowState.Maximized;
        TopMost = parameters.ContainsKey("topmost");

        await _WebView.EnsureCoreWebView2Async();
        if (parameters.TryGetValue("url", out string? url))
        {
            _EnableGoLinks = false;
            _WebView.Source = new Uri(url);
        }
        else
        {
            _EnableGoLinks = true;
            _WebView.NavigateToString(Properties.Resources.CommandLineHelp);
        }
    }
}
