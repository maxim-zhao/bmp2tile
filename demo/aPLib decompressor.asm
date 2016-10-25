; Usage:
;
; 1. Define a symbol "aPLibMemory" which points to the start of 5 bytes of RAM.
; 2. If you want to decompress to VRAM:
;    .define aPLibToVRAM
; 3. .include this file in your code
; 4. ld hl,<source address>
;    ld de,<destination address> ; e.g. $4000 for VRAM address 0
;    call aPLib_decompress
;
; The stack is used a bit, I never saw more than 12 bytes used.
; ROM usage is 297 bytes in VRAM mode, 242 in RAM mode. The extra bytes are the cost 
; of VRAM to VRAM copies, which also makes it pretty slow.
; This file is using WLA-DX syntax quite heavily, you'd better use it too...

.define calcblocks
; comment out this line to suppress the block size notifications
; (useful for optimising to see size changes)

.struct aPLibMemoryStruct
bits     db ; A bitmask for the bit to read next. It is initialised to 1, and rotated right each time a bit is read. When the 1 falls into the carry, the next byte is read into "byte".
byte     db ; not directly referenced, assumed to come after bits
LWM      db ; Flag for LZ offset reuse, 1 if the last operation set it. We only reuse it if the last operation *didn't* set it.
R0       dw ; Last used LZ offset
.endst

.enum aPLibMemory
mem instanceof aPLibMemoryStruct
.ende

; Reader's note:
; The structure of the code has been arranged such that the entry point is in the middle -
; this is so it can use jr to branch out to the various subsections to save a few bytes,
; but it makes it somewhat harder to read. "depack" is the entry point and "aploop" is
; the main loop.

.section "aPLib" free
.ifdef calcblocks
.block "aPLib"
.endif

; Gets a bit from the bitstream into the Z flag
; Always leaves carry flag unset
_getBit:
	push bc
    ; Get bitmask + value
		ld bc,(mem.bits)
    ; Rotate bitmask
		rrc c
		jr nc,+
    ; If the bitmask fell into the carry, we need a new byte
		ld b,(hl)
		inc hl
+:	; Then mask to the bit we want - so the result is in the Z flag
    ld a,c
		and b
    ; Save the state
		ld (mem.bits),bc
	pop bc
	ret

; Shifts a bit from the bitstream into bc
_getBit_bc:
  ; Shift bc left by 1
	sla c
	rl b
  ; Get a bit from the bitstream
	call _getBit
  ; Add it to bc if 1
	ret z
	inc bc
	ret

; Gets a variable-length number from the bitstream
_getVariableLengthNumber:
  ; Implicit high bit
	ld bc,1
-:call _getBit_bc ; Shift in following bits
	call _getBit ; Until we hit a 0 indicator bit
	jr nz,-
	ret

; Emit a byte of data
_literal:
.ifdef aPLibToVRAM
  ld a,(hl)
  out ($be),a
  inc hl
  inc de
.else
	ldi
.endif
  ; Clear LWM
	xor a
	ld (mem.LWM),a
	jr _mainLoop

; Emit an LZ block
_block:
  ; Get the offset MSB. The variable-length encoding means it's stored +2
	call _getVariableLengthNumber
	dec bc
	dec bc

  ; Check for the LWM flag. If non-zero, we need to continue to read the offset - but the MSB is stored as +2.
	ld a,(mem.LWM)
	or a
	jr nz,++
  
  ; Check the offset MSB. If not zero, we need to continue to read the offset - and we need to subtract another 1 from it.
  ; Could optimise to ignore b here, we'll overflow if it's too large anyway
	ld a,b
	or c
	jr nz,+
  
_block_reuseOffset:  
  ; If we get here then we're re-using the LZ offset.
  ; Get the length
	call _getVariableLengthNumber
  ; Copy LZ run
	push hl
		ld h,d
		ld l,e
		push bc
			ld bc,(mem.R0)
			sbc hl,bc
		pop bc
.ifdef aPLibToVRAM
    call _ldir_vram_to_vram
.else
    ldir
.endif
	pop hl
  ; Done
	jr +++
  
_block_getOffsetLSB_MSBisPlus3:
+:; The MSB is stored +3, we need to decrement once more
  dec bc
  
_block_getOffsetLSB:
++:
  ; Shift the MSB into b and get the LSB in c
	ld b,c
	ld c,(hl)
	inc hl
  ; Save it for possible later reuse
	ld (mem.R0),bc
	push bc
    ; Get the length in bc
		call _getVariableLengthNumber
    ; Get the offset into hl and save the data pointer in the stack.
    ; This is a bit cunning because the preceding line may have modified hl, and we needed to preserve bc from before it.
		ex (sp),hl
    ; Check for ranges of values and increase the length accordingly
    ;      Offset    Adjustment
    ;     0..  127       2
    ;   128.. 1279       0
    ;  1280..31999       1
    ; 32000..            2
    ; Could optimise for the common cases (shorter offsets), 32K will almost never be seen on Z80, and bail after range is found
		push de
			ex de,hl
			;some comparison junk for some reason
			; Maxim: optimised to use add instead of sbc
      ld hl,-32000
      add hl,de
			jr nc,+
			inc bc
+:    ld hl,-1280
      add hl,de
			jr nc,+
			inc bc
+:    ld hl,-128
      add hl,de
			jr c,+
			inc bc
			inc bc
+:	pop hl
    ; Apply offset to current output pointer
		push hl
			or a
			sbc hl,de
		pop de
    ; now hl = LZ source, de = dest, bc = count
.ifdef aPLibToVRAM
    call _ldir_vram_to_vram
.else
		ldir
.endif
  ; Restore data pointer
	pop hl

+++:
  ; Set the LWM flag
	ld a,1
	ld (mem.LWM),a
	jr _mainLoop

aPLib_decompress:
	;hl = source
	;de = dest (VRAM address with write bit set)
  ld c,$bf
  out (c),e
  out (c),d

	; ldi
  ld a,(hl)
  out ($be),a
  inc hl
  inc de
  
	xor a ; Initialise LWM to 0
	ld (mem.LWM),a
	inc a ; Initialise bits to 1
	ld (mem.bits),a

_mainLoop:
	call _getBit
	jr z, _literal
	call _getBit
	jr z, _block
	call _getBit
	jr z, _shortBlock
  ; Fall through
  
_singleByte:
	; Clear the LWM flag
	xor a
	ld (mem.LWM),a
	; Read the four-bit offset
	ld bc,0
	call _getBit_bc
	call _getBit_bc
	call _getBit_bc
	call _getBit_bc
  ; Check for zero
	ld a,b
	or c
	jr nz, _singleByte_nonZeroOffset
_singleByte_zeroOffset:
  ; Zero offset means just emit a zero
  ; a is already 0 here
.ifdef aPLibToVRAM
  out ($be),a
.else
  ld (de),a
.endif
  inc de
	jr _mainLoop

_singleByte_nonZeroOffset:
  ; bc = offset
  ; Swap source and destination pointers
	ex de,hl
    push hl
      ; Subtract offset
      sbc hl,bc
      ; Read byte
.ifdef aPLibToVRAM
      ld c,$bf
      out (c),l
      ld a,h
      xor $40
      out (c),a
      in a,($be)
.else
      ld a,(hl)
.endif
    pop hl
    ; Then emit it
.ifdef aPLibToVRAM
    out (c),l
    out (c),h
    out ($be),a
.else
    ld (hl),a
.endif
    inc hl
  ; Swap pointers back again
	ex de,hl
	jr _mainLoop

; Emit an LZ block encoded in a single byte - or end
_shortBlock:
  ; Get the byte
  ; High 7 bits = offset
  ; Low bit = length - 2
	ld c,(hl)
	inc hl
  ; Shift offset. Carry is always unset here
	rr c
  ; Zero offset means end of compressed data
	ret z
  ; Use the carry flag to get the length (2 or 3) in b
	ld b,2
	jr nc,+
	inc b
+:
  ; Set the LWM flag
	ld a,1
	ld (mem.LWM),a

  ; Calculate the source address
	push hl
		ld a,b
    ; Save the offset for future use
		ld b,0
		ld (mem.R0),bc
    ; Subtract the offset from the pointer
		ld h,d
		ld l,e
		or a
		sbc hl,bc
    ; Get the byte count in bc
		ld c,a
    ; Copy data
.ifdef aPLibToVRAM
    call _ldir_vram_to_vram
.else
		ldir
.endif
	pop hl
  ; Done
	jr _mainLoop
  
.ifdef aPLibToVRAM
_ldir_vram_to_vram:
  ; Copy bc bytes from VRAM address hl to VRAM address de
  ; Both hl and de are "write" addresses ($4xxx)

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
    ld c,$bf
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
  ld c,$bf
+:
-:out (c),l
  out (c),h
  in a,($be)
  out (c),e
  out (c),d
  out ($be),a
  inc hl
  inc de
  djnz -
  ret
.endif
  
.ifdef calcblocks
.endb
.endif
.ends
