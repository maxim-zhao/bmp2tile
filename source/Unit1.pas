unit Unit1;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, ExtCtrls, ComCtrls, Math, ShellAPI, XPMan, CommDlg, GraphicEx;

type
  TForm1 = class(TForm)
    GroupBox1: TGroupBox;
    FileName: TEdit;
    btnLoad: TButton;
    PageControl1: TPageControl;
    TabSheet1: TTabSheet;
    btnProcess: TButton;
    XPManifest1: TXPManifest;
    TabSheet3: TTabSheet;
    mmResults: TMemo;
    TabSheet2: TTabSheet;
    mmReconstData: TMemo;
    StatusBar1: TStatusBar;
    TabSheet4: TTabSheet;
    mmPalette: TMemo;
    btnLoadPalette: TButton;
    SaveDialog1: TSaveDialog;
    OpenDialog1: TOpenDialog;
    imgOriginal: TImage;
    imgColumn: TImage;
    TabSheet5: TTabSheet;
    mmMessages: TMemo;
    Panel1: TPanel;
    cbRemoveDupes: TCheckBox;
    cbUseMirroring: TCheckBox;
    cbPlanar: TCheckBox;
    cb8x16: TCheckBox;
    Label3: TLabel;
    edTileOffset: TEdit;
    btnSaveTilesRaw: TButton;
    Panel2: TPanel;
    cbSpritePalette: TCheckBox;
    cbInFront: TCheckBox;
    btnSaveReconst: TButton;
    Panel3: TPanel;
    rbPalHex: TRadioButton;
    rbPalConst: TRadioButton;
    rbPalGG: TRadioButton;
    btnSavePalette: TButton;
    Panel4: TPanel;
    imgPalette: TImage;
    Panel5: TPanel;
    procedure btnLoadClick(Sender: TObject);
    procedure btnProcessClick(Sender: TObject);
    procedure btnSaveTilesRawClick(Sender: TObject);
    procedure rb2bitClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure btnSaveReconstClick(Sender: TObject);
    procedure edTileOffsetChange(Sender: TObject);
    procedure btnLoadPaletteClick(Sender: TObject);
    procedure btnSavePaletteClick(Sender: TObject);
    procedure SaveDialog1TypeChange(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure cb8x16Click(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure PopulateTileDataMemo;
    procedure PopulateTilemapDataMemo;
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

uses StrUtils;

{$R *.DFM}

type
  TGetName = function:PChar; cdecl;
  TGetExt = function:PChar; cdecl;
  TCompressTiles = function(source: PChar; numTiles: integer; dest:PChar; destLen:integer):integer; cdecl;
  TCompressTilemap = function(source: PChar; width, height: integer; dest:PChar; destLen:integer):integer; cdecl;

  TPlugin = record
    Name: string;
    Ext: string;
    CompressTiles: TCompressTiles;
    CompressTilemap: TCompressTilemap;
    Handle: THandle;
  end;

var
  plugins:array of TPlugin;
  OldHint:string;
  NumColours:integer;
  tilehexcodes:TStringList;
  tiledata:PByteArray; // pointer to allocated memory
  tiledatasize:Integer;
  tilemap:PWordArray;
  tilemapheight:integer;
  tilemapwidth:integer;

procedure SetHint(s:string);
begin
  OldHint:=s;
  Form1.StatusBar1.SimpleText:=s;
  Form1.mmMessages.Lines.Add(s);
end;

procedure CompressTiles(data:PByteArray; numTiles:integer; compressor:TCompressTiles; filename:string);
var
  buf:PByteArray;
  bufsize,result:integer;
  size:integer;
begin
  // Try to compress to memory
  bufsize := 16384; // hope it fits...

  GetMem(buf, bufsize);
  while (bufsize < 1024*1024) do begin
    result:=compressor(PChar(@data^[0]), numTiles, PChar(@buf^[0]), bufsize);
    if (result < 0) then break; // cannot compress
    if (result > 0) then begin
      // save to file
      with TFileStream.Create(filename, fmCreate) do begin
        WriteBuffer(buf^, result);
        Free;
      end;
      // report
      size := numTiles * 32;
      SetHint(Format('Saved %d tiles to "%s" (%.2f%% compression)',[numTiles, ExtractFileName(filename), (size-result)*100.0/size]));
      break;
    end;
    // else try again with more memory
    bufsize:=bufsize*2;
    ReallocMem(buf, bufsize);
  end;
  FreeMem(buf);
end;

procedure CompressTilemap(data:PByteArray; width, height:integer; compressor:TCompressTilemap; filename:string);
var
  buf:PByteArray;
  bufsize,result:integer;
  size:integer;
begin
  // Try to compress to memory
  bufsize := 16384; // hope it fits...

  GetMem(buf, bufsize);
  while (bufsize < 1024*1024) do begin
    result:=compressor(PChar(@data^[0]), width, height, PChar(@buf^[0]), bufsize);
    if (result < 0) then break; // cannot compress
    if (result > 0) then begin
      // save to file
      with TFileStream.Create(filename, fmCreate) do begin
        WriteBuffer(buf^, result);
        Free;
      end;
      // report
      size := width * height * SizeOf(Word);
      SetHint(Format('Saved %dx%d tilemap to "%s" (%.2f%% compression)',[width, height, ExtractFilename(filename), (size-result)*100.0/size]));
      break;
    end;
    // else try again with more memory
    bufsize:=bufsize*2;
    ReallocMem(buf, bufsize);
  end;
  FreeMem(buf);
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
  if (Sender=btnLoad) then begin
    if OpenDialog1.Execute
    then FileName.Text:=OpenDialog1.FileName
    else exit;
  end;

  // Exit if file does not exist
  if not fileexists(FileName.Text) then begin
    Application.MessageBox('File not found',nil,MB_OK+MB_ICONERROR);
    exit;
  end;

  SetHint('Loading "' + FileName.Text + '"');

  // Load bitmap
  bm:=TBitmap.Create;
  try
    imgOriginal.Picture.LoadFromFile(FileName.Text);
    bm.Assign(imgOriginal.Picture.Bitmap);

    // Check bitmap pixel format
    if (bm.PixelFormat<>pf1bit)
    and (bm.PixelFormat<>pf8bit)
    and (bm.PixelFormat<>pf4bit) then begin
      Application.MessageBox('Image format not supported! Can''t process it.',nil,MB_ICONERROR);
      Exit;
    end;

    // Check bitmap dimensions
    if ((bm.width  mod 8)<>0)
    or ((bm.height mod 8)<>0)
    then begin
      Application.MessageBox('Bitmap''s dimensions are not multiples of 8!',nil,MB_ICONERROR);
      Exit;
    end;
    if (cb8x16.Checked and ((bm.Height mod 16)<>0))
    then begin
      Application.MessageBox('Bitmap''s height is not a multiple of 16!',nil,MB_ICONERROR);
      Exit;
    end;

    w:=bm.width  div 8;
    h:=bm.height div 8;

    SetHint('Decomposing to tiles...');
    with imgColumn.Picture.Bitmap do begin
      Assign(bm);
      Width:=8;
      Height:=w*h*8;
      if cb8x16.Checked then begin
        h:=h div 2;
        for y:=0 to h-1 do begin
          for x:=0 to w-1 do begin
            Canvas.CopyRect(
              Rect(0,(y*w+x)*16,8,(y*w+x)*16+16), // dest
              bm.Canvas,
              Rect(x*8,y*16,x*8+8,y*16+16) // src
            );
          end;
        end;
      end else begin
        for y:=0 to h-1 do begin
          for x:=0 to w-1 do begin
            Canvas.CopyRect(
              Rect(0,(y*w+x)*8,8,(y*w+x)*8+8),
              bm.Canvas,
              Rect(x*8,y*8,x*8+8,y*8+8)
            );
          end;
        end;
      end;
    end;
//    imgOriginal.picture.assign(bm);
  finally
    bm.free;
  end;

  btnProcess.Enabled:=false;
  // Check how many colours are used if 4bpp or 8bpp
  if bm.PixelFormat<>pf1bit then begin
    SetHint('Counting colours...');
    h:=0; // h = highest
    for y:=0 to imgColumn.Picture.Bitmap.height-1 do begin
      p:=imgColumn.Picture.Bitmap.ScanLine[y];
      if imgColumn.Picture.Bitmap.PixelFormat=pf4bit then
        for x:=0 to 3 do begin
          if (p[x] and $f)>h then h:=p[x] and $f;
          if (p[x] shr 4 and $f)>h then h:=p[x] shr 4 and $f;
        end
      else // 8 bit
        for x:=0 to 7 do if p[x]>h then h:=p[x]
    end;
    NumColours:=h+1;
    SetHint(Format('Image uses %d colours',[NumColours]));
  end else begin
    // 1 bit
    NumColours:=2;
  end;

  Application.ProcessMessages;
  btnProcess.Enabled:=True;

  if NumColours>16 then begin
    SetHint('Error converting bitmap');
    Application.MessageBox('Too many colours! This bitmap can''t be processed.',nil,MB_ICONERROR+MB_OK);
    exit;
  end;

  SetHint('Reading palette...');
  btnLoadPalette.Click;

  btnProcess.Click;
end;

function Swap32(value:DWORD):DWORD; assembler;
asm
  BSWAP eax
end;

function hflip(str:string):string;
const
  lookup:array[0..15] of char = ('0', '8', '4', 'c', '2', 'a', '6', 'e', '1',
    '9', '5', 'd', '3', 'b', '7', 'f');
var
  i:integer;
begin
  // I want to reverse the bits in each byte
  // this is horrid!
  result := '';
  SetLength(result, 64);
  for i:=0 to 31 do begin
    result[i*2+1] := lookup[StrToInt('$'+str[i*2+2])];
    result[i*2+2] := lookup[StrToInt('$'+str[i*2+1])];
  end;
end;

function vflip(str:string):string;
var
  i:integer;
begin
  // A little nicer: reverse the order of 8-char substrings
  result := '';
  for i:=1 to 8 do begin
    result := result + Copy(str, 65 - i*8, 8);
  end;
end;

procedure TForm1.btnProcessClick(Sender: TObject);
var
  row:integer;
  p:pByteArray;
  wp:pWord;
  Tile:array[0..7] of dword;
  TileRow:dword;
  i,j,k:integer;
  pixelvalue:byte;
  hexstring, althexstring:string;
begin
  if not btnProcess.Enabled then exit;
  if imgOriginal.Picture.Bitmap.Empty then exit;
  SetHint('Converting tiles...');
  cbUseMirroring.Enabled:=cbRemoveDupes.Checked;

  tilehexcodes.Clear;
  tilehexcodes.Sorted := cbRemoveDupes.Checked;

  // Allocate the tilemap 2D array
  tilemapwidth := imgOriginal.Picture.Bitmap.Width div 8;
  tilemapheight := imgOriginal.Picture.Bitmap.Height div 8;
  GetMem(tilemap, tilemapheight * tilemapwidth * SizeOf(word));
  wp := @tilemap^[0];

  // For each tile
  for row:=0 to (imgColumn.Picture.Bitmap.Height div 8)-1 do begin
    // Get bitmap into Tile in a standard format
    case (imgColumn.Picture.Bitmap.PixelFormat) of
    pf1bit: begin
      // 1bpp version
      for i:=0 to 7 do begin
        p:=imgColumn.Picture.Bitmap.ScanLine[row*8+i];
        Tile[i]:=(((p[0] shr 7) and $1) shl 28) or
                 (((p[0] shr 6) and $1) shl 24) or
                 (((p[0] shr 5) and $1) shl 20) or
                 (((p[0] shr 4) and $1) shl 16) or
                 (((p[0] shr 3) and $1) shl 12) or
                 (((p[0] shr 2) and $1) shl  8) or
                 (((p[0] shr 1) and $1) shl  4) or
                 (((p[0] shr 0) and $1) shl  0);
      end
    end;
    pf4bit: begin
      // 4bpp version
      for i:=0 to 7 do begin
        p:=imgColumn.Picture.Bitmap.ScanLine[row*8+i];
        move(p^[0],Tile[i],4);
        Tile[i]:=swap32(Tile[i]); // Tile[i] now contains $01234567 where each digit is each pixel's index
      end
    end;
    pf8bit: begin
      // 8bpp version
      for i:=0 to 7 do begin
        p:=imgColumn.Picture.Bitmap.ScanLine[row*8+i];
        Tile[i]:=((p[0] and $f) shl 28) or
                 ((p[1] and $f) shl 24) or
                 ((p[2] and $f) shl 20) or
                 ((p[3] and $f) shl 16) or
                 ((p[4] and $f) shl 12) or
                 ((p[5] and $f) shl  8) or
                 ((p[6] and $f) shl  4) or
                 ((p[7] and $f) shl  0);
        // Tile[i] now contains $01234567 where each digit is each pixel's index
      end;
    end;
    end;

    // Now Tile contains the 4bpp paletted data in "chunky" format -
    // each 4 bits defines a single pixel

    // Convert to planar if needed
    if (cbPlanar.Checked) then begin
      // planar
      // 1st byte = LSBs of each pixel
      // ..
      // 4th byte = MSBs of each pixel
      for i:=0 to 7 do begin
        TileRow := 0;
        for j:=0 to 7 do begin
          pixelvalue := (Tile[i] shr (4*(7-j))) and $f;
          // for each bit in pixelvalue
          for k:=0 to 3 do begin
            // OR it into TileRow in the right place
            TileRow := TileRow
              or (((pixelvalue shr k) and 1) shl (8*k+(7-j)));
          end;
        end;
        // put it back in Tile
        // the ordering is wrong though
        // TODO: make it be right in the first place?
        Tile[i] := swap32(TileRow);
      end;
    end;

    // Make a string of text out of it (hex string)
    hexstring := '';
    for i:= 0 to 7 do begin
      hexstring := hexstring + IntToHex(Tile[i], 8);
    end;

    if (cbRemoveDupes.Checked)
    then begin
      // check for an exact match
      if (tilehexcodes.Find(hexstring, i)) then begin
        i := Integer(tilehexcodes.Objects[i]);
      end else begin
        if (cbUseMirroring.Checked) then begin
          // check for flipped/mirrored matches
          althexstring := hflip(hexstring); // Hflip
          if (tilehexcodes.Find(althexstring, i)) then begin
            // H-flip match
            i := Integer(tilehexcodes.Objects[i]) or $0200;
          end else begin
            althexstring := vflip(hexstring); // VFlip
            if (tilehexcodes.Find(althexstring, i)) then begin
              i := Integer(tilehexcodes.Objects[i]) or $0400;
            end else begin
              althexstring := hflip(althexstring); // VFlip + HFlip
              if (tilehexcodes.Find(althexstring, i)) then begin
                i := Integer(tilehexcodes.Objects[i]) or $0600;
              end else begin
                i := -1;
              end;
            end;
          end;
        end else begin
          // no mirroring
          i := -1;
        end;
      end;
      // so add the original version and its tile number
      if (i = -1) then begin
        i := tilehexcodes.Count;
        tilehexcodes.AddObject(hexstring, TObject(i));
      end;
    end else begin
      i := tilehexcodes.Count;
      tilehexcodes.AddObject(hexstring, TObject(i));
    end;
    // i is now the index of the tile in the list

    // append it to the tilemap data
    wp^ := i;
    Inc(wp);
  end;

  // Convert silly hex codes into pure RAM
  if Assigned(tiledata) then begin
    FreeMem(tiledata);
  end;
  tiledatasize := tilehexcodes.Count * 32;
  GetMem(tiledata, tiledatasize);
  for i:=0 to tilehexcodes.Count - 1 do begin
    // Get the memory location we want
    p := PByteArray(Integer(tiledata) + Integer(tileHexCodes.Objects[i]) * 32);
    // Convert the hex to there
    for j:=0 to 31 do begin
      p^[j] := StrToInt('$' + Copy(tilehexcodes[i], j*2+1, 2));
    end;
  end;

  SetHint(Format('%d tiles converted',[tilehexcodes.Count]));

  // Pretty-print the data to the memos
  PopulateTileDataMemo;
  PopulateTilemapDataMemo;
end;

procedure SaveText(lines:TStrings;filename:string);
begin
  lines.SaveToFile(filename);
  SetHint('Saved in text format to "'+ExtractFileName(filename)+'"');
end;

procedure SaveBinary(lines:TStrings;filename:string);
var
  i,j,n:integer;
  s:string;
  values:TStringList;
  fs:TFileStream;
  size:integer;
begin
  values:=TStringList.Create;
  fs:=TFileStream.Create(filename,fmCreate);

  size:=0;

  for i:=0 to lines.Count-1 do begin
    s:=lines[i];
    values.Clear;
    ExtractStrings( [',',' '], [], PChar(s),values);
    for j:=0 to values.Count-1 do begin
      if values[j]='.dw' then size:=2;
      if values[j]='.db' then size:=1;
      if pos(';',values[j])>0 then break; // stop parsing line when ; encountered
      n:=StrToIntDef(values[j],-1);
      if n>-1 then fs.Write(n,size);
    end;
  end;
  fs.free;
  values.free;
  SetHint('Saved in binary format to "'+ExtractFileName(filename)+'"');
end;

procedure SaveTiles(filename:string;format:integer);
var
  i,n:integer;
begin
  if format = 1 then SaveText(Form1.mmResults.Lines,filename)
  else begin
    // Find the nth savetiles-capable plugin
    n:=1;
    for i:=0 to Length(plugins)-1 do begin
      if Assigned(plugins[i].CompressTiles) then Inc(n);
      if n = format then begin
        // found it
        CompressTiles(tiledata, tiledatasize div 32, plugins[i].CompressTiles, filename);
        break;
      end;
    end;
  end;
end;

procedure TForm1.btnSaveTilesRawClick(Sender: TObject);
var
  filter:string;
  i:integer;
  mask: string;
  FilterParts: TStringList;
begin
  // Build the save dialog filter
  filter := 'Include files (*.inc)|*.inc';
  for i:=0 to Length(plugins) - 1 do begin
    if Assigned(plugins[i].CompressTiles)
    then filter := filter + Format('|%s (*.%s)|*.%s',[plugins[i].Name, plugins[i].Ext, plugins[i].Ext]);
  end;
  SaveDialog1.Filter := filter;

  if (sender = nil) then begin
    // automated save
    // find the index of the ext of the filename
    mask := '*' + ExtractFileExt(SaveDialog1.Filename);
    FilterParts:=TStringList.Create;
    ExtractStrings(['|'],[],PChar(filter),FilterParts);
    SaveTiles(SaveDialog1.FileName, (FilterParts.IndexOf(mask) + 1) div 2);
    FilterParts.Free;
  end else begin
    SaveDialog1.FileName := ChangeFileExt(FileName.Text,' (tiles).inc');
    SaveDialog1.OnTypeChange(SaveDialog1);
    if SaveDialog1.Execute then SaveTiles(SaveDialog1.FileName,SaveDialog1.FilterIndex);
  end;
end;

procedure TForm1.rb2bitClick(Sender: TObject);
begin
  btnProcess.Click;
end;

procedure TForm1.FormCreate(Sender: TObject);
var
  GetName: TGetName;
  GetExt: TGetExt;
  sr:TSearchRec;
  dllHandle: THandle;
  plugin:TPlugin;
begin
  DragAcceptFiles(Handle,True); // allow dropping of files
  Application.OnHint:=DisplayHint;
  Application.Title:=Form1.Caption;

  // build plugins list
  if (FindFirst(ExtractFilePath(ParamStr(0)) + 'gfxcomp_*.dll', faAnyFile, sr) = 0) then  begin
    repeat
      // load DLL
      dllHandle := LoadLibrary(PAnsiChar(sr.Name));
      if (dllHandle > HINSTANCE_ERROR) then begin
        @GetName := GetProcAddress(dllHandle, 'getName');
        @GetExt := GetProcAddress(dllHandle, 'getExt');
        @plugin.CompressTiles := GetProcAddress(dllHandle, 'compressTiles');
        @plugin.CompressTilemap := GetProcAddress(dllHandle, 'compressTilemap');
        if Assigned(GetName)
        and Assigned(GetExt)
        then begin
          SetLength(plugins,Length(plugins)+1);
          plugin.Name := GetName();
          plugin.Ext := GetExt();
          plugin.Handle := dllHandle;
          plugins[Length(plugins)-1] := plugin;
        end else begin
          FreeLibrary(dllHandle);
        end;
      end;
    until FindNext(sr) <> 0;
    FindClose(sr);
  end;

  // Initialise data containers
  tilehexcodes := TStringList.Create;
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
  btnLoadClick(nil);
end;

procedure SaveTilemap(filename:string;format:integer);
var
  i,n:integer;
  extraflags,bufsize:integer;  // SVERX's dirty fix
begin
  if format = 1 then SaveText(Form1.mmReconstData.Lines,filename)
  else begin
    // Find the nth savetiles-capable plugin
    n:=1;
    for i:=0 to Length(plugins)-1 do begin
      if Assigned(plugins[i].CompressTilemap) then Inc(n);
      if n = format then begin
        // found it
        
        // SVERX's dirty fix - BEGIN
        extraflags := 0;
        if (cbSpritePalette.Checked) then extraflags := extraflags or $0800;
        if (cbInFront.Checked) then extraflags := extraflags or $1000;
        bufsize := 0;
        while (bufsize < tilemapheight*tilemapwidth) do begin
          tilemap[bufsize] := (tilemap[bufsize] and $07FF) or extraflags;
          bufsize : = bufsize + 1;
        end;
        // SVERX's dirty fix - END

        CompressTilemap(PByteArray(tilemap), tilemapheight, tilemapwidth, plugins[i].CompressTilemap, filename);
        break;
      end;
    end;
  end;
end;

procedure TForm1.btnSaveReconstClick(Sender: TObject);
var
  filter:string;
  i:integer;
  mask: string;
  FilterParts: TStringList;
begin
  // Build the save dialog filter
  filter := 'Include files (*.inc)|*.inc';
  for i:=0 to Length(plugins) - 1 do begin
    if Assigned(plugins[i].CompressTileMap)
    then filter := filter + Format('|%s (*.%s)|*.%s',[plugins[i].Name, plugins[i].Ext, plugins[i].Ext]);
  end;
  SaveDialog1.Filter := filter;

  if (sender = nil) then begin
    // find the index of the ext of the filename
    mask := '*' + ExtractFileExt(SaveDialog1.Filename);
    FilterParts:=TStringList.Create;
    ExtractStrings(['|'],[],PChar(filter),FilterParts);
    SaveTilemap(SaveDialog1.FileName, (FilterParts.IndexOf(mask) + 1) div 2);
    FilterParts.Free;
  end else begin
    SaveDialog1.FileName := ChangeFileExt(FileName.Text,' (tilemap).inc');
    SaveDialog1.OnTypeChange(SaveDialog1);
    if SaveDialog1.Execute then SaveTilemap(SaveDialog1.FileName,SaveDialog1.FilterIndex);
  end;
end;

procedure TForm1.edTileOffsetChange(Sender: TObject);
begin
  PopulateTileDataMemo;
  PopulateTilemapDataMemo;
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
  palette:array[0..16*4-1] of byte;
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

procedure SavePalette(filename:string;format:integer);
begin
  case format of
  1: SaveText(Form1.mmPalette.Lines,filename);
  2: SaveBinary(Form1.mmPalette.Lines,filename);
  else
    Application.MessageBox('Invalid file type for palette',nil,MB_ICONERROR);
  end;
end;

procedure TForm1.btnSavePaletteClick(Sender: TObject);
var
  filter:string;
  mask: string;
  FilterParts: TStringList;
begin
  // Build the save dialog filter
  filter := 'Include files (*.inc)|*.inc';
  if not rbPalConst.Checked
  then filter := filter + '|Binary files (*.bin)|*.bin';
  SaveDialog1.Filter := filter;

  if (sender = nil) then begin
    // find the index of the ext of the filename
    mask := '*' + ExtractFileExt(SaveDialog1.Filename);
    FilterParts:=TStringList.Create;
    ExtractStrings(['|'],[],PChar(filter),FilterParts);
    SavePalette(SaveDialog1.FileName, (FilterParts.IndexOf(mask) + 1) div 2);
    FilterParts.Free;
  end else begin
    SaveDialog1.FileName := ChangeFileExt(FileName.Text,' (palette).inc');
    SaveDialog1.OnTypeChange(SaveDialog1);
    if SaveDialog1.Execute then SavePalette(SaveDialog1.FileName,SaveDialog1.FilterIndex);
  end;
end;

procedure TForm1.SaveDialog1TypeChange(Sender: TObject);
var
  DlgParent: HWND;
  StrFileName, StrExt: string;
  FilterParts:TStringList;
begin
  DlgParent := GetParent(TSaveDialog(Sender).Handle);

  // find the ext from the filter
  FilterParts:=TStringList.Create;
  try
    FilterParts.Text := StrUtils.AnsiReplaceStr(SaveDialog1.Filter, '|', #13#10);
    if (FilterParts.Count > SaveDialog1.FilterIndex * 2 - 1)
    then StrExt:=Copy(FilterParts[SaveDialog1.FilterIndex * 2 - 1], 2, 100)
    else exit;
  finally
    FilterParts.Free;
  end;

  StrFileName := ChangeFileExt(ExtractFileName(TSaveDialog(Sender).FileName), StrExt);

  if (DlgParent>0)
  then SendMessage(DlgParent, CDM_SETCONTROLTEXT, 1152, Longint(PChar(StrFileName)))
  else TSaveDialog(Sender).FileName:=StrFileName;
end;

procedure TForm1.FormShow(Sender: TObject);
var
  i:integer;
  s,filename,tilefilename,tilemapfilename,palettefilename:string;
  quitafter,ignorenext,gotfile:boolean;
begin
  filename:='';
  quitafter:=false;
  ignorenext:=false;
  gotfile:=false;
  // stop all processing until I'm ready
  btnProcess.Enabled:=false;
  // command-line handling
  for i:=1 to ParamCount do begin
    if ignorenext then begin // flag set to skip the next
      ignorenext:=false;
      continue;
    end;
    s:=ParamStr(i);
    if (s[1]<>'-') and FileExists(s) then begin
      Form1.FileName.Text:=s;
      gotfile := true;
    end else if s='-8x16' then cb8x16.Checked:=true
    else if s='-8x8' then cb8x16.Checked:=false
    else if s='-planar' then cbPlanar.Checked:=true
    else if s='-chunky' then cbPlanar.Checked:=false
    else if s='-mirror' then cbUseMirroring.Checked:=true
    else if s='-nomirror' then cbUseMirroring.Checked:=false
    else if s='-removedupes' then cbRemoveDupes.Checked:=true
    else if s='-noremovedupes' then cbRemoveDupes.Checked:=false
    else if s='-spritepalette' then cbSpritePalette.Checked:=true
    else if s='-infrontofsprites' then cbInFront.Checked:=true
    else if s='-palsms' then rbPalHex.Checked:=true
    else if s='-palgg' then rbPalGG.Checked:=true
    else if s='-palcl123' then rbPalConst.Checked:=true
    else if s='-tileoffset' then begin
      edTileOffset.Text:=paramstr(i+1);
      ignorenext:=true;
    end else if s='-savetiles' then begin
      tilefilename:=paramstr(i+1);
      ignorenext:=true;
    end else if s='-savetilemap' then begin
      tilemapfilename:=paramstr(i+1);
      ignorenext:=true;
    end else if s='-savepalette' then begin
      palettefilename:=paramstr(i+1);
      ignorenext:=true;
    end else if s='-exit' then quitafter:=true
    else Application.MessageBox(PAnsiChar('Unknown parameter:'#13#10+s),nil);
  end;

  // do processing if wanted
  if gotfile then begin
    btnLoadClick(nil);

    // Save results if specified
    if Length(tilefilename) > 0 then begin
      SaveDialog1.FileName := tilefilename;
      btnSaveTilesRawClick(nil);
    end;

    if Length(tilemapfilename) > 0 then begin
      SaveDialog1.FileName := tilemapfilename;
      btnSaveReconstClick(nil);
    end;

    if Length(palettefilename) > 0 then begin
      SaveDialog1.FileName := palettefilename;
      btnSavePaletteClick(nil);
    end;
  end;

  if quitafter then Application.Terminate;
end;

procedure TForm1.cb8x16Click(Sender: TObject);
begin
  if fileexists(FileName.Text) then btnLoadClick(Sender);
end;

procedure TForm1.FormClose(Sender: TObject; var Action: TCloseAction);
var
  i:integer;
begin
  tilehexcodes.free;

  if Assigned(tiledata) then FreeMem(tiledata);

  for i:=0 to Length(plugins) - 1 do begin
    FreeLibrary(plugins[i].Handle);
  end;
  SetLength(plugins, 0);
end;

procedure TForm1.PopulateTileDataMemo;
var
  i,j,offset:integer;
  s:string;
  p:PByteArray;
begin
  p := tiledata;
  offset := StrToIntDef(edTileOffset.Text, 0);
  with mmResults.Lines do begin
    BeginUpdate;
    Clear;
    for i:=0 to tilehexcodes.Count - 1 do begin
      Add('; Tile index $' + IntToHex(i + offset, 3));
      s:='.db';
      for j:=0 to 31 do begin
        s := s + ' $' + IntToHex(p^[j], 2);
      end;
      Add(s);
      p := PByteArray(Integer(p) + 32);
    end;
    EndUpdate;
  end;
end;

procedure TForm1.PopulateTilemapDataMemo;
var
  i,j,index,flags,offset,extraflags:integer;
  s:string;
  wp:PWordArray;
begin
  wp := tilemap;
  offset := StrToIntDef(edTileOffset.Text, 0);
  extraflags := 0;
  if (cbSpritePalette.Checked) then extraflags := extraflags or $0800;
  if (cbInFront.Checked) then extraflags := extraflags or $1000;
  with mmReconstData.Lines do begin
    BeginUpdate;
    Clear;
    for i:=0 to tilemapheight-1 do begin
      s:='.dw';
      for j:=0 to tilemapwidth-1 do begin
        index := (wp^[j] and $01ff) + offset;
        flags := (wp^[j] and $0600) or extraflags;
        s := s + ' $' + IntToHex(index or flags,4);
      end;
      Add(s);
      wp := PWordArray(Integer(wp) + tilemapwidth * SizeOf(Word));
    end;
    EndUpdate;
  end;
end;

end.
