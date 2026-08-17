using System.Net.Http.Json;
using System;
using System.Xml.Linq;
using System.Diagnostics;

namespace TrailerGet
{
    public class Video
    {
        public String iso_639_1 { get; set; }
        public String iso_3166_1 { get; set; }
        public String name { get; set; }
        public String key { get; set; }
        public String site { get; set; }
        public int size { get; set; }
        public String type { get; set; }
        public bool official { get; set; }
        public String published_at { get; set; }
        public String id { get; set; }
    }

    public class Response
    {
        public int id { get; set; }
        public Video[] results { get; set; }
    }

    internal class Getter
    {
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage:  TrailerGet moviedir trailerdir cookiesfile tmdbApiToken");
                Environment.Exit(1);
            }
            String moviedir = args[0];
            String trailerdir = args[1];
            String cookiesfile = args[2];
            String tmdbApiToken = args[3];

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tmdbApiToken}");
            client.DefaultRequestHeaders.Add("accept", "application/json");

            string[] moviePaths = Directory.GetDirectories(moviedir);

            foreach (string moviePath in moviePaths)
            {
                Console.WriteLine($"\n\nFetching trailers for {moviePath}");

                String nfoName = $"{moviePath}\\movie.nfo";
                if (File.Exists(nfoName))
                {
                    // Load the movie's NFO file
                    XDocument doc = XDocument.Load(nfoName);

                    if (doc.Root.Element("tmdbid") is not null)
                    {
                        // Get the tmdbid
                        string tmdbid = doc.Root.Element("tmdbid").Value;

                        Console.WriteLine($"Got TMDB ID {tmdbid}");

                        String tmdburl = $"https://api.themoviedb.org/3/movie/{tmdbid}/videos?language=en-US";

                        try
                        {
                            // Fetch and deserialize JSON
                            Response response = await client.GetFromJsonAsync<Response>(tmdburl);
                            foreach (Video video in response.results)
                            {
                                if (video.type == "Trailer" && video.site == "YouTube" && video.official && video.size >= 720)
                                {
                                    Console.WriteLine($"Attempting download of video {video.name} with key {video.key}");

                                    String ytURL = $"https://www.youtube.com/watch?v={video.key}";
                                    String movieName = Path.GetFileName(moviePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                                    String trailerPath = $"{trailerdir}\\{movieName}\\trailers";
                                    Directory.CreateDirectory(trailerPath);

                                    String baseName = $"{trailerPath}\\trailer1";
                                    String trailerName = $"{baseName}.%(ext)s";
                                    if (!File.Exists($"{baseName}.webm") && !File.Exists($"{baseName}.mkv") && !File.Exists($"{baseName}.mp4"))
                                    {
                                        Process.Start("yt-dlp.exe", $"--remote-components ejs:github --cookies {cookiesfile} -o \"{trailerName}\" {ytURL}").WaitForExit();
                                        await Task.Delay(1000);
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Skipping download for existing trailer {trailerName}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Skipping download of video {video.name} with type {video.type}, site {video.site}, size {video.size}, official={video.official}");
                                }
                            }
                        }
                        catch (HttpRequestException e)
                        {
                            Console.WriteLine($"TMDB request error: {e.Message}");
                        }
                    } else
                    {
                        Console.WriteLine($"{moviePath} has no tmdbid, skipping");
                    }

                    await Task.Delay(100);
                }
            }
        }
    }
}
