using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using HttpClient = System.Net.Http.HttpClient;
using HttpMethod = System.Net.Http.HttpMethod;

public partial class GDCSHTTPClient   : Node {
	[Signal]
	public delegate void chunk_receivedEventHandler(int size);

	[Signal]
	public delegate void headers_receivedEventHandler(Godot.Collections.Dictionary headers);

	[Signal]
	public delegate void request_completedEventHandler();

	// Godot 4 removed the HTTPClient class, so this enum replaces GDCSHTTPClient.Status.
	public enum Status {
		Connected,
		CantResolve,
		CantConnect,
		ConnectionError,
		SslHandshakeError,
		Requesting,
		Body
	}

	private HttpClient client;
	private HTTPResponse lastResponse;
	private string sHost;
	private string sProperName;
	private bool bUseSSL;
	private bool bCancelled;

	private static readonly TimeSpan RequestTimeout = TimeSpan.FromMinutes(10);

	public GDCSHTTPClient() {
		client = CreateHttpClient();
	}

	private static HttpClient CreateHttpClient() {
		var handler = new HttpClientHandler() {
			AllowAutoRedirect = false,
			AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
		};
		var httpClient = new HttpClient(handler);
		httpClient.Timeout = RequestTimeout;
		return httpClient;
	}

	public HTTPResponse LastResponse {
		get {
			return lastResponse;
		}
	}

	public static string GetUserAgent() {
		return $"User-Agent: Godot-Manager/{VERSION.GodotManager}-{VERSION.Channel} ({Platform.OperatingSystem})";
	}

	private void ApplyDefaultHeaders(HttpRequestMessage request) {
		request.Headers.TryAddWithoutValidation("Accept", "*/*");
		request.Headers.TryAddWithoutValidation("User-Agent", GetUserAgent());
	}

	private string BuildUrl(string path) {
		string scheme = bUseSSL ? "https" : "http";
		return $"{scheme}://{sHost}{path}";
	}

	private static Godot.Collections.Dictionary BuildHeaders(HttpResponseMessage response) {
		var headers = new Godot.Collections.Dictionary();
		foreach (var h in response.Headers)
			headers[h.Key] = string.Join(", ", h.Value);
		foreach (var h in response.Content.Headers)
			headers[h.Key] = string.Join(", ", h.Value);
		return headers;
	}

	public void Close() {
		// System.Net.Http.HttpClient uses pooled connections; nothing to close explicitly.
	}

	public void Cancel() {
		bCancelled = true;
	}
	public bool IsCancelled() => bCancelled;

	public void SetProxy(string host, int port, bool ssl = false) {
		var handler = new HttpClientHandler() {
			AllowAutoRedirect = false,
			AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
			UseProxy = true,
			Proxy = new WebProxy(host, port)
		};
		client = new HttpClient(handler);
		client.Timeout = RequestTimeout;
	}

	public void ClearProxy() {
		client = CreateHttpClient();
	}

	public async Task<Status> StartClient(string host, bool use_ssl = false) {
		return await StartClient(host, use_ssl ? 443 : 80, use_ssl);
	}

	public async Task<Status> StartClient(string host, int port, bool use_ssl = false) {
		sHost = host;
		bUseSSL = use_ssl;
		var split = sHost.Split('.');
		if (split.Length == 2) {
			sProperName = split[0].Capitalize();
		} else if (split.Length == 3) {
			sProperName = split[1].Capitalize();
		} else {
			sProperName = sHost.Capitalize();
		}
		bCancelled = false;

		// Pre-flight TCP connectivity check (replaces the removed HTTPClient.ConnectToHost).
		try {
			using (var tcp = new System.Net.Sockets.TcpClient()) {
				await tcp.ConnectAsync(host, port);
			}
			return Status.Connected;
		} catch (System.Net.Sockets.SocketException ex) {
			GD.PrintErr(string.Format(Tr("Unable to connect to {0}:{1} ({2})"), host, port, ex.SocketErrorCode));
			return Status.CantConnect;
		} catch (Exception ex) {
			GD.PrintErr(string.Format(Tr("Connection error with {0}:{1} ({2})"), host, port, ex.Message));
			return Status.ConnectionError;
		}
	}

	public async Task<HTTPResponse> HeadRequest(string path) {
		HTTPResponse resp = null;
		if (bCancelled)
			return resp;
		try {
			using (var request = new HttpRequestMessage(HttpMethod.Head, BuildUrl(path))) {
				ApplyDefaultHeaders(request);
				using (var response = await client.SendAsync(request)) {
					resp = new HTTPResponse();
					resp.ResponseCode = (int)response.StatusCode;
					resp.Headers = BuildHeaders(response);
				}
			}
		} catch (Exception ex) {
			GD.PrintErr($"HEAD request failed for {path}: {ex.Message}");
			return null;
		}
		return resp;
	}

	public async Task<HTTPResponse> MakeRequest(string path, bool binary = false) {
		HTTPResponse resp = null;
		if (bCancelled)
			return resp;
		try {
			using (var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl(path))) {
				ApplyDefaultHeaders(request);
				using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)) {
					resp = new HTTPResponse();
					resp.ResponseCode = (int)response.StatusCode;
					resp.Headers = BuildHeaders(response);
					EmitOnMain("headers_received", resp.Headers);

					// Stream the body, reporting progress as chunks arrive.
					using (var stream = await response.Content.ReadAsStreamAsync())
					using (var ms = new MemoryStream()) {
						byte[] buffer = new byte[81920];
						int read;
						while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
							if (bCancelled) {
								resp.Cancelled = true;
								break;
							}
							ms.Write(buffer, 0, read);
							// HTTP continuations may run off the main thread; marshal UI signals.
							EmitOnMain("chunk_received", read);
						}
						resp.BodyRaw = ms.ToArray();
					}

					if (!binary) {
						try {
							resp.Body = System.Text.Encoding.UTF8.GetString(resp.BodyRaw);
						} catch (Exception) {
							// Body may not be a string (zip files, executables, etc.)
						}
					}
					lastResponse = resp;
				}
			}
		} catch (TaskCanceledException) {
			GD.PrintErr($"Request timed out for {path}");
			return null;
		} catch (HttpRequestException ex) {
			GD.PrintErr($"Request failed for {path}: {ex.Message}");
			return null;
		} catch (Exception ex) {
			GD.PrintErr($"Request error for {path}: {ex.Message}");
			return null;
		}
		EmitOnMain("request_completed");
		return resp;
	}

	void EmitOnMain(StringName signal, params Variant[] args) {
		// HTTP async continuations may run off the main thread; marshal UI signals.
		Callable.From(() => EmitSignal(signal, args)).CallDeferred();
	}

	public bool SuccessConnect(Status result, bool dialogErrors = false, bool printErrors = true) {
		switch (result) {
			case Status.CantResolve:
				if (printErrors) GD.PrintErr(string.Format(Tr("Unable to resolve {0}"), sHost));
				return false;
			case Status.CantConnect:
				if (printErrors) GD.PrintErr(string.Format(Tr("Unable to connect to {0}:{1}"), sHost, bUseSSL ? 443 : 80));
				return false;
			case Status.ConnectionError:
				if (printErrors) GD.PrintErr(string.Format(Tr("Connection error with {0}:{1}"), sHost, bUseSSL ? 443 : 80));
				return false;
			case Status.SslHandshakeError:
				if (printErrors) GD.PrintErr(string.Format(Tr("Failed to negotiate SSL Connection with {0}:{1}"), sHost, bUseSSL ? 443 : 80));
				return false;
			case Status.Connected:
				return true;
			default:
				return false;
		}
	}
}
