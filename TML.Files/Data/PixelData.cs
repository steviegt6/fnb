namespace TML.Files.Data
{
    /// <summary>
    ///     Struct containing R, G, B, and A byte values. Represents a single pixel in a <c>.raw</c>.
    /// </summary>
    public readonly struct PixelData
    {
        /// <summary>
        ///     Byte channel of the red channel.
        /// </summary>
        public readonly byte R;

        /// <summary>
        ///     Byte channel of the green channel.
        /// </summary>
        public readonly byte G;
        
        /// <summary>
        ///     Byte channel of the blue channel.
        /// </summary>
        public readonly byte B;

        /// <summary>
        ///     Byte value of the alpha channel.
        /// </summary>
        public readonly byte A;

        /// <summary>
        ///     Constructs a new <see cref="PixelData"/> instance.
        /// </summary>
        public PixelData(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }
}