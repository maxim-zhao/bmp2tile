unit Unit1;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, ExtCtrls, ComCtrls, Math, ShellAPI;

type
  TForm1 = class(TForm)
    imgColumn: TImage;
    GroupBox1: TGroupBox;
    FileName: TEdit;
    btnLoad: TButton;
    Label1: TLabel;
    GroupBox2: TGroupBox;
    gb4bit: TGroupBox;
    rb2bit: TRadioButton;
    rb3bit: TRadioButton;
    rb4bit: TRadioButton;
    btnProcess: TButton;
    GroupBox4: TGroupBox;
    mmResults: TRichEdit;
    btnSave: TButton;
    imgOriginal: TImage;
    gb1bit: TGroupBox;
    cbInvert: TCheckBox;
    Label2: TLabel;
    rb1bit: TRadioButton;
    btnRemoveDupes: TButton;
    GroupBox3: TGroupBox;
    mmReconstData: TRichEdit;
    btnSaveReconst: TButton;
    Label3: TLabel;
    edTileOffset: TEdit;
    edBlankTile: TEdit;
    cbBytes: TCheckBox;
    cbPad: TCheckBox;
    cbSpritePalette: TCheckBox;
    cbInFront: TCheckBox;
    cbRemoveDupes: TCheckBox;
    procedure btnLoadClick(Sender: TObject);
    procedure btnProcessClick(Sender: TObject);
    procedure btnSaveClick(Sender: TObject);
    procedure rb2bitClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
//    procedure btnBitsToBytesClick(Sender: TObject);
    procedure btnRemoveDupesClick(Sender: TObject);
    procedure btnSaveReconstClick(Sender: TObject);
    procedure edTileOffsetChange(Sender: TObject);
  private
    { Private declarations }
    procedure WMDROPFILES(var Message: TWMDROPFILES); message WM_DROPFILES;
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
    Application.MessageBox('File not found','',MB_OK+MB_ICONERROR);
    exit;
  end;

  // Load bitmap
  bm:=TBitmap.Create;
  try
    bm.LoadFromFile(FileName.Text);
    // Check bitmap pixel format
    case bm.PixelFormat of
    pf1bit:begin EnableGroupBox(gb1bit,true ); EnableGroupBox(gb4bit,false); end;
    pf4bit:begin EnableGroupBox(gb1bit,false); EnableGroupBox(gb4bit,true);  end;
    else
      EnableGroupBox(gb1bit,false);
      EnableGroupBox(gb4bit,false);
      Application.MessageBox('Bitmap is not 1 bit or 4 bit! Can''t process it.','',MB_ICONERROR);
      mmResults.Text:='';
      btnSave.Enabled:=False;
      imgColumn.Picture.Bitmap:=nil;
      imgOriginal.Picture.Bitmap:=nil;
      Exit;
    end;

    // Check bitmap dimensions
    if ((bm.width  mod 8)>0)
    or ((bm.height mod 8)>0)
    then begin
      Application.MessageBox('Bitmap''s dimensions are not multiples of 8! Adjusting...','',MB_ICONWARNING);
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
    case h of
      0..1: rb1bit.Checked:=True;
      2..3: rb2bit.Checked:=True;
      4..7: rb3bit.Checked:=True;
      8..15:rb4bit.Checked:=True;
    end;
  end;
end;

function Swap32(value:DWORD):DWORD; assembler;
asm
  BSWAP eax
end;

procedure WriteReconstData;
var
  s,datatype:string;
  i,offset,digits,blank,val:integer;
  wa:PWordArray;
  Modifiers:word;
begin
  offset:=StrToIntDef(Form1.edTileOffset.Text,-1);
  blank :=StrToIntDef(Form1.edBlankTile.Text ,-1);

  // Make checkboxes valid
  if Form1.cbBytes.Checked and (HighestTileIndex+Offset>$ff)
  then begin
    Form1.cbBytes.Checked:=False;
    Form1.edTileOffsetChange(nil);
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
  form1.mmReconstData.Text:=s;
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
  btnSave.Enabled:=True;
  btnSave.SetFocus;

  if cbRemoveDupes.Checked then btnRemoveDupes.Click;
end;

procedure TForm1.btnSaveClick(Sender: TObject);
var
  fn:string;
begin
  fn:=ChangeFileExt(FileName.Text,'.inc');
  if (not FileExists(fn)) or (Application.MessageBox('File already exists! Overwrite?','',MB_YESNO+MB_ICONQUESTION)=idYes)
  then mmResults.Lines.SaveToFile(fn);
end;

procedure TForm1.rb2bitClick(Sender: TObject);
begin
  btnProcess.Click;
end;

procedure TForm1.FormCreate(Sender: TObject);
begin
  DragAcceptFiles(Handle,True); // allow dropping of files
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

procedure TForm1.btnRemoveDupesClick(Sender: TObject);
var
  i,j,k,OriginalTileNumber,DuplicateTileNumber:integer;
  sl:TStringList;
  s:string;
  wa:pWordArray;
begin
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
    j:=sl.IndexOf(s);
    while j>-1 do begin
      // Duplicate found at line j
      DuplicateTileNumber:=StrToInt('$'+Copy(sl[j-1],17,3)); // Get duplicate tile number
      k:=0; while wa[k]<>DuplicateTileNumber do Inc(k); // Find where in the reconst data it is used
      wa[k]:=OriginalTileNumber;
      sl.Delete(j);    // Delete line
      sl.Delete(j-1);  // Delete comment before it
      j:=sl.IndexOf(s);
    end;
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
        and (wa[k]=OriginalTileNumber)
        then wa[k]:=j;
      Inc(j);
    end;

  HighestTileIndex:=j-1;

  mmResults.Text:=sl.Text;
  sl.Free;

  WriteReconstData;
end;

procedure TForm1.btnSaveReconstClick(Sender: TObject);
var
  fn:string;
begin
  fn:=ChangeFileExt(FileName.Text,' (tile numbers).inc');
  if (not FileExists(fn)) or (Application.MessageBox('File already exists! Overwrite?','',MB_YESNO+MB_ICONQUESTION)=idYes)
  then mmReconstData.Lines.SaveToFile(fn);
end;

procedure TForm1.edTileOffsetChange(Sender: TObject);
begin
  cbSpritePalette.Enabled:=not cbBytes.Checked;
  cbInFront      .Enabled:=not cbBytes.Checked;
  WriteReconstData;
end;

end.
