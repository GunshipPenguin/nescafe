# NEScafé :coffee: :video_game:
[![Travis](https://img.shields.io/travis/GunshipPenguin/nescafe.svg)](https://travis-ci.org/GunshipPenguin/nescafe/)

A Nintendo Entertainment System (NES) emulator written in C#.

So named since almost all of the work on this was done in various cafés while 
drinking large quantities of coffee.

<table align="center">
    <tr>
        <td>
            <img src="https://i.imgur.com/xrJ6Yir.gif" width="256px">
        </td>
        <td>
            <img src="https://i.imgur.com/rYfov9J.gif" width="256px">
        </td>
    </tr>
</table>

# Building

Compile with Visual Studio or from the command line with:

`msbuild /property:Configuration=Release nescafe.sln`

or with xbuild:

`xbuild /property:Configuration=Release nescafe.sln`

# Loading a ROM

Load an iNES ROM using File->Load ROM. The game should start immediately
or an error will be displayed indicating why the ROM could not be loaded.

# Limitations

- The NES APU is currently not implemented meaning no audio.
- Mapper 0 is currently the only mapper supported.

# Controls

- Arrow Keys = up,down,left,right
- Z = A
- X = B
- Q = Start
- W = Select

# License

[MIT](https://github.com/GunshipPenguin/nescafe/blob/master/LICENSE) © Rhys Rustad-Elliott