; Phantasy Star Gaiden tile decompressor
; To use:
;
; .define PSGDecoderBuffer $c000                   ; define a 34 byte work area for it
; .include "Phantasy Star Gaiden decompressor.inc" ; include this file
; ld ix, <address of compressed data>
; ld hl, <VRAM address to write to, ORed with $4000>
; call PSG_decompress
;

.section "PSG decompressor" free

.define vram_ptr PSGDecoderBuffer          ; word: VRAM address
.define buffer PSGDecoderBuffer + 2            ; 32-byte decompression buffer

; hl = dest
; ix = src
PSG_decompress:
  ld (vram_ptr),hl  ; cache VRAM address
  ld c,(ix+0)    ; bc = number of tiles
  inc ix
  ld b,(ix+0)
  inc ix

_DecompressTile:
  push bc        ; save number of tiles
    ld b,$04     ; count 4 bitplanes
    ld de,buffer ; write to de
    ld c,(ix+0)  ; c = encoding information for 4 bitplanes
    inc ix

_DecompressBitplane:
    rlc c        ; %0x = all bits either 0 or 1
    jr nc,_AllTheSame
    rlc c        ; %11 = raw data
    jr c,_RawData

_Compressed:
    ld a,(ix+0)  ; get method byte
    inc ix

    ex de,hl     ; get bitplane, if it's referring to one
    ld d,a
    and $03
    add a,a      ; calculate address of that bitplane
    add a,a      ; = buffer + bitplane * 8
    add a,a
    ld e,a
    ld a,d       ; get method byte back
    ld d,$00
    ld iy,buffer
    add iy,de    ; now iy points to the referred to bitplane
    ex de,hl

    ; now check the method byte
    cp $03       ; %000000pp
    jr c,_DuplicateBitplane
    cp $10
    jr c,_CommonValue
    cp $13       ; %000100pp
    jr c,_DuplicateBitplaneInvert
    cp $20
    jr c,_CommonValue
    cp $23       ; %001000pp
    jr c,_DuplicateBitplanePartial
    cp $40
    jr c,_CommonValue
    cp $43       ; %010000pp
    jr c,_DuplicateBitplanePartialInvert
    ; fall through

_CommonValue:
    ld h,a       ; h = bitmask
    ld l,(ix+0)  ; l = common value
    inc ix
    jr _OutputCommonValue

_RawData:
    ld h,$00     ; empty bitmask; no common value
    jr _OutputCommonValue

_AllTheSame:
    rlc c        ; get next bit into carry
    sbc a,a      ; will make $00 if carry = 0, $ff if it's 1
    ld l,a       ; that's the common value
    ld h,$ff     ; full bitmask
    ; fall through

_OutputCommonValue:
    push bc
      ld b,8     ; loop counter
-:    ld a,l     ; get common value
      rlc h      ; get bit out of bitmask
      jr c,+     ; if 1, use the common value
      ld a,(ix+0); else get it from (ix++)
      inc ix
+:    ld (de),a  ; write to dest
      inc de
      djnz -     ; loop over 8 bytes
    pop bc
  jr _BitplaneDone

_DuplicateBitplane:
    ld hl,$ff00  ; full copy bitmask, empty inversion bitmask
    jr _OutputDuplicate

_DuplicateBitplaneInvert:
    ld hl,$ffff  ; full copy bitmask, full inversion bitmask
    jr _OutputDuplicate

_DuplicateBitplanePartial:
    ld h,(ix+0)  ; get copy bitmask
    ld l,$00     ; empty inversion bitmask
    inc ix
    jr _OutputDuplicate

_DuplicateBitplanePartialInvert:
    ld h,(ix+0)  ; get copt bitmask
    ld l,$ff     ; full inversion bitmask
    inc ix
    ; fall through

_OutputDuplicate:
    push bc
      ld b,8     ; loop counter
-:    ld a,(iy+0); read byte to copy
      inc iy
      xor l      ; apply inversion mask
      rlc h      ; get bit out of bitmask
      jr c,+     ; if 1, use the copied value
      ld a,(ix+0); else get it from (ix++)
      inc ix
+:    ld (de),a  ; write to dest
      inc de
      djnz -     ; loop over 8 bytes
    pop bc
    ; fall through

_BitplaneDone:
    dec b        ; decrement bitplane counter
    jp nz,_DecompressBitplane ; loop if not zero

_OutputTileToVRAM:
    ld a,(vram_ptr)
    out ($bf),a
    ld a,(vram_ptr+1)
    out ($bf),a

    ld de,$0008  ; we are interleaving every 8th byte
    ld c,e       ; counter for the interleaving run
    ld hl,buffer ; point at data to write

--: ld b,4       ; there are 4 bytes to interleave
    push hl
-:    ld a,(hl)  ; read byte
      out ($be),a; write to vram
      add hl,de  ; skip 8 bytes
      djnz -
    pop hl
    inc hl       ; next interleaving run
    dec c
    jr nz,--

    ; Add 32 bytes to vram_ptr
    ld hl,(vram_ptr)
    ld bc,32
    add hl,bc
    ld (vram_ptr),hl

  pop bc
  dec bc         ; next tile
  ld a,b
  or c
  jp nz,_DecompressTile
  ret            ; done

.ends