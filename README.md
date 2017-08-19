# NEScafé :coffee: :video_game:

A Nintendo Entertainment System (NES) emulator written in C#.

So named since almost all of the work on this was done in various cafés while 
drinking large quantities of coffee.

<table align="center">
    <tr>
        <td>
            <img src="https://i.imgur.com/Rnr8Twr.gif" width="256px">
        </td>
        <td>
            <img src="https://i.imgur.com/iNMi9zC.gif" width="256px">
        </td>
    </tr>
</table>

# Building

Compile with Visual Studio or from the command line with:

`msbuild /property:Configuration=Release nescafe.sln`

or with xbuild:

`xbuild /property:Configuration=Release nescafe.sln`

# Loading a ROM

Load an iNES rom using File->Load NES Cartridge

# Controls

- Arrow Keys = up,down,left,right
- Z = A
- X = B
- Q = Start
- W = Select

# License

[MIT](https://github.com/GunshipPenguin/nescafe/blob/master/LICENSE) © Rhys Rustad-Elliott