; uncomment to skip first 2 pictures
;.define skipfirsttwopictures 1

;==============================================================
; WLA-DX banking setup
;==============================================================
.memorymap
defaultslot 0
slotsize $8000
slot 0 $0000
.endme

.rombankmap
bankstotal 1
banksize $8000
banks 1
.endro

; Memory map (for Phantasy Star tilemap decompression)
.enum $c000
TileMapData: dsb 32*24*2

.ende

;==============================================================
; SDSC tag and SMS rom header
;==============================================================
.sdsctag 1.00,"Demo program","Demonstrates usage of BMP2tile","Maxim"

.bank 0 slot 0
.org $0000

.include "..\colours.inc"
.include "..\Phantasy Star decompressors.inc"

;==============================================================
; Boot section
;==============================================================
.org $0000
.section "Boot section" force
  di              ; disable interrupts
  im 1            ; Interrupt mode 1
  jp main         ; jump to main program
.ends

;==============================================================
; Pause button handler
;==============================================================
.org $0066
.section "Pause button handler" force
  ; Do nothing
  retn
.ends

;==============================================================
; Main program
;==============================================================
.section "Main program" free
main:
  ld sp, $dff0

  ; Initialise VDP
  call DefaultInitialiseVDP
  call ClearVRAM

  call NoSprites ; they mess things up

picture1:
.ifndef skipfirsttwopictures
  ;==============================================================
  ; Picture 1: WLA DX includes
  ;==============================================================
  ; Load palette
  ld hl,$c000                     ; palette index 0 write address
  call VRAMToHL
  ld hl,akmwpalette               ; data
  ld bc,akmwpaletteend-akmwpalette ; size
  call WriteToVRAM

  ; Load tiles
  ld hl,$4000                      ; Tile index 0 write address
  call VRAMToHL
  ; Write to VRAM
  ld hl,akmwtiles
  ld bc,267*32    ; amount to write (267 tiles, 32 bytes each)
  call WriteToVRAM

  ; Load tilemap
  ld hl,$3800 | $4000               ; Tilemap (0,0) write address
  call VRAMToHL
  ld hl,akmwtilemap
  ld bc,32*24*2   ; full-screen, 2 bytes per tile
  call WriteToVRAM

  ; Turn screen on
  ld a,%11000100 ; $c4
;        |||| |`- Zoomed sprites -> 16x16 pixels
;        |||| `-- Doubled sprites -> 2 tiles per sprite, 8x16
;        |||`---- 30 row/240 line mode
;        ||`----- 28 row/224 line mode
;        |`------ VBlank interrupts
;        `------- Enable display
  out ($bf),a
  ld a,$81
  out ($bf),a

  call WaitForButton

  ; Turn screen off
  ld a,$84
  out ($bf),a
  ld a,$81
  out ($bf),a

  ;==============================================================
  ; Picture 2: binary
  ;==============================================================
  ; Load palette
  ld hl,$c000                     ; palette index 0 write address
  call VRAMToHL
  ld hl,sonicpalette               ; data
  ld bc,sonicpalettesize           ; size
  call WriteToVRAM

  ; Load tiles
  ld hl,$4000                      ; Tile index 0 write address
  call VRAMToHL
  ; Write to VRAM
  ld hl,sonictiles
  ld bc,sonictilessize             ; amount to write
  call WriteToVRAM

  ; Load tilemap
  ld hl,$3800 | $4000               ; Tilemap (0,0) write address
  call VRAMToHL
  ld hl,sonictilemap
  ld bc,sonictilemapsize
  call WriteToVRAM

  ; Turn screen on
  ld a,$c4
  out ($bf),a
  ld a,$81
  out ($bf),a

  call WaitForButton

  ; Turn screen off
  ld a,$84
  out ($bf),a
  ld a,$81
  out ($bf),a
.endif
  ;==============================================================
  ; Picture 3: PS compressed
  ;==============================================================
  ; Load palette
  ld hl,$c000                     ; palette index 0 write address
  call VRAMToHL
  ld hl,pspalette               ; data
  ld bc,pspaletteend-pspalette ; size
  call WriteToVRAM

  ; Load tiles
  ld de,$4000
  ld hl,pstiles
  call LoadTiles4BitRLENoDI

  ; Load tilemap
  ld hl,pstilemap
  call DecompressToTileMapData

  ; Copy it to VRAM (normally you'd do this in the VBlank)
  ld hl,$3800 | $4000     ; Tilemap (0,0) write address
  call VRAMToHL
  ld hl,TileMapData
  ld bc,32*24*2
  call WriteToVRAM

  ; Turn screen on
  ld a,$c4
  out ($bf),a
  ld a,$81
  out ($bf),a

  call WaitForButton

  ; Turn screen off
  ld a,$84
  out ($bf),a
  ld a,$81
  out ($bf),a

  ; Go back to the start
  jp picture1
.ends

;==============================================================
; Data
;==============================================================
.section "Data" FREE
  akmwtiles:
  .include "akmwtiles.inc"
  akmwtilemap:
  .include "akmwtilemap.inc"
  akmwpalette:
  .include "akmw (palette).inc"
  akmwpaletteend:

  sonictiles:
  .incbin "sonictiles.bin" fsize sonictilessize
  sonictilemap:
  .incbin "sonictilemap.bin" fsize sonictilemapsize
  sonicpalette:
  .incbin "sonicpalette.bin" fsize sonicpalettesize

  pstiles:
  .incbin "ps (tiles).pscompr"
  pstilemap:
  .incbin "ps (tile numbers).pscompr"
  pspalette:
  .incbin "ps (palette).bin" fsize pstilessize
  pspaletteend:
.ends




;==============================================================
; Set up VDP registers (default values)
;==============================================================
; Call DefaultInitialiseVDP to set up VDP to default values.
; Also defines NameTableAddress, SpriteTableAddress and SpriteSet
; which can be used elsewhere.
; To change the values used, copy and paste the modified data
; and code into the main source. Data is commented to help.
;==============================================================
.section "Initialise VDP to defaults" free
DefaultInitialiseVDP:
    push hl
    push bc
        ld hl,_Data
        ld b,_End-_Data
        ld c,$bf
        otir
    pop bc
    pop hl
    ret

.define SpriteSet           0       ; 0 for sprites to use tiles 0-255, 1 for 256+
.define NameTableAddress    $3800   ; must be a multiple of $800; usually $3800; fills $700 bytes (unstretched)
.define SpriteTableAddress  $3f00   ; must be a multiple of $100; usually $3f00; fills $100 bytes

_Data:
    .db %00000100,$80
    ;    |||||||`- Disable synch
    ;    ||||||`-- Enable extra height modes
    ;    |||||`--- SMS mode instead of SG
    ;    ||||`---- Shift sprites left 8 pixels
    ;    |||`----- Enable line interrupts
    ;    ||`------ Blank leftmost column for scrolling
    ;    |`------- Fix top 2 rows during horizontal scrolling
    ;    `-------- Fix right 8 columns during vertical scrolling
    .db %10000100,$81
    ;     |||| |`- Zoomed sprites -> 16x16 pixels
    ;     |||| `-- Doubled sprites -> 2 tiles per sprite, 8x16
    ;     |||`---- 30 row/240 line mode
    ;     ||`----- 28 row/224 line mode
    ;     |`------ Enable VBlank interrupts
    ;     `------- Enable display
    .db (NameTableAddress>>10) |%11110001,$82
    .db (SpriteTableAddress>>7)|%10000001,$85
    .db (SpriteSet<<2)         |%11111011,$86
    .db $f|$f0,$87
    ;    `-------- Border palette colour (sprite palette)
    .db $00,$88
    ;    ``------- Horizontal scroll
    .db $00,$89
    ;    ``------- Vertical scroll
    .db $ff,$8a
    ;    ``------- Line interrupt spacing ($ff to disable)
_End:
.ends

;==============================================================
; Clear VRAM
;==============================================================
; Sets all of VRAM to zero
;==============================================================
.section "Clear VRAM" free
ClearVRAM:
  push af
  push hl
    ld hl,$4000
    call VRAMToHL
    ; Output 16KB of zeroes
    ld hl, $4000    ; Counter for 16KB of VRAM
  -:ld a,$00        ; Value to write
    out ($be),a ; Output to VRAM address, which is auto-incremented after each write
    dec hl
    ld a,h
    or l
    jp nz,-
  pop hl
  pop af
  ret
.ends

;==============================================================
; VRAM to HL
;==============================================================
; Sets VRAM write address to hl
;==============================================================
.section "VRAM to HL" free
VRAMToHL:
  push af
    ld a,l
    out ($bf),a
    ld a,h
    out ($bf),a
  pop af
  ret
.ends

;==============================================================
; VRAM writer
;==============================================================
; Writes BC bytes from HL to VRAM
; Clobbers HL, BC, A
;==============================================================
.section "Raw VRAM writer" free
WriteToVRAM:
-:ld a,(hl)
  out ($be),a
  inc hl
  dec bc
  ld a,c
  or b
  jp nz,-
  ret
.ends

;==============================================================
; Sprite disabler
;==============================================================
; Sets sprite 1 to y=208
; Clobbers HL, A
;==============================================================
.section "No sprites" free
NoSprites:
  ld hl,SpriteTableAddress | $4000
  call VRAMToHL
  ld a,208
  out ($be),a
  ret
.ends

;==============================================================
; Wait for button press
;==============================================================
; Clobbers A
; Not very efficient, I'm aiming for simplicity here
;==============================================================
.section "Wait for button press" free
WaitForButton:
-:in a,$dc ; get input
  cpl      ; invert bits
  or a     ; test bits
  jr nz,-  ; wait for no button press
-:in a,$dc ; get input
  cpl      ; invert bits
  or a     ; see if any are set
  jr z,-
  ret
.ends
