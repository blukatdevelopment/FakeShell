# Fake Shell

![Screenshot](https://raw.githubusercontent.com/blukatstudios/FakeShell/master/screenshot.png)

Yup, it's a fake unix shell with a few basic commands provided.
Compile it with mcs or whatever official microsoft compiler is available,
then call it from a terminal or commandline to interact with it through a captive
user interface.

This was made mostly out of curiosity, but should be portable enough to plug into a GUI inside a game framework like Godot or Unity3D.

## Commands

- clear
- ls
- cd
- exit
- pwd
- rm
- touch
- mkdir
- cat

## Development

Piping fake stdout into other programs isn't implemented currently, but can be by setting outputMode to pipe before running a command, then feeding stdout into the next program and so forth. I might get around to that, but this was made on a whim, so perhaps not.
