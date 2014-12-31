program bmp2tile;

uses
  Forms,
  Unit1 in 'Unit1.pas' {Form1},
  GraphicEx in 'GraphicEx\GraphicEx.pas',
  GraphicColor in 'GraphicEx\GraphicColor.pas',
  GraphicCompression in 'GraphicEx\GraphicCompression.pas',
  GraphicStrings in 'GraphicEx\GraphicStrings.pas',
  JPG in 'GraphicEx\JPG.pas',
  MZLib in 'GraphicEx\MZLib.pas';

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TForm1, Form1);
  Application.Run;
end.
