; PUCRUNCH unpacker for SMS
; Modified from Jeff Frohwein's GB-Z80 code by Maxim
;
; Usage:
; Uncrunch needs 44 bytes of RAM to operate in.
; You can tell it what RAM to use by .defining a constant (with any of
;   .define, .enum, .ramsection or any other way you can think)
;   called PucrunchVars, before you .include this file.
; If you don't it'll raise a compilation error.
; If you want to decompress to VRAM, .define PuCrunchToVRAM before including 
; this file, and pass a destination with the VRAM control bits set for 
; writing (i.e. $4000 | address).
;
; ld hl,srcdataoffset
; ld de,destdataoffset
; call Uncrunch
;
; To compress the data, you need to run it through pucrunch. You can get
;   an executable from the Pucrunch website:
;
;     http://www.cs.tut.fi/~albert/Dev/pucrunch/
;
;   or use the one I've included, which is for Win32+x86. You also need
;   to specify that you only want the compressed data, with no
;   decompressor, by using the following commandline:
;
;     pucrunch.exe -d -c0 <source file> <destination file>
;
;   Both have to be pure binary data.
;
; To do:
; - maybe look at making it smaller, since I'm aiming for the MiniGame
;   Compo with this.
;
; Now follows the original comments in the file:

;*
;*
;* PUCRUNCH unpacker for GB
;*   Modeled after Pasi Ojala's C64 code.
;*
;*   Written in RGBDS
;*
;*  V1.0 - Ported to GB by Jeff Frohwein, started 22-Jul-99
;*  V1.1 - Various optimizations, 23-Jul-99
;*  V1.2 - Even more optimizations, 23-Jul-99
;*  V1.3 - Fixed a bug in the code. 256 byte copy didn't work. 24-Feb-00
;*
;* Note: If you unpack to VRAM than the screen needs to be
;* turned off because no checks for VRAM available are made.

; Pucrunch file format

;;; db INPOS low     (endAddr + overlap - size) & 0xff
;;; db INPOS high    (endAddr + overlap - size) >> 8
;;; db 'p'
;;; db 'u'
;;; db (endAddr - 0x100) & 0xff
;;; db (endAddr - 0x100) >> 8
; db escape>>(8-escBits)
;;; db (start & 0xff) (OUTPOS low)
;;; db (start >> 8) (OUTPOS high)
; db escBits
; db maxGamma + 1
; db (1<<maxGamma); /* Short/Long RLE */
; db extraLZPosBits;
;;; db (exec & 0xff)
;;; db (exec >> 8)
; db rleUsed (31)  ;needed
; ds rleUsed
;  ....data....

.ifndef PucrunchVars
.printt "Error: you haven't defined PucrunchVars!\n";
.fail
.endif

.enum PucrunchVars export
escPu     db
OutPtr    dw
lzpos     dw
EscBits   db
Esc8Bits  db
MaxGamma  db
Max1Gamma db
Max2Gamma db
Max8Gamma db
ExtraBits db
tablePu   ds 31
regy      db
.ende

; HL = InPtr
; D = bitstr
; E = X
; BC = temps

; ****** Unpack pucrunch data ******
; Entry: HL = Source packed data
;        DE = Destination for unpacked data

.section "Pucrunch unpacker" free
.block "Pucrunch" ; for finding out the compiled size
Uncrunch:
  ld (OutPtr),de
  ld c,$bf
  out (c),e
  out (c),d

; Read the file header & setup variables

  ld bc,6                                                   ; 01 06 00
  add hl,bc                                                 ; 09

  ld a,(hl)                                                 ; 7E
  ld (escPu),a                                              ; 32 C4 DF

  inc hl                                                    ; 23
  inc hl                                                    ; 23
  inc hl                                                    ; 23
  ld a,(hl)                                                 ; 7E

  ld (EscBits),a                                            ; 32 C9 DF

  ld a,8                                                    ; 3E 08    saved 1 byte
  sub (hl)                                                  ; 96

  ld (Esc8Bits),a                                           ; 32 CA DF

  inc hl                                                    ; 23
  ld a,(hl)                                                 ; 7E
  ld (MaxGamma),a                                           ; 32 CB DF

  ld a,8                                                    ; 3E 08    saved 1 byte
  sub (hl)                                                  ; 96
  inc a                                                     ; 3C

  ld (Max8Gamma),a                                          ; 32 CE DF

  inc hl                                                    ; 23
  ld a,(hl)                                                 ; 7E
  ld (Max1Gamma),a                                          ; 32 CC DF
  add a,a                                                   ; 87
  dec a                                                     ; 3D
  ld (Max2Gamma),a                                          ; 32 CD DF

  inc hl                                                    ; 23
  ld a,(hl)                                                 ; 7E
  ld (ExtraBits),a                                          ; 32 CF DF

  inc hl                                                    ; 23
  inc hl                                                    ; 23
  inc hl                                                    ; 23
; Copy the RLE table (maximum of 31 bytes) to RAM
  ld c,(hl)                                                 ; 7E

  inc hl                                                    ; 23

  ld de,tablePu                                             ; 11 D0 DF
  ld b,0
  ldir

  ld d,$80                                                  ; 16 80

  jr _main                                                  ; 18 2A


_newesc:
  ld b,a                                                    ; 47

  ld a,(escPu)                                              ; 3A C4 DF
  ld (regy),a                                               ; 32 EF DF

  ld a,(EscBits)                                            ; 3A C9 DF
  ld e,a                                                    ; 5F

  ld a,b                                                    ; 78

  inc e                                                     ; 1C

  call _getchk                                              ; CD 3D 06

  ld (escPu),a                                              ; 32 C4 DF
  ld a,(regy)                                               ; 3A EF DF

  ; Fall through and get the rest of the bits.

_noesc:
  ld b,a                                                    ; 47

  ld a,(Esc8Bits)                                           ; 3A CA DF
  ld e,a                                                    ; 5F

  ld a,b                                                    ; 78

  inc e                                                     ; 1C

  call _getchk                                              ; CD 3D 06

; Write out the escaped/normal byte

  ld bc,(OutPtr)                                            ; ED 4B C5 DF
.ifdef PuCrunchToVRAM
  out ($be),a
.else
  ld (bc),a                                                 ; 02
.endif
  inc bc                                                    ; 03
  ld (OutPtr),bc                                            ; ED 43 C5 DF

  ; Fall through and check the escape bits again

_main:
  ld a,(EscBits)                                            ; 3A C9 DF
  ld e,a                                                    ; 5F

  xor a ; A = 0                                             ; AF
  ld (regy),a                                               ; 32 EF DF

  inc e                                                     ; 1C

  call _getchk   ; X=2 -> X=0                               ; CD 3D 06

  ld b,a                                                    ; 47
  ld a,(escPu)                                              ; 3A C4 DF
  cp b                                                      ; B8
  ld a,b                                                    ; 78

  jr nz,_noesc  ; Not the escape code -> get the rest of the byte ; 20 D8

  ; Fall through to packed code

  call _getval    ; X=0 -> X=0                              ; CD 1B 06

  ld (lzpos),a    ; xstore - save the length for a          ; 32 C7 DF
                  ;   later time

  srl a           ; cmp #1  ; LEN == 2 ? (A is never 0)     ; CB 3F
  jr nz,_lz77     ; LEN != 2 -> LZ77                        ; jp -> jr -> saved 1 byte

  call _get1bit   ; X=0 -> X=0                              ; CD 33 06

  srl a           ; bit -> C, A = 0                         ; CB 3F

  jr nc,_lz77_2   ; A=0 -> LZPOS+1  LZ77, len=2             ; jp -> jr -> saved 1 byte

	; e..e01
  call _get1bit   ; X=0 -> X=0                              ; CD 33 06
  srl a           ; bit -> C, A = 0                         ; CB 3F
  jr nc,_newesc   ; e..e010 New Escape                      ; jp -> jr -> saved 1 byte

	; e..e011				Short/Long RLE
  ld a,(regy)     ; Y is 1 bigger than MSB loops            ; 3A EF DF
  inc a                                                     ; 3C
  ld (regy),a                                               ; 32 EF DF

  call _getval    ; Y is 1, get len,  X=0 -> X=0            ; CD 1B 06
  ld (lzpos),a    ; xstore - Save length LSB                ; 32 C7 DF

  ld c,a                                                    ; 4F

  ld a,(Max1Gamma)                                          ; 3A CC DF
  ld b,a                                                    ; 47

  ld a,c                                                    ; 79

  cp b            ; ** PARAMETER 63-64 -> C set,            ; B8
                  ; 64-64 -> C clear..

  jr c,_chrcode   ; short RLE, get bytecode                 ; 38 12

	; Otherwise it's long RLE
_longrle:
  ld b,a                                                    ; 47
  ld a,(Max8Gamma)                                          ; 3A CE DF
  ld e,a          ; ** PARAMETER  111111xxxxxx              ; 5F
  ld a,b                                                    ; 78

  call _getbits   ; get 3/2/1 more bits to get a full byte, ; CD 34 06
                  ;   X=2 -> X=0
  ld (lzpos),a    ; xstore - Save length LSB                ; 32 C7 DF

  call _getval    ; length MSB, X=0 -> X=0                  ; CD 1B 06

  ld (regy),a     ; Y is 1 bigger than MSB loops            ; 32 EF DF

_chrcode:
  call _getval    ; Byte Code,  X=0 -> X=0                  ; CD 1B 06

  ld e,a                                                    ; 5F

  add a,(tablePu-1) # 256                                   ; C6 xx ; saved 1 byte
  ld c,a                                                    ; 4F

  ld a,(tablePu-1) / 256                                    ; 3E xx
  adc a,0                                                   ; CE 00
  ld b,a                                                    ; 47

  ld a,e                                                    ; 7B
  cp 32           ; 31-32 -> C set, 32-32 -> C clear..      ; FE 20
  ld a,(bc)                                                 ; 0A
  jr c,_less32    ; 1..31                                   ; 38 06

	; Not ranks 1..31, -> 11111 ° xxxxx (32..64), get byte..

  ld a,e          ; get back the value (5 valid bits)       ; 7B

  ld e,3                                                    ; 1E 03

  call _getbits   ; get 3 more bits to get a full byte,     ; CD 34 06
                  ; X=3 -> X=0

_less32:
  push hl                                                   ; E5
    push af                                                 ; F5
      ld a,(lzpos)                                          ; 3A C7 DF
      ld e,a          ; xstore - get length LSB             ; 5F

      ld b,e                                                ; 43
      inc b           ; adjust for cpx#$ff;bne -> bne       ; 04

      ld a,(regy)                                           ; 3A EF DF
      ld c,a                                                ; 4F

      ld hl,(OutPtr)                                        ; 2A C5 DF
    pop af                                                  ; F1

_dorle:
.ifdef PuCrunchToVRAM
    out ($be),a
.else
    ld (hl),a                                               ; 77
.endif
    inc hl                                                  ; 23

    djnz _dorle                                             ; 10 nn  ; saved 1 byte

    dec c                                                   ; 0D
    jr nz,_dorle    ; Y was 1 bigger than wanted originally ; 20 F8

    ld (OutPtr),hl                                          ; 22 C5 DF

  pop hl                                                    ; E1
  jp _main                                                  ; C3 2D 05

_lz77:
  call _getval    ; X=0 -> X=0                              ; CD 1B 06

  ld b,a                                                    ; 47

  ld a,(Max2Gamma)                                          ; 3A CD DF
  cp b            ; end of file ?                           ; B8
  ret z           ; yes, exit                               ; C8

  ld a,(ExtraBits) ; ** PARAMETER (more bits to get)        ; 3A CF DF
  ld e,a                                                    ; 5F

  ld a,b                                                    ; 78

  dec a           ; subtract 1  (1..126 -> 0..125)          ; 3D

  inc e                                                     ; 1C

  call _getchk    ;f  ; clears Carry, X=0 -> X=0            ; CD 3D 06

_lz77_2:
  ld (lzpos+1),a  ; offset MSB                              ; 32 C8 DF

  ld e,8                                                    ; 1E 08

  call _getbits   ; clears Carry, X=8 -> X=0                ; CD 34 06

  ; Note: Already eor:ed in the compressor..
  ld b,a                                                    ; 47

  ld a,(lzpos)                                              ; 3A C7 DF
  ld e,a          ; xstore - LZLEN (read before it's        ; 5F
                  ;   overwritten)

  ld a,(OutPtr)                                             ; 3A C5 DF
  add a,b         ; -offset -1 + curpos (C is clear)        ; 80
  ld (lzpos),a                                              ; 32 C7 DF

  ld a,(lzpos+1)                                            ; 3A C8 DF
  ld b,a                                                    ; 47

  ld a,(OutPtr+1)                                           ; 3A C6 DF
  ccf                                                       ; 3F
  sbc a,b                                                   ; 98
  ld (lzpos+1),a  ; copy X+1 number of chars from LZPOS to  ; 32 C8 DF
                  ; OUTPOS
  inc e           ; adjust for cpx#$ff;bne -> bne           ; 1C

  ; Write decompressed bytes out to RAM
  ld b,e                                                    ; 43

  push de                                                   ; D5
  push hl                                                   ; E5
  
    ld hl,(lzpos)                                           ; 2A C7 DF
.ifdef PuCrunchToVRAM
    ld a,h
    xor $40
    ld h,a
.endif
    ld de,(OutPtr)                                          ; ED 5B C5 DF

.ifdef PuCrunchToVRAM
    ld c,$bf
.endif

    ld a,b                                                  ; 78
    or a            ; Is it zero?                           ; B7
    jr z,_zero      ; yes                                   ; 28 19

    inc b                                                   ; 04
    srl b                                                   ; CB 38
    jr nc,_olzloop                                          ; 30 04

_lzloop:
.ifdef PuCrunchToVRAM
    out (c),l
    out (c),h
    in a,($be)
    out (c),e
    out (c),d
    out ($be),a
    inc hl
    inc de
.else
    ld a,(hl)                                               ; 7E
    inc hl                                                  ; 23
    ld (de),a                                               ; 12
    inc de                                                  ; 13
.endif
_olzloop:
.ifdef PuCrunchToVRAM
    out (c),l
    out (c),h
    in a,($be)
    out (c),e
    out (c),d
    out ($be),a
.else
    ld a,(hl)       ; Note: Must be copied forward          ; 7E
    ld (de),a                                               ; 12
.endif
    inc hl                                                  ; 23
    inc de                                                  ; 13
    djnz _lzloop                                            ; 10 <offset>    saved 1 byte

    ld (OutPtr),de                                          ; ED 53 C5 DF

  pop hl                                                    ; E1
  pop de                                                    ; D1
  jp _main                                                  ; C3 2D 05

_zero:
  ld b,128                                                  ; 06 80
  jr _lzloop                                                ; 18 E8

; getval : Gets a 'static huffman coded' value
; ** Scratches X, returns the value in A **
_getval: ; X must be 0 when called!
  ld a,1                                                    ; 3E 01
  ld e,a                                                    ; 5F
_loop0:
  sla d                                                     ; CB 22

  jr nz,_loop1                                              ; 20 04

  ld d,(hl)                                                 ; 56
  inc hl                                                    ; 23

  rl d ; Shift in C=1 (last bit marker)                     ; CB 12
       ; bitstr initial value = $80 == empty
_loop1:
  jr nc,_getchk ; got 0-bit                                 ; 30 15

  inc e                                                     ; 1C

  ld b,a  ; save a                                          ; 47

  ld a,(MaxGamma)                                           ; 3A CB DF
  cp e                                                      ; BB

  ld a,b  ; restore a                                       ; 78

  jr nz,_loop0                                              ; 20 ED

  jr _getchk                                                ; 18 0A


; getbits: Gets X bits from the stream
; ** Scratches X, returns the value in A **

_get1bit:
  inc e                                                     ; 1C
_getbits:
  sla d                                                     ; CB 22

  jr nz,_loop3                                              ; 20 04

  ld d,(hl)                                                 ; 56
  inc hl                                                    ; 23

  rl d ; Shift in C=1 (last bit marker)                     ; CB 12
       ; bitstr initial value = $80 == empty
_loop3:
  rla                                                       ; 17
_getchk:
  dec e                                                     ; 1D

  jr nz,_getbits                                            ; 20 F4
;  or a  ; clear carry flag                                  ; B7
  ret                                                       ; C9

.endb
.ends
