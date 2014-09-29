; Phantasy Star tile/tilemap decompression routines
;
; WLA DX warning:
;
;  Great as it is, sometimes WLA DX is buggy, and I've especially found
;  this with anonymous labels. If you're gettng screwy results, a
;  temporary fix is often to change the anonymous labels (-, +, __, etc)
;  to named labels.
;
; Usage notes:
;
; Tile loaders:
;
;   ld de,<target VRAM address ORed with $4000>
;   ld hl,<data address>
;   call LoadTiles4BitRLE
;
;   or
;
;   ld de,<target VRAM address ORed with $4000>
;   ld hl,<data address>
;   call LoadTiles4BitRLENoDI
;
;   The NoDI one does not di/ei around the VRAM accesses, so it's a bit
;   faster but will be messed up by VBlanks.
;
; Tilemap loader 1:
;
;   You need to define a constant called TileMapData. This must be a RAM
;   address of a copy of the tilemap which you'll have to copy into VRAM
;   during the VBlank.
;
;   ld hl,<data address>
;   call LoadTilemapToTileMapData
;
;   I haven't supplied the code to do the copying from RAM to VRAM.
;
; Tilemap loader 2:
;
;   ld de,<target VRAM address ORed with $4000>
;   ld hl,<data address>
;   call LoadTilemapToVRAM
;
;   This is one I wrote myself :) based on the tile loader. It loads the
;   tilemap directly into VRAM. It does not offer a version with di/ei
;   around VRAM accesses, because updating the tilemap during the active
;   display is generally bad; if you really want, you can modify it to
;   do that.
;
; Note:
;
;   Phantasy Star locates its SetVRAMAddressToDE function at offset
;   $0008, so it can do
;     rst $08                   (1 byte, 11 clock cycles)
;   instead of
;     call SetVRAMAddressToDE   (3 bytes, 17 clock cycles)
;   If your code is using that then either define UseRst yourself, or
;   uncomment the next line.

;.define UseRst

.section "SetVRAMAddressToDE" free
SetVRAMAddressToDE:
  ld a,e
  out ($bf),a
  ld a,d
  out ($bf),a
  ret
.ends

.section "Tile loader (4 bpp RLE, no di/ei)" free
; Decompresses tile data from hl to VRAM address de
LoadTiles4BitRLENoDI:
  ld b,$04
-:push bc
    push de
      call _f ; called 4 times for 4 bitplanes
    pop de
    inc de
  pop bc
  djnz -
  ret

_NotAnonymous:
__:
  ld a,(hl)          ; read count byte <----+
  inc hl             ; increment pointer    |
  or a               ; return if zero       |
  ret z              ;                      |
                     ;                      |
  ld c,a             ; get low 7 bits in b  |
  and $7f            ;                      |
  ld b,a             ;                      |
  ld a,c             ; set z flag if high   |
  and $80            ; bit = 0              |
                     ;                      |
-:                   ;                      |
.ifdef UseRst        ; SetVRAMAddressToDE<+ |
  rst $08            ;                    | |
.else                ;                    | |
  call SetVRAMAddressToDE ;               | |
.endif               ;                    | |
  ld a,(hl)          ; Get data byte in a | |
  out ($be),a        ; Write it to VRAM   | |
  jp z,+             ; If z flag then  -+ | |
                     ; skip inc hl      | | |
  inc hl             ;                  | | |
                     ;                  | | |
+:inc de             ; Add 4 to de <----+ | |
  inc de             ;                    | |
  inc de             ;                    | |
  inc de             ;                    | |
  djnz -             ; repeat block  -----+ |
                     ; b times              |
  jp nz,_b           ; If not z flag -------+
  inc hl             ; inc hl here instead  |
  jp _b              ; repeat forever ------+
                     ; (zero count byte quits)
.ends

.section "Phantasy Star Tile loader (4 bpp RLE, with di/ei)" free
LoadTiles4BitRLE:    ; Same as NoDI only with a di/ei around the VRAM access (because VBlanks will mess it up)
  ld b,$04           ; 4 bitplanes
-:push bc
    push de
      call _f ; called 4 times for 4 bitplanes
    pop de
    inc de
  pop bc
  djnz -
  ret

__:
  ld a,(hl)          ; header byte
  inc hl             ; data byte
  or a
  ret z              ; exit at zero terminator
  ld c,a             ; c = header byte
  and $7f
  ld b,a             ; b = count
  ld a,c
  and $80            ; z flag = high bit
-:di
.ifdef UseRst
  rst $08
.else
  call SetVRAMAddressToDE
.endif
  ld a,(hl)
  out ($be),a        ; output data
  ei
  jp z,+             ; if z flag then don't move to next data byte
  inc hl
+:inc de             ; move target forward 4 bytes
  inc de
  inc de
  inc de
  djnz -             ; repeat b times
  jp nz,_b
  inc hl
  jp _b
.ends

.section "Decompress to TileMapData" free
; Copies data from (hl) to TileMapData
; with RLE decompression and 2-interleaving
; data format:
; Header: $fccccccc
;   f = flag: 1 = not RLE, 0 = RLE
;   ccccccc = count
; Then [count] bytes are copied to even bytes starting at TileMapData
; Then the process is repeated for the odd bytes
LoadTilemapToTileMapData:
  ld b,$00           ; b=0
  ld de,TileMapData
  call _f            ; Process even bytes first -------------+
  inc hl             ; and odd bytes second                  |
  ld de,TileMapData+1 ;                                      |
__:ld a,(hl)         ; Get data count in a <-----------------+
  or a               ; \ return                              |
  ret z              ; / if zero                             |
  jp m,+             ; if bit 8 is set then ---------------+ |
                     ; else:                               | |
  ld c,a             ; put it in c -> bc = data count      | |
  inc hl             ; move hl pointer to next byte (data) | |
-:ldi                ; copy 1 byte from hl to de, <------+ | |
                     ; inc hl, inc de, dec bc            | | |
  dec hl             ; move hl pointer back (RLE)        | | |
  inc de             ; skip dest byte                    | | |
  jp pe,-            ; if bc!=0 then repeat -------------+ | |
  inc hl             ; move past RLE'd byte                | |
  jp _b              ; repeat -----------------------------|-+
+:and $7f            ; (if bit 8 is set:) unset it <-------+ |
  ld c,a             ; put it in c -> bc = data count        |
  inc hl             ; move hl pointer to next byte (data)   |
-:ldi                ; copy 1 byte from hl to de, <--------+ |
                     ; inc hl, inc de, dec bc              | |
  inc de             ; skip dest byte                      | |
  jp pe,-            ; if bc!=0 then repeat ---------------+ |
  jp _b              ; repeat -------------------------------+
.ends

.section "Decompress tilemap to VRAM" free
; Decompresses tilemap data from hl to VRAM address de
; This isn't from Phantasy Star; it's the tile routine modified for
; interleaving 2 instead of 4
LoadTilemapToVRAM:
  push de
    call _f
  pop de
  inc de
  call _f
  ret

__:
  ld a,(hl)          ; read count byte <----+
  inc hl             ; increment pointer    |
  or a               ; return if zero       |
  ret z              ;                      |
                     ;                      |
  ld c,a             ; get low 7 bits in b  |
  and $7f            ;                      |
  ld b,a             ;                      |
  ld a,c             ; set z flag if high   |
  and $80            ; bit = 0              |
                     ;                      |
-:di                 ;                      |
.ifdef UseRst        ; SetVRAMAddressToDE<+ |
  rst $08            ;                    | |
.else                ;                    | |
  call SetVRAMAddressToDE ;               | |
.endif               ;                    | |
  ld a,(hl)          ; Get data byte in a | |
  out ($be),a        ; Write it to VRAM   | |
  ei
  jr z,+             ; If z flag then  -+ | |
                     ; skip inc hl      | | |
  inc hl             ;                  | | |
                     ;                  | | |
+:inc de             ; Add 2 to de <----+ | |
  inc de             ;                    | |
  djnz -             ; repeat block  -----+ |
                     ; b times              |
  jr nz,_b           ; If not z flag -------+
  inc hl             ; inc hl here instead  |
  jr _b              ; repeat forever ------+
                     ; (zero count byte quits)
.ends
