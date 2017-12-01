# NEScafé :video_game: :coffee:
[![Travis](https://img.shields.io/travis/GunshipPenguin/nescafe/master.svg)](https://travis-ci.org/GunshipPenguin/nescafe/)
[![AppVeyor](https://ci.appveyor.com/api/projects/status/wlachyvx7o80tr94/branch/master?svg=true)](https://ci.appveyor.com/project/GunshipPenguin/nescafe)

A Nintendo Entertainment System (NES) emulator written in C#.

So named since almost all of the work on this was done in various cafés while 
drinking large quantities of coffee.

<table align="center">
    <tr>
        <td>
            <img src="https://i.imgur.com/xrJ6Yir.gif" width="256px">
        </td>
        <td>
            <img src="https://i.imgur.com/Wjd4onH.gif" width="256px">
        </td>
        <td>
            <img src="https://i.imgur.com/rYfov9J.gif" width="256px">
        </td>
    </tr>
</table>

# Running

Grab the latest build [from AppVeyor](https://ci.appveyor.com/project/GunshipPenguin/nescafe/build/artifacts) or build it yourself (see below).

# Building

Compile with Visual Studio or from the command line with:

`msbuild /property:Configuration=Release Nescafe.sln`

# Loading a ROM

Load an iNES ROM using File->Load ROM. The game should start immediately
or an error will be displayed indicating why the ROM could not be loaded.

# Mapper Support

The following iNES mappers are supported:

- [Mapper 0 (NROM)](https://wiki.nesdev.com/w/index.php/NROM) - Super Mario Bros., Donkey Kong, Spy vs. Spy
- [Mapper 1 (MMC1)](https://wiki.nesdev.com/w/index.php/MMC1) - The Legend of Zelda, Castlevania 2, Tetris
- [Mapper 2 (UxROM)](https://wiki.nesdev.com/w/index.php/UxROM) - Castlevania, Mega Man, Contra
- [Mapper 4 (MMC3)](https://wiki.nesdev.com/w/index.php/MMC3) - Super Mario Bros. 2, Super Mario Bros 3., Mega Man 3

# Accuracy

The NES CPU and PPU have been implemented to a fairly cycle accurate extent. Certain things (eg. sprite evaluation) are not totally cycle accurate, but this doesn't seem to be a problem for the majority of games.

# Limitations

- The NES APU is currently not implemented meaning no audio.
- Battery backed persistent memory is not currently supported

# Controls

Controls cannot currently be configured.

- Arrow Keys = up,down,left,right
- Z = A
- X = B
- Q = Start
- W = Select

# License

[MIT](https://github.com/GunshipPenguin/nescafe/blob/master/LICENSE) © Rhys Rustad-Elliott
