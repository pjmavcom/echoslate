namespace Echoslate.Core.Theming;

public readonly struct ColorRgba(byte r, byte g, byte b, byte a = 255) {
	public byte R { get; } = r;
	public byte G { get; } = g;
	public byte B { get; } = b;
	public byte A { get; } = a;

	public static readonly ColorRgba Default = new(127, 127, 127);
	public static readonly ColorRgba White = new(255, 255, 255);
	public static readonly ColorRgba Black = new(0, 0, 0);

	public static readonly ColorRgba Transparent = new(0, 0, 0, 0);
	
	public static readonly ColorRgba AccentBlue = new(0, 122, 204);
	public static readonly ColorRgba SuccessGreen = new(40, 167, 69);
	public static readonly ColorRgba DangerRed = new(220, 53, 69);
	public static readonly ColorRgba RefactorBlue = new(0, 123, 255);
	public static readonly ColorRgba ChoreGray = new(108, 117, 125);
	public static readonly ColorRgba DocsYellow = new(253, 203, 110);
	public static readonly ColorRgba EditingOrange = new(255, 98, 0);
	
	
	public static readonly ColorRgba SeverityHigh = new(190, 0, 0);
	public static readonly ColorRgba SeverityMed = new(200, 160, 0);
	public static readonly ColorRgba SeverityLow = new(0, 140, 0);
	public static readonly ColorRgba SeverityNone = new(50, 50, 50);
	public static readonly ColorRgba SeverityOff = new(25, 25, 25);


	public static readonly ColorRgba AppBackground = new(43, 43, 43);

	
	public static readonly ColorRgba HighlightBrushKey = new(43, 87, 154);
	public static readonly ColorRgba AppDarkBackground = new(30, 31, 34);
	public static readonly ColorRgba ControlBackground = new(14, 14, 14);
	public static readonly ColorRgba EditorBackground = new(0, 0, 0);
	public static readonly ColorRgba RaisedBackground = new(52, 59, 65);
	public static readonly ColorRgba Foreground = new(220, 220, 220);
	public static readonly ColorRgba SubtleForeground = new(153, 153, 153);
	public static readonly ColorRgba LightAccent = new(0, 159, 255);
	public static readonly ColorRgba Accent = new(0, 122, 204);
	public static readonly ColorRgba AccentHover = new(0, 122, 204, 180);
	public static readonly ColorRgba AccentPressed = new(0, 122, 204, 230);
	public static readonly ColorRgba DarkAccent = new(0, 58, 119);
	public static readonly ColorRgba Border = new(41, 41, 41);
	public static readonly ColorRgba BorderSilver = new(153, 153, 153);
	public static readonly ColorRgba BorderMid = new(68, 68, 68);
	public static readonly ColorRgba BorderDark = new(34, 34, 34);
	public static readonly ColorRgba ButtonDisabledBackground = new(20, 20, 20);
	public static readonly ColorRgba ButtonBackground = new(39, 39, 39);
	public static readonly ColorRgba ButtonHoverBackground = new(55, 55, 55);
	public static readonly ColorRgba ButtonPressedBackground = new(33, 33, 33);
}