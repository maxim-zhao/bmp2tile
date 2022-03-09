# BMP2Tile

It’s a program that can converts BMP, PNG, PCX and GIF format images into raw or compressed 8×8 pixel separated-bitplane tile data suitable for inclusion in SMS and GG homebrew programs. At the same time, it optimises duplicate tiles, creates a tilemap to recreate the original image and converts the palette for you.

http://www.smspower.org/maxim/Software/BMP2Tile
https://github.com/maxim-zhao/bmp2tile

It has a Winamp-inspired plugin interface for data compression. Source for compressors is found here:

https://github.com/maxim-zhao/bmp2tilecompressors

# Instructions

FIRST you have to prepare your file in an image editor. This is the important
part! There are certain requirements for it to be processed by this program. If
you are using an image editor that doesn't allow you to output in the right
format then you might need to get one that can convert for you. I like Paint
Shop Pro 7, it can easily do what you want here.

### Requirement 1
The image should have a width and height that are multiples of 8.
If it's not, the program can handle that (adding padding) but it's not ideal.

### Requirement 2
The image must be in one of these formats:
- PNG
- BMP
- GIF

### Requirement 3
The image MUST be either in 1bpp, 4bpp or 8bpp format.
Higher bit depths are not acceptable. So there. The reason for this is
because you (should) want to control your palette, since it will be shared
among all the images you display. Your image editor should have facilities to
save a palette and apply it to all the images you want to use. Since a given
tile is limited to one of the two 16-colour palettes, there is no use for
any higher colour depth and by removing the possibility I remove the chance
of accidentally having higher indices. If you are using an 8-bit image and
you use too many colours, the program will not process it. You can define
colours beyond index 15, just don't use them.

So, once you've got that all sussed, save your image to a file and then drag
and drop it onto the program. (Alternatively, you can find your file the old-
fashioned way with the Browse button.) Then it'll load it and process it for
you. Then you have some options depending on what you want...

## Source tab

This will show you your image. Isn't that nice?

## Tiles tab

This tab shows the tile data. It is in a suitable format to be used with
WLA DX; if you use another assembler/compiler then you might need to
edit it a bit.

If "Remove duplicates" is checked then identical tiles will be removed.
This can save a lot of space, and is essential for full-screen graphics
on the SMS.

If "Use tile mirroring" is checked then tiles which are horizontal or
vertical mirror images of others will be removed. If you design your graphics
with this in mind you can save a lot of graphics space.

If "Treat as 8x16" is checked then the image is decomposed in the order

```
  1 3 5
  2 4 6
  7 9 b
  8 a c
```

This is suitable for use with 8x16 sprites, for example. However, "Remove
duplicates" may make the resulting data not work as expected. You probably
ought to make sure the image height is a multiple of 16.

If "Planar tile output" is checked, the resulting data is bitplane-separated -
each byte contains the data for one bitplane of a row of pixels. If it's
unchecked, you will get "chunky" data - each byte holds all four bits for two
pixels.

Enter a number in the "Index of first tile" box to have the data generated with
the assumption that the first tile is not tile zero. For the tile data, this
just affects the comments, but it also applies to the tilemap data (see below).
To enter a hex number, prefix it with '$'.

Click on "Save" to save the tile data to a file. The available file formats
depend on the plugins (see below) - but you will always be able to save the
displayed text as an "inc" file. This can either be included in your source
with .include "filename.inc" or simply pasted in.

Note that the program does not enforce the SMS's limitations on the number of
tiles that can be defined - the SMS is practically limited to 448 (0x1c0) tiles
unless you squeeze some into unused tilemap/sprite table space, and you'll have
to do that manually.

## Tilemap tab

This tab shows the tilemap data. It is also in WLA DX's data format. If your
tiles are sprites then you don't want this.

"Save" does much the same as before.

## Palette tab

This tab reads in the palette from the bitmap and attempts to convert it to
data suitable for you to use. It's tricky because there are different ways to
represent the SMS's 4-bit colour with a 24-bit colour system (as used in BMP
files' palettes). I've kludged it to work OK according to the colours used in
Meka (both palette types) and eSMS but I recommend you use the bright Meka
palette (colour values 0, 85, 170, 255) just because I prefer it.

At the top you're shown the current palette. This is extracted from your image,
rather than recreated from the converted data, but that shouldn't matter.

There are a few options for the text output. If you want plain hex values then
choose "Output hex (SMS)". If you choose "Output cl123 (SMS)" then you can
include the "colours.inc" file in your project to define the constants used; it
makes it easier to tell what colour each value represents (see colours.inc for
more description).

There's also an option to "Output hex (GG)" which will output 12-bit Game Gear
palette data. This one doesn't attempt to handle different palette systems, it
just shifts colours to their high 4 bits so you'd better make sure white is 255,
255,255.

The option "Always emit 16 colours" toggles between emitting only as many colours
as are used (possibly including unused ones in the middle, but omitting unused 
ones with higher indexes than any that are used in the image) and emitting a full 
16 colour palette (if you want it to emit the full palette regardless of what's
actually used).

"Save" works again, but there are no plugins any more.

# Plugins

For saving tile and tilemap data, plugins are used. These are DLL files found
in the same directory as the program, with filenames starting with "gfxcomp_".

Each plugin must define a unique file extension for its data. (Double
extensions don't work.) This allows the commandline mode (see below) to infer
the plugin to use. It also helps to have it define a reasonable name for itself.
Plugins can be tiles-only, tilemap-only or both.

If you want to write a plugin, make a DLL with the right filename that exports
these functions (with cdecl calling convention, which is the default for most
C/C++ DLLs):

```C
extern "C" __declspec(dllexport) const char* getName()
```
Returns a null-terminated string giving the name of the format, for display.

```C
extern "C" __declspec(dllexport) const char* getExt()
```
Returns a null-terminated file extension (without any preceding dot) that is
used to build filename masks and to tell which plugin to use in commandline
mode.

```C
extern "C" __declspec(dllexport) int compressTiles(const uint8_t* pSource, const uint32_t numTiles, uint8_t* pDestination, const uint32_t destinationLength)
```
Compresses the tile data from pSource to pDestination. Each tile is 32 bytes. If
destinationLength is too small, you must return 0. If there is an error while
compressing (perhaps the tile data does not conform to some restriction),
return -1. Else return the number of bytes inserted into the buffer.

```C
extern "C" __declspec(dllexport) int compressTilemap(const uint8_t* pSource, const uint32_t width, const uint32_t height, uint8_t* pDestination, const uint32_t destinationLength)
```
Compresses the tilemap data from pSource to pDestination. Each tilemap entry is 2
bytes in little-endian order. If destinationLength is too small, you must return 0. If
there is an error (perhaps some restriction on the data), return -1. Else
return the number of bytes inserted into the buffer.

You can support one or both `compress*` functions.

# Commandline mode

Pass the following on the commandline to make the corresponding option/action
choices. Defaults are marked with :star:.

|Command switch           |Effect                                       |
|-------------------------|---------------------------------------------|
|`<filename>`             |Load the specified bitmap. Note that the format restrictions are the same as before.           |
|`-removedupes`           |:star: Optimise out duplicate tiles                                                                  |
|`-noremovedupes`         |Or don't                                                                                       |
|`-mirror`                |:star: Use tile mirroring to further optimise duplicates                                             |
|`-nomirror`              |Or don't                                                                                       |
|`-8x8`                   |:star: Treat tile data as 8x8                                                                        |
|`-8x16`                  |Treat tile data as 8x16                                                                        |
|`-planar`                |:star: Output planar tile data                                                                       |
|`-chunky`                |Output chunky tile data                                                                        |
|`-tileoffset <n>`        |The starting index of the first tile. :star: Default is 0.                                           |
|`-spritepalette`         |Set the tilemap bit to make tiles use the sprite palette. :star: Default is unset.                   |
|`-infrontofsprites`      |Set the tilemap bit to make tiles appear in front of sprites. :star: Default is unset.               |
|`-palsms`                |:star: Output the palette in SMS colour format                                                       |
|`-palgg`                 |Output the palette in GG colour format                                                         |
|`-palcl123`              |Output the palette in SMS colour format, using constants of the form cl123 (see above).        |
|`-fullpalette`           |Output 16 colours rather than as many as are present in the image.                             |
|`-savetiles <filename>`  |Save tile data to `<filename>`. The format will be inferred from the extension of `<filename>`.    |
|`-savetilemap <filename>`|Save tilemap data to `<filename>`. The format will be inferred from the extension of `<filename>`. |
|`-savepalette <filename>`|Save palette data to `<filename>`. The format will be inferred from the extension of `<filename>`. |

Note that options are interpreted sequentially. That means you should specify any options (and the input file) before any
`-save*` actions. It also you can chain together operations, as so:

```
bmp2tile.exe foo.png -savetiles "foo.tiles.zx7" -savetiles "foo.tiles.bin" -savetilemap "fool.tilemap.withmirroring.zx7" -nomirror -savetilemap "fool.tilemap.nomirroring.zx7"
```

# Source

https://github.com/maxim-zhao/bmp2tile

BMP2Tile started life in Delphi 7, but as of version 0.5 became written in C#.
Most of the plugins are written in C++.

# History

0.61
- Fixed bug in GUI regarding palette format controls

0.6
- Support reading "raw" data as the input image (SMS/GG raw tile data, .bin extension)
- Print time taken to console
- Support 32-colour images (provided they fit in the dual-palette limitations of SMS/GG)
- Avoid holding image file handles open
- Fixed planar mode checkbox
- Bundling tile compression DLLs
- Show help in commandline mode

0.5
- Rewrite to C#. Commandline mode is now much faster and operates sequentially.

0.44
- Palette preview now shows you the result of mapping to the SMS or GG palette

0.43
- Fixed a stupid bug which meant tilemap width and height were swapped when compressing

0.42
- Option to always emit 16 colours

0.41
- Fixed the broken tilemap modifications (flags and offsets) when saving as non-text

0.4
- Rewrote the tile/tilemap code, it's much faster now
- Added plugin support
- Removed 1/2/3bpp output support, plugins can do that now

0.35
- Fixed a stupid bug with 8-bit images
- Modified demo to use more varied source images to help with testing
- Included source that got left out of 0.34

0.34
- Relaxed image size restrictions as far as possible
- Added some checking of formats when saving to avoid nonsensical option combinations
- Added more decompressor code and demos

0.33
- The Load button is now a Browse button and gives you a normal Windows Open dialogue

0.32
- Added support for 8-bit images (using only the first 16 palette entries)
- Added support for more image formats (GIF, PNG, PCX)
- Fixed some bugs when loading a second image
- Fixed some bugs with 1-bit images

0.31
- Added a missing commandline switch
- Added optional output filename specification
- Fixed a bug in the palette convertor
- Added a demo of all the output formats

0.3
- Added binary and Phantasy Star compressed output
- Added commandline mode

0.22
- Vertical mirroring above 1bpp was broken; now it isn't.

0.21
- Fixed a few glitches

0.2
- Added palette decoding
- Added tile mirroring optimisation
- Smoothed out the rough edges a bit more

0.1
- Initial release

# Dedication

To my beautiful wife :)
