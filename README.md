# TrailerGet

Simple console app to retrieve trailers for a local media library using yt-dlp.


Requires:
* Media metadata including TMDB id present in .nfo files alongside local media
* TMDB api read access (bearer) token
* yt-dlp and yt-dlp-ejs, deno, and ffmpeg installed, and binary locations added to Path
* cookies.txt file containing relevant cookies exported from browser in Netscape format


Install yt-dlp-ejs using `pip install -U "yt-dlp[default]"`

Install deno using `irm https://deno.land/install.ps1 | iex` in PowerShell
