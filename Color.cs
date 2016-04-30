namespace GSLogger
{
    public class GsColor
    {
        public GsColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public GsColor(byte r, byte g, byte b, byte a)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public byte R { get; }

        public byte G { get; }

        public byte B { get; }

        public byte A { get; }

        //static colors
        public static GsColor Red => new GsColor(200, 20, 20);

        public static GsColor Maroon => new GsColor(128, 0, 0);

        public static GsColor Green => new GsColor(20, 200, 20);

        public static GsColor DarkGreen => new GsColor(0, 128, 0);

        public static GsColor Blue => new GsColor(20, 20, 200);

        public static GsColor DarkBlue => new GsColor(0, 0, 128);

        public static GsColor Magenta => new GsColor(200, 20, 200);

        public static GsColor Cyan => new GsColor(20, 200, 200);

        public static GsColor Yellow => new GsColor(200, 200, 20);

        public static GsColor Olive => new GsColor(128, 128, 0);

        public static GsColor Gray => new GsColor(128, 128, 128);

        public static GsColor LightGray => new GsColor(192, 192, 192);

        public static GsColor DarkGray => new GsColor(64, 64, 64);

        public static GsColor Orange => new GsColor(255, 127, 0);

        public static GsColor RedOrange => new GsColor(255, 69, 0);

        public static GsColor Black => new GsColor(0, 0, 0);

        public static GsColor White => new GsColor(255, 255, 255);

        public static GsColor GrayGreen => new GsColor(85, 110, 65);

        public static GsColor Debug => new GsColor(50, 50, 50);

        public static GsColor Info => new GsColor(0, 0, 0);

        public static GsColor Warning => Orange;

        public static GsColor Error => new GsColor(128, 0, 0);

        public string ToHexadecimal()
        {
            return R.ToString("X2") + G.ToString("X2") + B.ToString("X2");
        }

        public override string ToString()
        {
            return $"GSColor R:{R} G:{G} B:{B} A:{A}";
        }
    }
}