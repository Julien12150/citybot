# CityBot
A Discord bot made for [City Islands](https://discord.gg/htSMXTr), a server where people run for president.

# Building
Requirements: A program for developing C# programs. (ex. [Visual Studio](https://www.visualstudio.com/vs/visual-studio-2017-rc/)
 or [Xamarin Studio & MonoDevelop](http://www.monodevelop.com/))

- Go to `%APPDATA%` on Windows or `~/.config` on Linux and create the directory `Julien12150`, then `CityBot` inside of that.

- Create a file named `token.txt` in the `CityBot` directory, and put your bot's token inside of it.

- Create a file named `config.txt` too (see below).

- Open the `CityBot.sln` file inside your C# developing program, then compile the bot.

# Config file
Inside the `config.txt` file, there's most of the configuration of the bot, and IDs of roles and stuff.

You must write:
- `candidateRole=<ID>`, and replace `<ID>` by the ID of the candidate role.
- `presidentRole=<ID>`, and replace `<ID>` by the ID of the president role.
- `adminRole=<ID>`, and replace `<ID>` by the ID of the admin role. (Is permanent, but might be unrecommanded to do stuff that the president can do)
- `announcementChannel=<ID>`, and replace `<ID>` by the ID of the announcement channel.
- `server=<ID>`, and replace `<ID>` by the ID of your server.
- `prefix=<string>`, and replace `<string>` by any kind of string you want to be used before using a command.
- `botname=<string>`, and replace `<string>` by how you call your bot's name.
- `host=<string>`, and replace `<string>` by your name.
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