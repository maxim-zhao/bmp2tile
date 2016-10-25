; Usage:
;
; 1. If you want to decompress to VRAM:
;    .define aPLibToVRAM
; 2. .include this file in your code
; 3. ld hl,<source address>
;    ld de,<destination address> ; e.g. $4000 for VRAM address 0
;    call aPLib_decompress
;
; At most four bytes of stack are used (apart from the call to the decompressor in the first place)
; ROM usage is 336 bytes in VRAM mode, 262 in RAM mode. The extra bytes are the cost 
; of VRAM to VRAM copies, which also makes it pretty slow.
; This file is using WLA-DX syntax quite heavily, you'd better use it too...

; Reader's note:
; The code uses the registers as follows:
; hl  = (input) data pointer. This is incremented as we work through the data, and always points at the next byte to be read.
; de  = (input) destination address. This is incremented after every write.
; a   = the current byte from the bitstream part of the compressed data. 
;       (See https://github.com/maxim-zhao/aplib.py/blob/master/aplib.py for an overview of the format.)
;       Through careful use of the carry flag, it will never be zero until it is time for a new byte to be read in.
;       The shadow a register is used when the code needs to do some 8-bit accumulator work.
; bc  = "variable-length numbers", when they are encountered.
; iy  = last used offset
; ixh = "paired sequence" flag: 1 when not in such a sequence, 0 when we are
; The main loop is _mainLoop, which dispatches off to handlers for the four data types.

; Format note: we do not attempt to handle offsets that exceed 16 bits (for obvious reasons) or lengths exceeding 32000 
; (which are a fourth case for the length-amendment part).

; aPPack decompressor 
; original source by dwedit
; very slightly adapted by utopian
; optimized by Metalbrain

;hl = source
;de = dest

.define VDP_ADDRESS_PORT $bf
.define VDP_DATA_PORT $be

aPLib_decompress:
.ifdef aPLibToVRAM
  ; Set the write address
  ld c, VDP_ADDRESS_PORT
  out (c), e
  out (c), d
.endif

  ; Set up bitstream with only the MSB set. This will trigger a bitstream read.
  ; The code shifts a left and then checks for zero every time a bit is consumed.
  ; If the remaining bits in a are zero, then the carry will always be 1.
  ; This then leads to code which reads another 8 bits in, and immediately rotates through c.
  ; This is guaranteed to insert a set bit at the LSB. Thus, it is possible to know when all the
  ; bits are consumed (a = 0 after shift) even if the bitstream itself is all zeroes.
  ld a, %10000000
  
  ; First byte must be a literal, so fall through into this handler
_emitRawByte:
; *de++ = *hl++
.ifdef aPLibToVRAM
  push af
    ld a,(hl)
    out (VDP_DATA_PORT),a
  pop af
  inc hl
  inc de
.else
	ldi
.endif
  ; Fall through
  
_mainLoop_noPair: ; _mainLoop2
  ld ixh, 1 ; Set "no pair" flag

_mainLoop:
  ; Main loop
  ; a = bitstream, we shift it left into carry
  ; hl = next byte in the stream
  ; de = destination
  ; ixh = "pair" flag, 0 if can use last offset
  ; iy = last used offset
  
  ; Get next bit. See above about how this works...
  add a, a
  jr z, _getBitstream_bit1
  
  ; If we got a zero, emit a raw byte
  jr nc, _emitRawByte
_getBitstream_bit1_set:

  ; Else, look at the next bit. If we have run out of bits, again go to a bespoke handler
  add a, a
  jr z, _getBitstream_bit2
  jr nc, _emitBlock
_getBitstream_bit2_set:

  ; Next bit
  add a, a
  jr z, _getBitstream_bit3
  jr nc, _emitSmallBlock
_getBitstream_bit3_set:

_emitSingleByte:
  ; We read in a 4-bit offset.
  ; We do this by putting a bit in c and rotating bits out of a into it, 
  ; until the first bit comes out. We also zero b here so bc comes out as the final result.
  ld bc, 1<<4

-:add a, a
  jr z, _getBitstream_fourBitNumber
_getBitstream_fourBitNumber_done:
  rl c
  jp nc, -

  jr nz, _emitSingleByte_offset
  
_emitSingleByte_zero:
  ; If the final result in c is zero, it's a literal zero. We zeroed b above.
  ex de, hl
.ifdef aPLibToVRAM
  ld c, VDP_DATA_PORT
  out (c), b
.else
  ld (hl), b ;write a 0
.endif
  ex de, hl
  inc de
  jp _mainLoop_noPair

_getBitstream_bit1:
  ; Read eight more bits of the bitstream
  ld a,(hl)
  inc hl
  ; Check high bit - existing carry goes into LSB
  rla
  ; 1 = continue
  jr c, _getBitstream_bit1_set
  ; 0 = raw byte
  jp _emitRawByte


; we jr here and then jp back, because it is the fastest:
; - the jr condition is met 1/8 of the time; it costs 7 cycles when not met and 12 otherwise, so average 7.625
; - jp condition costs 10 cycles, so the averge cost is higher
; - a plain jp costs 10 cycles, 12 for jr
; - a conditional call costs 17/10 cycles, and ret is 10, so it is worse (and we always return to the same place)
_getBitstream_fourBitNumber:
  ld a, (hl)
  inc hl
  rla
  jp _getBitstream_fourBitNumber_done


_emitSingleByte_offset:
  ; Preserve the bitstream
  ex af, af'
    ; Get the dest into hl for maths
    ex de, hl
.ifdef aPLibToVRAM
    push hl
      sbc hl, bc
      ld c,VDP_ADDRESS_PORT
      out (c),l
      ld a,h
      xor $40
      out (c),a
      in a,(VDP_DATA_PORT)
    pop hl
    out (c),l
    out (c),h
    out (VDP_DATA_PORT),a
.else
    ; Subtract offset. Presumably f' never has carry set?
    sbc hl, bc
    ; Read byte
    ld a, (hl)
    ; Add offset back on
    add hl, bc
    ld (hl), a
.endif
    ex de, hl
  ex af, af'
  inc de
  jp _mainLoop_noPair


_getBitstream_bit3:
  ld a, (hl)
  inc hl
  rla
  jr c, _getBitstream_bit3_set
_emitSmallBlock:
  ld c, (hl) ;use 7 bit offset, length = 2 or 3
  inc hl
  ex af, af'
  rr c
  ret z ;if a zero is found here, it's EOF
  ld a, 2
  ld b, 0
  adc a, b
  push hl
    ld iyh, b
    ld iyl, c
    ld h, d
    ld l, e
    sbc hl, bc
    ld c, a
    ex af, af'
.ifdef aPLibToVRAM
    call _ldir_vram_to_vram
.else
    ldir
.endif
  pop hl
  ld ixh, b ; will be zero
  jp _mainLoop
  
_getBitstream_bit2: ; ap2:
  ; Get bitstream next byte
  ld a, (hl)
  inc hl
  rla
  jr c, _getBitstream_bit2_set
  ; fall through for zero

_emitBlock:
  ; Get the first part of the offset (usually the MSB)
  call _getVariableLengthNumber
  dec c
  ex af, af' ; make a usable for maths
    ld a, c
    sub a,ixh ; will be 1 if we should use the last offset, 0 otherwise (?)
    jr z, ap_r0_gamma ; If we hit zero here the the encoded MSB was 2 and ixh was 1.
    dec a ; Else we subtract another 1 so we are at n-3 (if the "r0" flag was set) or n-2 (otherwise)

    ; Shift into b and get a byte in c
    ld b, a
    ld c, (hl)
    inc hl
    
    ; Save offset
    ld iyh, b
    ld iyl, c

    push bc
      ; Get the length
      call _getVariableLengthNumber_fromShadowA
    ; previous call restores us to normal af
    ; Swap the offset (on the stack) with the source pointer, to preserve the latter and do maths with the former
    ; This is cunning: it's almost equivalent to pop bc; push hl; ld h,b; ld l,c
    ex (sp), hl ;bc = len, hl=offs
    ; We need to amend the length
    ; Range       Amendment
    ; 0..127        +2
    ; 128..1279      0
    ; 1280..31999   +1
    ; 32000+        +2    <-- we don't bother with this
    push de
      ex de, hl ; de = length, hl = dest
      ex af, af' ; To scratch a
        ; First check for <128
        ld hl, 127
        sbc hl, de
        jr c, +
        inc bc
        inc bc
        jp ++
+:      ; Then for >=1280
        ld a, 4
        cp d
        jr nc,++
        inc bc
++:
/*
        ; First check for >= $0500 = 1280
        ; i.e. check d > 4
        ld a, 4
        cp d
        jr nc, +
        ; d > 4 so we are in the third category
        inc bc
        or a ; clear carry
+:      ; Next check for <=127 (will always fail for things that passed the first case)
        ld hl, 127
        sbc hl, de
        jr c, +
        inc bc
        inc bc
+:        
*/        
      pop hl ;bc = len, de = offs, hl=junk
      push hl
        or a
        sbc hl, de
      ex af, af' ; To bitstream a
    pop de ;hl=dest-offs, bc=len, de = dest
.ifdef aPLibToVRAM
    call _ldir_vram_to_vram
.else
    ldir
.endif
  pop hl
  ld ixh, b ; will be 0
  jp _mainLoop


ap_r0_gamma:
  call _getVariableLengthNumber_fromShadowA ;and a new gamma code for length
  push hl
    push de
      ex de, hl
      ld d, iyh
      ld e, iyl
      sbc hl, de
    pop de ;hl=dest-offs, bc=len, de = dest
.ifdef aPLibToVRAM
    call _ldir_vram_to_vram
.else
    ldir
.endif
  pop hl
  ld ixh, b ; will be 0
  jp _mainLoop


_getBitstream_variableLengthNumber_bit1:
  ld a,(hl)
  inc hl
  rla
  jp _getBitstream_variableLengthNumber_bit1_done
_getBitstream_variableLengthNumber_bit1flag:
  ld a, (hl)
  inc hl
  rla
  jp _getBitstream_variableLengthNumber_bit1flag_done
_getBitstream_variableLengthNumber_bit2:
  ld a, (hl)
  inc hl
  rla
  jp _getBitstream_variableLengthNumber_bit2_done
_getBitstream_variableLengthNumber_bit2flag:
  ld a, (hl)
  inc hl
  rla
  jp _getBitstream_variableLengthNumber_bit2flag_done
_getBitstream_variableLengthNumber_bit:
  ld a, (hl)
  inc hl
  rla
  jp _getBitstream_variableLengthNumber_bit_done


_getBitstream_variableLengthNumber_bitflag:
  ld a, (hl)
  inc hl
  rla
  ret nc 
  jp _getVariableLengthNumberloop


_getVariableLengthNumber_fromShadowA:
  ; Variant of the below where we restore the bitstream a bofore we start
  ex af, af'
  
_getVariableLengthNumber:
  ; Reads a number encoded as all the bits of the number after the first 1 bit, 
  ; separated by 1 bits and terminated by a 0. Returns the number in bc.
  ; It has a minimum value of 2, i.e. we always read at tleast two bits.
  ; The most common cases will be shorter numbers, so we have an unrolled loop for the first two bits.
  
  ; Accumulate into bc
  ld bc, 1
  
  ; Bit 1
  add a, a
  jr z, _getBitstream_variableLengthNumber_bit1
_getBitstream_variableLengthNumber_bit1_done:
  rl c
  add a, a
  jr z, _getBitstream_variableLengthNumber_bit1flag
_getBitstream_variableLengthNumber_bit1flag_done:
  ret nc
  
  ; Bit 2
  add a, a
  jr z, _getBitstream_variableLengthNumber_bit2
_getBitstream_variableLengthNumber_bit2_done:
  rl c
  add a, a
  jr z, _getBitstream_variableLengthNumber_bit2flag
_getBitstream_variableLengthNumber_bit2flag_done:
  ret nc
  
  ; Remaining bits
_getVariableLengthNumberloop:
  add a, a
  jr z, _getBitstream_variableLengthNumber_bit
_getBitstream_variableLengthNumber_bit_done:
  rl c
  rl b
  add a, a
  jr z, _getBitstream_variableLengthNumber_bitflag
_getBitstream_variableLengthNumber_bitflag_done:
  ret nc 
  jp _getVariableLengthNumberloop
  
.ifdef aPLibToVRAM
_ldir_vram_to_vram:
  ; Copy bc bytes from VRAM address hl to VRAM address de
  ; Both hl and de are "write" addresses ($4xxx)

  ex af, af'
  ; Make hl a read address
  ld a,h
  xor $40
  ld h,a
  ; Check if the count is below 256
  ld a,b
  or a
  jr z,_below256
  ; Else emit 256*b bytes
-:push bc
    ld c,VDP_ADDRESS_PORT
    ld b,0
    call +
  pop bc
  djnz -
  ; Then fall through for the rest  
_below256:
  ; By emitting 256 at a time, we can use the out (c),r opcode
  ; for address setting, which then relieves pressure on a
  ; and saves some push/pops; and we can use djnz for the loop.
  ld b,c
  ld c,VDP_ADDRESS_PORT
+:
-:out (c),l
  out (c),h
  in a,(VDP_DATA_PORT)
  out (c),e
  out (c),d
  out (VDP_DATA_PORT),a
  inc hl
  inc de
  djnz -
  ex af, af'
  ret
.endif
