unit Unit1;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, ExtCtrls, ComCtrls, Math, ShellAPI, XPMan;

type
  TForm1 = class(TForm)
    GroupBox1: TGroupBox;
    FileName: TEdit;
    btnLoad: TButton;
    PageControl1: TPageControl;
    TabSheet1: TTabSheet;
    gb1bit: TGroupBox;
    cbInvert: TCheckBox;
    btnProcess: TButton;
    gb4bit: TGroupBox;
    Label2: TLabel;
    rb2bit: TRadioButton;
    rb3bit: TRadioButton;
    rb4bit: TRadioButton;
    rb1bit: TRadioButton;
    XPManifest1: TXPManifest;
    TabSheet3: TTabSheet;
    mmResults: TRichEdit;
    imgOriginal: TImage;
    imgColumn: TImage;
    btnRemoveDupes: TButton;
    btnSave: TButton;
    TabSheet2: TTabSheet;
    mmReconstData: TRichEdit;
    Label3: TLabel;
    edTileOffset: TEdit;
    cbPad: TCheckBox;
    edBlankTile: TEdit;
    cbBytes: TCheckBox;
    cbSpritePalette: TCheckBox;
    cbInFront: TCheckBox;
    btnSaveReconst: TButton;
    StatusBar1: TStatusBar;
    cbRemoveDupes: TCheckBox;
    cbUseMirroring: TCheckBox;
    TabSheet4: TTabSheet;
    mmPalette: TMemo;
    btnLoadPalette: TButton;
    rbPalHex: TRadioButton;
    rbPalConst: TRadioButton;
    lblNumColours: TLabel;
    rbPalGG: TRadioButton;
    imgPalette: TImage;
    Bevel1: TBevel;
    SaveDialog1: TSaveDialog;
    Label1: TLabel;
    btnSavePalette: TButton;
    procedure btnLoadClick(Sender: TObject);
    procedure btnProcessClick(Sender: TObject);
    procedure btnSaveClick(Sender: TObject);
    procedure rb2bitClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
//    procedure btnBitsToBytesClick(Sender: TObject);
    procedure btnRemoveDupesClick(Sender: TObject);
    procedure btnSaveReconstClick(Sender: TObject);
    procedure edTileOffsetChange(Sender: TObject);
    procedure btnLoadPaletteClick(Sender: TObject);
    procedure WriteReconstData;
    procedure btnSavePaletteClick(Sender: TObject);
  private
    { Private declarations }
    procedure WMDROPFILES(var Message: TWMDROPFILES); message WM_DROPFILES;
    procedure DisplayHint(Sender: TObject);
  public
    { Public declarations }
  end;

var
  Form1: TForm1;

implementation

{$R *.DFM}

var
  ReconstData:string;
  HighestTileIndex:integer;
  OldHint:string;
  NumColours:integer;

procedure SetHint(s:string);
begin
  OldHint:=s;
  Form1.StatusBar1.SimpleText:=s;
end;

procedure EnableGroupBox(const gb:TGroupBox;const enabled:boolean);
var
  i:integer;
begin
  gb.Enabled:=enabled;
  for i:=0 to gb.ControlCount-1 do
    gb.Controls[i].Enabled:=enabled;
end;

procedure TForm1.btnLoadClick(Sender: TObject);
var
  bm:TBitmap;
  w,h,x,y:integer;
  p:pByteArray;
begin
  // Exit if file does not exist
  if not fileexists(FileName.Text) then begin
    Application.MessageBox('File not found',nil,MB_OK+MB_ICONERROR);
    exit;
  end;

  // Load bitmap
  bm:=TBitmap.Create;
  try
    bm.LoadFromFile(FileName.Text);

    if (bm.Width>256)
    or (bm.Height>256)
    then begin
      Application.MessageBox('Bitmap''s dimensions exceed possible tilemap area (usually 256x224, 256x256 in stretched modes)!',nil,MB_ICONERROR);
      exit;
    end;
    // Check bitmap pixel format
    case bm.PixelFormat of
    pf1bit:begin EnableGroupBox(gb1bit,true ); EnableGroupBox(gb4bit,false); end;
    pf4bit:begin EnableGroupBox(gb1bit,false); EnableGroupBox(gb4bit,true);  end;
    else
      EnableGroupBox(gb1bit,false);
      EnableGroupBox(gb4bit,false);
      Application.MessageBox('Bitmap is not 1 bit or 4 bit! Can''t process it.',nil,MB_ICONERROR);
      Exit;
    end;

    // Check bitmap dimensions
    if ((bm.width  mod 8)>0)
    or ((bm.height mod 8)>0)
    then begin
      Application.MessageBox('Bitmap''s dimensions are not multiples of 8! Adjusting...',nil,MB_ICONWARNING);
      bm.width :=ceil(bm.width /8)*8;
      bm.height:=ceil(bm.height/8)*8;
    end;

    w:=bm.width  div 8;
    h:=bm.height div 8;

    with imgColumn.Picture.Bitmap do begin
      Assign(bm);
      Width:=8;
      Height:=w*h*8;
      for y:=0 to h-1 do
        for x:=0 to w-1 do
          Canvas.CopyRect(
            Rect(0,(y*w+x)*8,8,(y*w+x)*8+8),
            bm.Canvas,
            Rect(x*8,y*8,x*8+8,y*8+8)
          );
    end;
    imgOriginal.Center:=((bm.width<imgOriginal.width) and (bm.height<imgOriginal.height));
    imgOriginal.Stretch:=not imgOriginal.Center;
    imgOriginal.picture.assign(bm);
  finally
    bm.free;
  end;

  // Check how many colours are used if 4bpp
  if gb4bit.Enabled then begin
    h:=0; // h = highest
    for y:=0 to imgColumn.Picture.Bitmap.height-1 do begin
      p:=imgColumn.Picture.Bitmap.ScanLine[y];
      Move(p[0],w,4); // w contains 4 bytes = 8 pixels
      for x:=0 to 7 do begin
        if (w shr (4*x) and $f)>h then h:=w shr (4*x) and $f;
        if (w and $f)>h then h:=w and $f;
      end;
    end;
    NumColours:=h+1;
    lblNumColours.Caption:=Format('BMP uses %d colours',[NumColours]);
    case h of
      0..1: rb1bit.Checked:=True;
      2..3: rb2bit.Checked:=True;
      4..7: rb3bit.Checked:=True;
      8..15:rb4bit.Checked:=True;
    end;
  end;

  btnLoadPalette.Click;
  WriteReconstData;
end;

function Swap32(value:DWORD):DWORD; assembler;
asm
  BSWAP eax
end;

procedure TForm1.WriteReconstData;
var
  s,datatype:string;
  i,offset,digits,blank,val:integer;
  wa:PWordArray;
  Modifiers:word;
begin
  offset:=StrToIntDef(Form1.edTileOffset.Text,-1);
  blank :=StrToIntDef(Form1.edBlankTile.Text ,-1);

  // Make checkboxes valid
  if cbBytes.Checked
  and (
        (HighestTileIndex+Offset>$ff)
     or (cbUseMirroring.Checked)
     or (cbSpritePalette.Checked)
     or (cbInFront.Checked)
  ) then begin
    cbBytes.Checked:=False;
    edTileOffsetChange(nil);
    exit;
  end;

  Modifiers:=0;
  if not Form1.cbBytes.Checked then begin
    if Form1.cbSpritePalette.Checked then Modifiers:=Modifiers or (1 shl 11);
    if Form1.cbInFront      .Checked then Modifiers:=Modifiers or (1 shl 12);
  end;

  wa:=@ReconstData[1];
  if (offset=-1) or (blank=-1) then begin
    s:='Invalid tile offset or blank tile index';
  end else begin
    if form1.cbBytes.Checked
    then begin
      datatype:='.db ';
      digits:=2;
    end else begin
      datatype:='.dw ';
      digits:=4;
    end;
    for i:=0 to (Form1.imgOriginal.Picture.Bitmap.Height div 8)*32-1 do begin
      if (i mod 32)=0 then begin // new line
        Delete(s,Length(s),1);
        s:=s+#13#10+datatype;
      end;
      if wa[i]=$ffff then val:=blank else val:=wa[i]+offset;
      val:=val and ((1 shl (digits*4))-1) or Modifiers;
      if (wa[i]<>$ffff) or Form1.cbPad.Checked
      then s:=s+'$'+IntToHex(val,digits)+',';
    end;
    Delete(s,Length(s),1);
    Delete(s,1,2);
  end;
  mmReconstData.Text:=s;
end;

procedure TForm1.btnProcessClick(Sender: TObject);
var
  row:integer;
  p:pByteArray;
  Tile,ProcessedTile:array[0..7] of dword;
  b:byte;
  i,j,k,l:integer;
  pixelvalue:byte;
  sl:TStringList;
  wa:pWordArray;
  currentline:string;
begin
  sl:=TStringList.Create;
  for row:=0 to (imgColumn.Picture.Bitmap.Height div 8)-1 do begin
    sl.Add('; Tile number 0x'+IntToHex(row,3));

    currentline:='.db ';
    if gb1bit.Enabled then begin
      for i:=0 to 7 do begin
        p:=imgColumn.Picture.Bitmap.ScanLine[row*8+i];
        b:=p^[0];
        if cbInvert.Checked then b:=b xor $ff;
        currentline:=currentline+'$'+IntToHex(b,2)+',';
      end;
    end else begin
      for i:=0 to 7 do begin
        p:=imgColumn.Picture.Bitmap.ScanLine[row*8+i];
        move(p^[0],Tile[i],4);
        Tile[i]:=swap32(Tile[i]); // Tile[i] now contains $01234567 where each digit is each pixel's index
        ProcessedTile[i]:=0;
      end;
      for i:=0 to 7 do // for each line in the tile
        for j:=0 to 7 do begin // for each pixel on the line
          pixelvalue:=Tile[i] shr (4*(7-j)) and $f;
          for k:=0 to 3 do // for each bit in the pixel
            ProcessedTile[i]:=ProcessedTile[i] or (((pixelvalue shr k) and 1) shl (8*k+(7-j)));
        end;

      for i:=0 to 7 do begin
        currentline:=currentline+'$'+IntToHex((ProcessedTile[i] shr  0) and $ff,2)+',';
        if rb1bit.checked then continue;
        currentline:=currentline+'$'+IntToHex((ProcessedTile[i] shr  8) and $ff,2)+',';
        if rb2bit.checked then continue;
        currentline:=currentline+'$'+IntToHex((ProcessedTile[i] shr 16) and $ff,2)+',';
        if rb3bit.checked then continue;
        currentline:=currentline+'$'+IntToHex((ProcessedTile[i] shr 24) and $ff,2)+',';
        if rb4bit.checked then continue;
      end;
    end;
    Delete(currentline,Length(currentline),1);
    sl.add(currentline);
  end;
  mmResults.Text:=sl.text;

  // Build reconstruction data
  j:=imgOriginal.Picture.Bitmap.Height div 8;             // Number of rows
  k:=imgOriginal.Picture.Bitmap.Width div 8;              // Tiles per row (no. of columns)
  SetLength(ReconstData,j*32*2);  // make string big enough to hold it - 32 tiles per line, 2 bytes per tile
  wa:=@ReconstData[1]; // treat string as array of words
  for i:=0 to j-1 do begin
    for l:=0 to k-1 do wa[i*32+l]:=i*k+l;
    for l:=k to 31  do wa[i*32+l]:=$ffff;
  end;

  sl.free;

  SetHint(Format('%d tiles converted',[imgColumn.Picture.Bitmap.Height div 8]));

  HighestTileIndex:=(imgColumn.Picture.Bitmap.Height div 8)-1;

  if cbRemoveDupes.Checked then btnRemoveDupes.Click;
  WriteReconstData;
end;

procedure TForm1.btnSaveClick(Sender: TObject);
begin
  SaveDialog1.FileName:=ChangeFileExt(FileName.Text,'.inc');
  if SaveDialog1.Execute then mmResults.Lines.SaveToFile(SaveDialog1.FileName);
end;

procedure TForm1.rb2bitClick(Sender: TObject);
begin
  btnProcess.Click;
end;

procedure TForm1.FormCreate(Sender: TObject);
begin
  DragAcceptFiles(Handle,True); // allow dropping of files
  Application.OnHint:=DisplayHint;
  Application.Title:=Form1.Caption;
end;

procedure TForm1.WMDROPFILES(var Message: TWMDROPFILES);
var
  NTstring: array[0..255] of char;
  FileDropped:string;
begin
  DragQueryFile(Message.drop,0,NTstring,255); // Get 1st dropped file
  FileDropped:=StrPas(NTString);              // Convert to a Delphi string
  dragfinish(message.drop);                   // Discard dropped file(s) data
  FileName.Text:=FileDropped;
  btnLoad.Click;
end;

function MirrorTileData(original:string;DoHoriz:boolean):string;
var
  i,j,byte,n:integer;
  temp:string;
begin
  // original = '.db $00,$10,$10,$10,$10,$10,$00,$00'
  // hex bytes could be more
  result:='';
  if DoHoriz then begin
    // need to mirror individual bytes in order
    result:='.db ';
    i:=5;
    while i<length(original) do begin
      byte:=strtoint(copy(original,i,3));
      // mirror byte
      byte:=((byte and $80) shr 7) or
            ((byte and $40) shr 5) or
            ((byte and $20) shr 3) or
            ((byte and $10) shr 1) or
            ((byte and $08) shl 1) or
            ((byte and $04) shl 3) or
            ((byte and $02) shl 5) or
            ((byte and $01) shl 7);
      result:=result+'$'+inttohex(byte,2)+',';
      Inc(i,4);
    end;
    Delete(result,Length(result),1);
  end else begin
    // Vertical mirror: need to switch bytes around in groups of 8
    result:='.db ';
    i:=5; // index of 1st byte in string

    if      form1.rb1bit.checked then n:=3 // how many bytes to switch in each group
    else if form1.rb2bit.checked then n:=7
    else if form1.rb3bit.checked then n:=11
    else                              n:=15;

    while i<length(original) do begin
      temp:='';
      for j:=1 to 8 do begin
        temp:=copy(original,i,n)+','+temp;
        Inc(i,n+1);
      end;
      result:=result+temp;
    end;
    Delete(result,Length(result),1);
  end;
end;

procedure RemoveDupes(sl:TStringList;s:string;wa:pWordArray;OriginalTileNumber:integer);
var
  j,k,DuplicateTileNumber:integer;
begin
  // find duplicates
  j:=sl.IndexOf(s);
  while j>-1 do begin
    // Duplicate found at line j
    DuplicateTileNumber:=StrToInt('$'+Copy(sl[j-1],17,3)); // Get duplicate tile number
    k:=0;
    while wa[k]<>DuplicateTileNumber do Inc(k); // Find where in the reconst data it is used
    wa[k]:=OriginalTileNumber;
    sl.Delete(j);    // Delete line
    sl.Delete(j-1);  // Delete comment before it
    j:=sl.IndexOf(s);
  end;
  // try mirroring...
  if (form1.cbUseMirroring.Checked) and (OriginalTileNumber<512) then begin
    RemoveDupes(sl,MirrorTileData(s,true),wa,OriginalTileNumber or $4000);
    RemoveDupes(sl,MirrorTileData(s,false),wa,OriginalTileNumber or $8000);
    RemoveDupes(sl,MirrorTileData(MirrorTileData(s,true),false),wa,OriginalTileNumber or $c000);
  end;
end;

procedure TForm1.btnRemoveDupesClick(Sender: TObject);
var
  i,j,k,OriginalTileNumber:integer;
  sl:TStringList;
  s:string;
  wa:pWordArray;
begin
  SetHint('Removing duplicate tiles...');
  sl:=TStringList.Create;
  sl.Text:=mmResults.Text;
  wa:=@ReconstData[1];

  i:=0;
  while i<sl.Count-1 do begin
    s:=sl[i];
    if s[1]<>'.' then begin // Not a data line
       Inc(i);
       Continue;
    end;

    OriginalTileNumber:=StrToInt('$'+Copy(sl[i-1],17,3)); // Get original tile number
    sl[i]:='';
    // find duplicates
    RemoveDupes(sl,s,wa,OriginalTileNumber);
    sl[i]:=s;
    Inc(i);
  end;
  // Make numbers consecutive again
  j:=0;
  for i:=0 to sl.Count-1 do
    if pos('; Tile number 0x',sl[i])=1 then begin
      OriginalTileNumber:=StrToInt('$'+Copy(sl[i],17,3));
      sl[i]:='; Tile number 0x'+IntToHex(j,3);
      // I want to change all occurrences of OriginalTileData to j
      for k:=0 to (Length(ReconstData) div 2)-1 do
        if  (wa[k]<>$ffff)
        and ((wa[k] and $3fff)=(OriginalTileNumber and $3fff))
        then wa[k]:=j or (wa[k] and not $3fff);
      Inc(j);
    end;
  HighestTileIndex:=j-1;

  // process my "moved" mirror bits
  for i:=0 to (Length(ReconstData) div 2)-1 do
    if  (wa[i]<>$ffff)
    and ((wa[i] and $c000)>0)
    then wa[i]:=(wa[i] and $3fff)
                or ((wa[i] and $c000) shr 5);
  // vh0pcvhdddddddd
  // >>>>>

  mmResults.Text:=sl.Text;
  sl.Free;

  WriteReconstData;
  SetHint(
    IntToStr(imgColumn.Picture.Bitmap.Height div 8)+
    ' tiles converted ('+
    IntToStr(HighestTileIndex+1)
    +' unique)'
  );
end;

procedure TForm1.btnSaveReconstClick(Sender: TObject);
begin
  SaveDialog1.FileName:=ChangeFileExt(FileName.Text,' (tile numbers).inc');
  if SaveDialog1.Execute then mmReconstData.Lines.SaveToFile(SaveDialog1.FileName);
end;

procedure TForm1.edTileOffsetChange(Sender: TObject);
begin
  cbSpritePalette.Enabled:=not cbBytes.Checked;
  cbInFront      .Enabled:=not cbBytes.Checked;
  WriteReconstData;
end;

procedure TForm1.DisplayHint(Sender: TObject);
begin
  if Application.Hint<>''
  then StatusBar1.SimpleText:=Application.Hint
  else StatusBar1.SimpleText:=OldHint;
end;

procedure TForm1.btnLoadPaletteClick(Sender: TObject);
var
  hpal:HPALETTE;
  palette:array[0..15*4] of byte;
  s:string;
  i,j:integer;
  colours:array[0..2] of integer;
  bm:TBitmap;
  p:pbytearray;
begin
  if imgOriginal.Picture.Bitmap.Empty then exit;
  // get palette from bitmap
  hpal:=imgOriginal.Picture.Bitmap.Palette;
  // get colours
  GetPaletteEntries(hpal,0,NumColours,palette);
  // draw
  bm:=TBitmap.Create;
  with bm do begin
    PixelFormat:=pf4Bit;
    Palette:=CopyPalette(imgOriginal.Picture.Bitmap.Palette);
    Width:=NumColours;
    Height:=1;
    p:=ScanLine[0];
    for i:=0 to NumColours-1 do
      if ((i and 1) = 1)
      then p[i div 2]:=p[i div 2] and $f0 or i
      else p[i div 2]:=p[i div 2] and $0f or (i shl 4);
    imgPalette.Picture.Assign(bm);
    Free;
  end;

  // process
  s:='.db';
  for i:=0 to NumColours-1 do begin
    // get rgb
    for j:=0 to 2 do colours[j]:=palette[i*4+j];

    if rbPalHex.Checked or rbPalConst.Checked then begin
      // figure out values (SMS)
      // eSMS = 0, 57, 123, 189
      // Meka = 0, 85, 170, 255
      //     or 0, 65, 130, 195
      for j:=0 to 2 do
      if      colours[j]<56  then colours[j]:=0
      else if colours[j]<122 then colours[j]:=1
      else if colours[j]<188 then colours[j]:=2
      else                        colours[j]:=3;

      if rbPalHex.Checked
      then s:=s+' $'+inttohex(colours[0] shl 0 or
                              colours[1] shl 2 or
                              colours[2] shl 4
                              ,2)
      else if rbPalConst.Checked
      then s:=s+' cl'+inttostr(colours[0])
                     +inttostr(colours[1])
                     +inttostr(colours[2]);
    end else begin
      s[3]:='w';
      // GG palette - reduce each to 4 bits
      for j:=0 to 2 do colours[j]:=colours[j] shr 4;

      // output
      if rbPalGG.Checked
      then s:=s+' $'+IntToHex(colours[0]
                          or (colours[1] shl 4)
                          or (colours[2] shl 8),4);
    end;
  end;
  mmPalette.Text:=s;
end;

procedure TForm1.btnSavePaletteClick(Sender: TObject);
begin
  SaveDialog1.FileName:=ChangeFileExt(FileName.Text,' (palette).inc');
  if SaveDialog1.Execute then mmPalette.Lines.SaveToFile(SaveDialog1.FileName);
end;

end.
