using System.CodeDom.Compiler;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Uri = System.Uri;

public partial class ImageDownloader   : GodotObject {
	GDCSHTTPClient client;

	public Task ActiveTask { get; set; }
	public string Tag { get; set; }

	private string sUrl;
	private string sRedirected;
	private string sOutPath;
	private bool bIsRedirected;
	private Uri uUri;

	public string Url { get { return sUrl; }}

	public ImageDownloader(string url, string outPath, string tag = "") {
		sUrl = url;
		uUri = new Uri(url);
		sOutPath = outPath;
		bIsRedirected = false;
		client = new GDCSHTTPClient();
		Tag = tag;
	}

	public ImageDownloader(Uri uri, string outPath) {
		sUrl = uri.OriginalString;
		uUri = uri;
		sOutPath = outPath;
		bIsRedirected = false;
		client = new GDCSHTTPClient();
	}

	public async Task<bool> StartDownload() {
		if (bIsRedirected)
			uUri = new Uri(sRedirected);
		
		if (CentralStore.Settings.UseProxy)
			client.SetProxy(CentralStore.Settings.ProxyHost, CentralStore.Settings.ProxyPort, uUri.Scheme == "https");
		else
			client.ClearProxy();
		
		Task<GDCSHTTPClient.Status> cres = client.StartClient(uUri.Host, uUri.Port, (uUri.Scheme == "https"));

		while (!cres.IsCompleted)
			await this.IdleFrame();
		
		if (!client.SuccessConnect(cres.Result))
			return false;
		
		var tresult = client.MakeRequest(uUri.PathAndQuery);
		while (!tresult.IsCompleted)
			await this.IdleFrame();
		
		HTTPResponse result = tresult.Result;
		client.Close();
		Array<int> redirect_codes = new Array<int> { 301, 302, 303, 307, 308 };
		
		if (redirect_codes.IndexOf(result.ResponseCode) >= 0) {
			bIsRedirected = true;
			if (result.Headers.ContainsKey("Location"))
				sRedirected = (string)result.Headers["Location"];
			else if (result.Headers.ContainsKey("location"))
				sRedirected = (string)result.Headers["location"];
			else
				GD.Print($"Fatal Error, Location header field not found.");
			Task<bool> recurse = StartDownload();
			while (!recurse.IsCompleted)
				await this.IdleFrame();
			return recurse.Result;
		}

		if (result.ResponseCode != 200) {
			return false;
		}

		if (result == null || result.BodyRaw == null)
			return false;
		try {
			System.IO.File.WriteAllBytes(sOutPath, result.BodyRaw);
		} catch (System.Exception ex) {
			GD.Print($"Failed to write file {sOutPath}, Error: {ex.Message}");
			return false;
		}

		return true;
	}
}