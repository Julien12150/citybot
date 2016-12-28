# CityBot
A Discord bot made for [City Islands](https://discord.gg/htSMXTr), a server where people run for president.

# Building
Requirements: A program for developing C# programs. (ex. [Visual Studio](https://www.visualstudio.com/vs/visual-studio-2017-rc/)
 or [Xamarin Studio & MonoDevelop](http://www.monodevelop.com/))

- Go to `%APPDATA%` on Windows or `~/.config` on Linux and create the directory `Julien12150`, then `CityBot` inside of that.

- Create a file named `token.txt` in the `CityBot` directory, and put your bot's token inside of it.

- Create a file named `config.txt` too (see below).

- Open the `CityBot.sln` file inside your C# developing program, then compile the bot.

NOTE: It's recommanded to have the bot being in one single server, as the bot being in multiple server has been untested.

# Config
Inside the `config.txt` file, there's most of the configuration of the bot, such as IDs of roles.

Your config must contain:
- `candidateRole=<ID>`, with `<ID>` replaced by the ID of the candidate role.
- `presidentRole=<ID>`, with `<ID>` replaced by the ID of the president role.
- `adminRole=<ID>`, with `<ID>` replaced by the ID of the admin role. (Is permanent, but might be unrecommended to do stuff that the president can do)
- `announcementChannel=<ID>`, with `<ID>` replaced by the ID of the announcement channel.
- `server=<ID>`, with `<ID>` replaced by the ID of your server.
- `prefix=<string>`, with `<string>` replaced with what you want to use as a command prefix. Example: if you set it to `!`, you would run the help command with `!help`
- `botname=<string>`, with `<string>` replaced by your bot's name.
- `host=<string>`, with `<string>` replaced by your name.

Example:
```
candidateRole=238106051203510530
presidentRole=238230546084040368
adminRole=237321035016805604
announcementChannel=237054654654020610
server=229607687065560106
prefix=!
botname=Yo controller
host=ProJohn1337
```
