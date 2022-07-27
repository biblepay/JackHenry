using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace JackHenryTwitterService
{
	public interface ITwitterExport
    {
		public int GetHashtagCount();
		public int GetTotalTweetsReceived();
		public List<TwitterExport> GetTrendingHashtags(int iLimit);
	}

	// This public class is the Type exposed to the consuming class yielding the trending hashtag name and count:
	public class TwitterExport
	{
		public string Hashtag { get; set; }
		public int Occurrences { get; set; }
	}

	public class TwitterInboundService : ITwitterExport
	{
		public static int iVersion = 1;

		// This dictionary is a static singleton used to store the HashTag and the count.
		private static Dictionary<string, int> dictHashtags = new Dictionary<string, int>();
		private static int iTweetsProcessed = 0;
	
		public TwitterInboundService()
        {
			System.Threading.Thread t = new System.Threading.Thread(TwitterServiceLoop);
			t.Start();
		}
		private static void StoreHashtags(List<string> lTags)
		{
			foreach (string sTag in lTags)
			{
				if (!dictHashtags.ContainsKey(sTag))
				{
					dictHashtags.Add(sTag, 1);
				}
				else
				{
					dictHashtags[sTag]++;
				}
			}
		}
		// Singleton to get the Total Hashtag storage count:
		public int GetHashtagCount()
		{
			return dictHashtags.Count;
		}

		// Singleton to get the Total tweets processed:
		public int GetTotalTweetsReceived()
        {
			return iTweetsProcessed;
        }

		// Singleton to get the Trending Export:
		public List<TwitterExport> GetTrendingHashtags(int iLimit)
		{
			List<TwitterExport> l = new List<TwitterExport>();
			int iProc = 0;
			foreach (KeyValuePair<string, int> item in dictHashtags.OrderByDescending(key => key.Value))
			{
				TwitterExport t = new TwitterExport();
				t.Hashtag = item.Key;
				t.Occurrences = item.Value;
				l.Add(t);
				iProc++;
				if (iProc >= iLimit)
					break;
			}
			return l;
		}
		// This function parses a tweet string into a list of hashtags.
		// If we encounter a space, enter, or non numeric or alphabetic character, the parser moves to the next tag.
		private static List<string> ExtractHashtags(string s)
		{
			List<string> lHashtags = new List<string>();
			StringBuilder sb = null;
			bool bInTag = false;
			char[] cArray = s.ToCharArray();
			for (int i = 0; i < s.Length; i++)
			{
				char c = cArray[i];
				if (c == '#')
				{
					sb = new StringBuilder();
					bInTag = true;
				}
				else if ((c == ' ' || i == cArray.Length - 1 || !char.IsLetterOrDigit(c)) && bInTag)
				{
					bInTag = false;
					string sHashtag = sb.ToString().Trim();
					if (sHashtag.Length > 0)
					{
						lHashtags.Add(sHashtag);
					}
				}
				if (bInTag)
				{
					if (char.IsLetterOrDigit(c))
					{
						sb.Append(c);
					}
				}
			}
			return lHashtags;
		}

		private static async void TwitterServiceLoop()
		{
			// This is async because we are using the HttpClient.  
			var config = new ConfigurationBuilder()
					.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
					.AddJsonFile("appsettings.json").Build();
			
			string sBearer = config.GetValue<string>("TwitterCredentialsBearer");
			while (true)
			{
				try
				{
					// This inner HTTPS connection to twitter never breaks out unless an error occurs:
					using (var httpClient = new HttpClient())
					{
						httpClient.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
						string sURI = "https://api.twitter.com/2/tweets/sample/stream";
						var request = new HttpRequestMessage(HttpMethod.Get, sURI);
						request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + sBearer);
						var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
						response.EnsureSuccessStatusCode();
						var stream = await response.Content.ReadAsStreamAsync();
						using (var streamReader = new StreamReader(stream))
						{
							while (!streamReader.EndOfStream)
							{
								var currentLine = streamReader.ReadLine();
								iTweetsProcessed++;
								List<string> l = ExtractHashtags(currentLine);
								StoreHashtags(l);
								if (System.Diagnostics.Debugger.IsAttached)
									System.Diagnostics.Debug.WriteLine(currentLine);
							}
						}
					}
				}
				catch (Exception ex)
				{
					// TODO: We need to write this to the Jack Henry domain log instead of console...
					Console.WriteLine(ex.Message);
				}
				// Sleep for a minute, then start a new HTTPS connection to twitter.
				System.Threading.Thread.Sleep(60000);
			}
		}
	}
}


