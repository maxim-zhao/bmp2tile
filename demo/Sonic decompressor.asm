; Sonic 1 tile decompressor
; 
; Needs 8 bytes of RAM for temporary storage. Define Sonic1TileLoaderMemory as the start address of the RAM to use.

.block "TileLoaderSonic1"
.section "Tile loader (Sonic 1)" free

; RAM usage
.enum Sonic1TileLoaderMemory export
Sonic1TileLoader_StartOfData dw
Sonic1TileLoader_RowCount dw
Sonic1TileLoader_UniqueRowsData dw
Sonic1TileLoader_ArtData dw
.ende

; A definition
.define SMS_VDP_DATA $be

Sonic1TileLoader_Decompress:
; Arguments:
; hl = data address
; de = VDP write address
; Uses af,bc,de,bc',de',hl'
; Note: this has been slightly modified from what is found in Sonic 1, for size and speed, plus it sets the VRAM address internally.

; See http://info.sonicretro.org/SCHG:Sonic_the_Hedgehog_%288-bit%29#Header for details on the format

    ld c,$bf
    out (c),e
    out (c),d

    ld (Sonic1TileLoader_StartOfData),hl
    
    ; Skip the "48 59" art header marker
    inc hl
    inc hl
    
    ; Read in the various offsets and convert to pointers:
    ; - BC is the row count, this counts down to zero as we process each row
    ; - DE points into the art data
    ; - DE' points into the duplicate rows data (per-tile bitmasks)
    ; - HL' points at the start of the art data
    

    ; Read the DuplicateRows offset into DE and save for later
    ld e,(hl)
    inc hl
    ld d,(hl)
    inc hl
    push de
        ; Read the ArtData offset into DE and save for later
        ld e,(hl)
        inc hl
        ld d,(hl)
        push de
            ; Read the row count into BC
            inc hl
            ld c,(hl)
            inc hl
            ld b,(hl)
            inc hl

            ld (Sonic1TileLoader_RowCount),bc       ; Store the row count
            ld (Sonic1TileLoader_UniqueRowsData),hl ; Where the UniqueRows list begins

            ; swap BC/DE/HL with their shadow values
            exx

            ; load DE with the absolute starting address of the art header; the DuplicateRows and ArtData values are always relative to this
            ld de,(Sonic1TileLoader_StartOfData)
        pop hl ; pull the ArtData value from the stack
        add hl,de ; get the absolute address of ArtData
        ld (Sonic1TileLoader_ArtData),hl ; and save it
        ; copy it to BC. this will be used to produce a counter from 0 to RowCount
        ld c,l
        ld b,h
    pop hl ; load HL with the DuplicateRows offset
    add hl,de ; get the absolute address of DuplicateRows

    ; swap DE & HL. DE will now be the DuplicateRows absolute address,
    ; and HL will be the absolute address of the art header
    ex de,hl

    ; now swap the original values back,
    ; BC will be the row counter
    ; DE will be the ArtData value
    exx

_processRow:
    ld hl,(Sonic1TileLoader_RowCount) ; load HL with the original row count number
    xor a       ; set A to 0 (Carry is reset)
    sbc hl,bc   ; subtract current counter from the row count - that is, count upwards from 0
    push hl
        ; get the row number in the current tile (0-7):
        ld d,a          ; zero-out D
        ld a,l          ; load A with the lo-byte of the counter
        and %00000111   ; clip to the first three bits, that is, "mod 8" it so it counts 0-7
        ld e,a          ; load E with this value, making it a 16-bit number in DE
        ld hl,_rowIndexTable
        add hl,de
        ld a,(hl)       ; get the bit mask for the particular row
    pop de

    ; divide the counter by 4
    srl d
    rr e
    srl d
    rr e
    srl d
    rr e

    ld hl,(Sonic1TileLoader_UniqueRowsData) ; the absolute address where the UniqueRows list begins
    add hl,de   ; add the counter, so move along to the DE'th byte in the UniqueRows list
    ld e,a 
    ld a,(hl)   ; read the current byte in the UniqueRows list
    and e       ; test if the masked bit is set
    jr nz,_duplicateRow ; if the bit is set, it's a duplicate row, otherwise continue for a unique row

_uniqueRow:
    ; swap back the BC/DE/HL shadow values
    ; BC will be the pointer to the ArtData
    exx
        ; write 1 row of pixels (4 bytes) to the VDP
        ld a,(bc)
        out (SMS_VDP_DATA),a
        inc bc
        ld a,(bc)
        out (SMS_VDP_DATA),a
        inc bc
        ld a,(bc)
        out (SMS_VDP_DATA),a
        inc bc
        ld a,(bc)
        out (SMS_VDP_DATA),a
        inc bc
    exx
    jr _endOfRow

_duplicateRow:

    ; swap in the BC/DE/HL shadow values
    ; DE will be the pointer to the DuplicateRows data
    exx
        ld a,(de) ; read a byte from the duplicate rows list
        inc de ; move to the next byte
    exx

    ; HL will be re-purposed as the index into the art data
    ld h,0
    ; Check if the byte from the duplicate rows list begins with $f. This is used as a marker to specify a two-byte number for indexes over 256.
    cp $f0
    jr c,+      ; if less than $f0, use it as-is

    sub $f0     ; else, strip the $f0, i.e $f3 -> $03
    ld h,a      ; and set as the hi-byte for the art data index
    exx         ; fetch the next byte into A
        ld a,(de)
        inc de
    exx

+:  ; multiply the duplicate row's index number to the art data by 4 - each row of art data is 4 bytes
    ld l,a
    add hl,hl 
    add hl,hl

    ld de,(Sonic1TileLoader_ArtData) ; get the absolute address to the art data
    add hl,de ; add the index from the duplicate row list

    ; write 1 row of pixels (4 bytes) to the VDP
    ld a,(hl) 
    out (SMS_VDP_DATA),a
    inc hl
    ld a,(hl)
    out (SMS_VDP_DATA),a
    inc hl
    ld a,(hl)
    out (SMS_VDP_DATA),a
    inc hl
    ld a,(hl)
    out (SMS_VDP_DATA),a
    inc hl

_endOfRow:    
    ; decrease the remaining row count
    dec bc

    ; check if all rows have been done
    ld a,b
    or c
    jr nz,_processRow
    ret

_rowIndexTable:
.db %00000001
.db %00000010
.db %00000100
.db %00001000
.db %00010000
.db %00100000
.db %01000000
.db %10000000

.ends
.endb
