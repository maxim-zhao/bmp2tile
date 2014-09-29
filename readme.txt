BMP to SMS/GG tile converter 0.32
=================================

by Maxim in 2002-2004

This program converts images from BMP format to SMS/GG tile, tilemap
and palette data.

Instructions
============

FIRST you have to prepare your file in an image editor. This is the
important part! There are certain requirements for it to be processed by
this program. If you are using an image editor that doesn't allow you to
output in the right format then you might need to get one that can
convert for you. I like Paint Shop Pro, it can easily do what you want
here.

Requirement 1: The image should have a width and height that are
  multiples of 8. If it's not, the program can handle that (adding
  padding) but it's not ideal.

Requirement 2: The image must be in one of these formats:
  BMP (not RLE)
  GIF
  PNG (no transparency)
  PCX

Requirement 3: The image MUST be either in 1bpp, 4bpp or 8bpp format.
  Higher bit depths are not acceptable. So there. The reason for this
  is because you (should) want to control your palette, since it will
  be shared among all the images you display. Your image editor should
  have facilities to save a palette and apply it to all the images you
  want to use. Since a given tile is limited to one of the two 16-
  colour palettes, there is no use for any higher colour depth and by
  removing the possibility I remove the chance of accidentally having
  higher indices. If you are using an 8-bit image and you use too many 
  colours, the program will not process it. You can define colours 
  beyond index 15, just don't use them.

So, once you've got that all sussed, save your image to a file and then
drag and drop it onto the program. (You can type or paste the filename
into the box if you prefer to be keyboardy.) Then it'll load it and
process it for you. Then you have some options depending on what you
want...

Settings tab
============

First of all, this will show you your image. Isn't that nice?

If your image is 1-bit, there is an option to invert the colour indices.
Everything that is colour 1 will become colour 0 and vice versa. This is
useful if your image has its colours in the wrong order - perhaps your
image editor doesn't give you control over the palette in 1-bit images.
You can always just use the first two colours in a 4-bit or 8-bit image
for exactly the same effect.

If your image is 4-bit or 8-bit then it will count how many colours are
actually used. Depending on how many are used, it will choose the
minimum number of bits (bitplanes) needed to represent your image. Note,
however, that only 4-bit data is suitable for writing directly to VRAM -
less bits will save ROM space, but will require some handling by your
code to output zeroes in place of the missing bitplanes. If you don't
understand that then crank it up to 4-bit each time.

Tiles tab
=========

This tab shows the tile data. It is in a suitable format to be used with
WLA DX; if you use another assembler/compiler then you might need to
edit it a bit.

If "Remove duplicates" is checked then identical tiles will be removed.
This can save a lot of space, and is essential for full-screen graphics
on the SMS. Note that it can take a few seconds to process, more so if
there are more duplicate tiles. It could be done faster but I'd have to
rewrite the whole thing.

If "Use tile mirroring" is checked then tiles which are horizontal or
vertical mirror images of others will be removed. This requires the
tilemap data to be in word form (see later). If you design your graphics
with this in mind you can save a lot of graphics space.

Click on "Save" to save the tile data to a file. You have three choices
for the format to save in:

  - WLA DX include format. This is the format shown in the box, and can
    either be included in your source with
      .include "filename.inc"
    or simply pasted in.
  - Binary format. This is raw binary data, and can be included in your
    source with
      .incbin "filename.bin"
  - Phantasy Star compressed format. This is compresed data suitable for
    running through Phantasy Star's tile loader routines. You need to
    include it with
      .incbin "filename.bin"
    and also include the code from "Phantasy Star decompressors.inc" -
    read that file for more information. Note that you must choose 4
    bit, word sized data for this to work properly.

Note that the program does not enforce the SMS's limitations on the
number of tiles that can be defined - the SMS is practically limited to
448 (0x1c0) tiles unless you squeeze some into unused tilemap/sprite
table space, and you'll have to do that manually.

Tilemap tab
===========

This tab shows the tilemap data. It is also in WLA DX's data format. If
your tiles are sprites then you don't want this. If your input image is
too big then it won't be output - if you want big tilemaps then you'll
need to make a map editor anyway, and then the format depends on your
code anyway.

If "Tile offset" is changed to a sensible value, the data will be
changed to be correct assuming that the tile data will be loaded into
VRAM starting at that tile's address. In other words, if you put 8 here
(for example), the first tile in the tile data will be referred to in
the tilemap data as tile index 8. This is useful when you're loading
several sets of tiles as they can't all start at 0. If you want to enter
a hex value then prefix it with a $ sign.

If "Pad" is checked then the data will be padded with the given tile
index to make it up to 32 tiles per row. This is useful if you want to
lazily display the image since the data will not require any seeking
within VRAM when you write it. It's not hard to code a proper tile
displayer that accepts the width and height of the tilemap data as
parameters.

If "Bytes" is checked, if possible then the data will be reduced to just
the low bytes of each word. This excludes the following possibilities:

  - Tiles with indices over $ff (255) including any offset
  - Use of mirroring to remove duplicates
  - Setting the sprite palette flag
  - Setting the in front of sprites flag

If "Use sprite palette" or "In front of sprites" are checked then the
appropriate bits will be set in the tile data for all tiles. It is not
possible to do this on a tile-by-tile basis.

"Save" does much the same as before. Note that Phantasy Star compressed
data has an interleaving of 4 for tiles and 2 for tilemaps.

Palette tab
===========

By popular demand...

This tab reads in the palette from the bitmap and attempts to convert it
to data suitable for you to use. It's tricky because there are different
ways to represent the SMS's 4-bit colour with a 24-bit colour system (as
used in BMP files' palettes). I've kludged it to work OK according to
the colours used in Meka (both palette types) and eSMS but I recommend
you use the bright Meka palette (colour values 0, 85, 170, 255) just
because I prefer it.

If you've not arranged your palette properly then the output will be
unpredictable! Make sure your picture hasn't got any weird colours in it
that you weren't expecting.

At the top you're shown the current palette - or rather, just the
colours that are used.

There are a few options for the text output. If you want plain hex
values then choose "Output hex (SMS)". If you choose "Output cl123 (SMS)"
then you can include the "colours.inc" file in your project to define 
the constants used; it makes it easier to tell what colour each value 
represents (see colours.inc for more description).

There's also an option to "Output hex (GG)" which will output 12-bit
Game Gear palette data. This one doesn't attempt to handle different
palette systems, it just shifts colours to their high 4 bits so you'd
better make sure white is 255,255,255.

"Save" works again, but saving as binary or Phantasy Star compressed
doesn't make sense.

Commandline mode
================

Pass the following on the commandline to make the corresponding option/
action choices. The order has no effect.

Command switch      Effect
<filename>          Load the specified bitmap. Note that the format
                    restrictions are the same as before.
-1bit               Treat input data as 1bpp
-2bit               Treat input data as 2bpp
-3bit               Treat input data as 3bpp
-4bit               Treat input data as 4bpp

If you don't specify the bitdepth, the program automatically chooses the
smallest one that will contain all the colour indices used.

-noremovedupes      Do not optimise out duplicate tiles
-nomirror           Do not use tile mirroring to further optimise
                    duplicates

-tileoffset         The starting index of the first tile, in the tilemap
                    data. 0 if unspecified.
-pad<n>             Add this parameter to pad the tilemap data to 32
                    columns, to make smaller bitmaps produce full-width
                    tilemap data. The tile number to pad with must be
                    given (in decimal or $-prefixed hex), with no space
                    between it and the 'd', eg. -pad0
-bytes              Produce only the low byte of the tilemap data - this
                    restricts the number of tiles possible and stops the
                    next two options having any effect.
-spritepalette      Set the tilemap bits to make tiles use the sprite
                    palette
-infrontofsprites   Set the tilemap bits to make tiles appear in front
                    of sprites

-palsms             Output the palette in SMS colour format
-palgg              Output the palette in GG colour format
-palcl123           Output the palette in SMS colour format, using
                    constants of the form cl123 (see above).

-savetilesinc [filename]
-savetilesbin [filename]
-savetilespscompr [filename]
                    Save tile data in WLA DX include format, binary
                    format or Phantasy Star compressed format
                    respectively, to [filename]. If [filename] is
                    omitted then one will be automatically generated
                    from the input filename.
-savetilemapinc [filename]
-savetilemapbin [filename]
-savetilemappscompr [filename]
                    Save tilemap data in WLA DX include format, binary
                    format or Phantasy Star compressed format
                    respectively, to [filename]. If [filename] is
                    omitted then one will be automatically generated
                    from the input filename.
-savepaletteinc [filename]
-savepalettebin [filename]
                    Save palette data in WLA DX include format or binary
                    respectively, to [filename]. If [filename] is
                    omitted then one will be automatically generated
                    from the input filename.

If you specify more than one item from each group above then only the
last will have any effect.

-exit               Exit the program after doing all the above


Still to come
=============

Any suggestions? I ought to make it faster, the way it's done now is
very slow...

Source
======

Included. It's not that great though. Delphi 7 it is.

Dedication
==========

To my beautiful wife :)

