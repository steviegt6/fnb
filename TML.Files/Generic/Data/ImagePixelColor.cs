namespace TML.Files.Generic.Data
{
    /// <summary>
    ///     Struct containing R, G, B, and A values in bytes.
    /// </summary>
    public struct PixelData
    {
        /// <summary>
        /// </summary>
        public byte r;

        /// <summary>
        /// </summary>
        public byte g;
        
        /// <summary>
        /// </summary>
        public byte b;

        /// <summary>
        /// </summary>
        public byte a;

        /// <summary>
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public PixelData(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }
}