using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using ServiceStack.Common.Web;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints.Extensions;

namespace ServiceStack.ServiceHost
{
	public static class HttpResponseExtensions
	{
		public static void RedirectToUrl(this IHttpResponse httpRes, string url)
		{
			httpRes.AddHeader(HttpHeaders.Location, url);
			httpRes.Close();
		}

		public static void TransmitFile(this IHttpResponse httpRes, string filePath)
		{
			var aspNetRes = httpRes as HttpResponseWrapper;
			if (aspNetRes != null)
			{
				aspNetRes.Response.TransmitFile(filePath);
				return;
			}

			using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				fs.WriteTo(httpRes.OutputStream);
			}

			httpRes.Close();
		}

		public static void WriteFile(this IHttpResponse httpRes, string filePath)
		{
			var aspNetRes = httpRes as HttpResponseWrapper;
			if (aspNetRes != null)
			{
				aspNetRes.Response.WriteFile(filePath);
				return;
			}

			using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				fs.WriteTo(httpRes.OutputStream);
			}

			httpRes.Close();
		}

		public static void Redirect(this IHttpResponse httpRes, string url)
		{
			httpRes.AddHeader(HttpHeaders.Location, url);
			httpRes.Close();
		}

		public static void ReturnAuthRequired(this IHttpResponse httpRes)
		{
			httpRes.ReturnAuthRequired("Auth Required");
		}

		public static void ReturnAuthRequired(this IHttpResponse httpRes, string authRealm)
		{
			httpRes.StatusCode = (int)HttpStatusCode.Unauthorized;
			httpRes.AddHeader(HttpHeaders.WwwAuthenticate, "Basic realm=\"" + authRealm + "\"");
			httpRes.Close();
		}

		/// <summary>
		/// Sets a persistent cookie which never expires
		/// </summary>
		public static void SetPermanentCookie(this IHttpResponse httpRes, string cookieName, string cookieValue)
		{
			httpRes.Cookies.AddPermanentCookie(cookieName, cookieValue);
		}

		/// <summary>
		/// Sets a session cookie which expires after the browser session closes
		/// </summary>
		public static void SetSessionCookie(this IHttpResponse httpRes, string cookieName, string cookieValue)
		{
			httpRes.Cookies.AddSessionCookie(cookieName, cookieValue);
		}

		/// <summary>
		/// Sets a persistent cookie which expires after the given time
		/// </summary>
		public static void SetCookie(this IHttpResponse httpRes, string cookieName, string cookieValue, TimeSpan expiresIn)
		{
			httpRes.Cookies.AddCookie(new Cookie(cookieName, cookieValue) {
				Expires = DateTime.UtcNow + expiresIn
			});
		}

		/// <summary>
		/// Sets a persistent cookie with an expiresAt date
		/// </summary>
		public static void SetCookie(this IHttpResponse httpRes, string cookieName,
			string cookieValue, DateTime expiresAt, string path = "/")
		{
			httpRes.Cookies.AddCookie(new Cookie(cookieName, cookieValue, path) {
				Expires = expiresAt,
			});
		}

		/// <summary>
		/// Deletes a specified cookie by setting its value to empty and expiration to -1 days
		/// </summary>
		public static void DeleteCookie(this IHttpResponse httpRes, string cookieName)
		{
			httpRes.Cookies.DeleteCookie(cookieName);
		}

		public static Dictionary<string, string> CookiesAsDictionary(this IHttpResponse httpRes)
		{
			var map = new Dictionary<string, string>();
			var aspNet = httpRes.OriginalResponse as HttpResponse;
			if (aspNet != null)
			{
				foreach (var name in aspNet.Cookies.AllKeys)
				{
					var cookie = aspNet.Cookies[name];
					if (cookie == null) continue;
					map[name] = cookie.Value;
				}
			}
			else
			{
				var httpListener = httpRes.OriginalResponse as HttpListenerResponse;
				if (httpListener != null)
				{
					for (var i = 0; i < httpListener.Cookies.Count; i++)
					{
						var cookie = httpListener.Cookies[i];
						if (cookie == null || cookie.Name == null) continue;
						map[cookie.Name] = cookie.Value;
					}
				}
			}
			return map;
		}

		public static void AddHeaderLastModified(this IHttpResponse httpRes, DateTime? lastModified)
		{
			if (!lastModified.HasValue) return;
			var lastWt = lastModified.Value.ToUniversalTime();
			httpRes.AddHeader(HttpHeaders.LastModified, lastWt.ToString("r"));
		}
	}
}