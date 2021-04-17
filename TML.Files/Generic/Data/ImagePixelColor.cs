namespace TML.Files.Generic.Data
{
    public readonly struct ImagePixelColor
    {
        public readonly int r;
        public readonly int g;
        public readonly int b;
        public readonly int a;

        public ImagePixelColor(int r, int g, int b, int a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }
}
