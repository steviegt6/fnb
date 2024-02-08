/* This file was derived from libmspack
 * (C) 2003-2004 Stuart Caie.
 * (C) 2011 Ali Scissons.
 *
 * The LZX method was created by Jonathan Forbes and Tomi Poutanen, adapted
 * by Microsoft Corporation.
 *
 * This source file is Dual licensed; meaning the end-user of this source file
 * may redistribute/modify it under the LGPL 2.1 or MS-PL licenses.
 *
 *
 * GNU LESSER GENERAL PUBLIC LICENSE version 2.1
 * LzxDecoder is free software; you can redistribute it and/or modify it under
 * the terms of the GNU Lesser General Public License (LGPL) version 2.1.
 *
 *
 * MICROSOFT PUBLIC LICENSE
 * This source code is subject to the terms of the Microsoft Public License (Ms-PL).
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * is permitted provided that redistributions of the source code retain the above
 * copyright notices and this file header.
 *
 * Additional copyright notices should be appended to the list above.
 *
 * For details, see <http://www.opensource.org/licenses/ms-pl.html>.
 *
 *
 * This derived work is recognized by Stuart Caie and is authorized to adapt
 * any changes made to lzxd.c in his libmspack library and will still retain
 * this dual licensing scheme. Big thanks to Stuart Caie!
 *
 * DETAILS
 * This file is a pure C# port of the lzxd.c file from libmspack, with minor
 * changes towards the decompression of XNB files. The original decompression
 * software of LZX encoded data was written by Suart Caie in his
 * libmspack/cabextract projects, which can be located at
 * http://http://www.cabextract.org.uk/
 */

using System.IO;

namespace Tomat.FNB.XNB.LZX;

internal sealed class LzxDecoder {
    private static readonly uint[] position_base;
    private static readonly byte[] extra_bits;

    static LzxDecoder() {
        extra_bits = new byte[52];
        for (int i = 0, j = 0; i <= 50; i += 2) {
            extra_bits[i] = extra_bits[i + 1] = (byte)j;
            if (i != 0 && j < 17) j++;
        }

        position_base = new uint[51];
        for (int i = 0, j = 0; i <= 50; i++) {
            position_base[i] = (uint)j;
            j += 1 << extra_bits[i];
        }
    }

    private LzxState state;

    public LzxDecoder (int window) {
        var windowSize = (uint)(1 << window);
        int positionSlots;

        if (window is < 15 or > 21)
            throw new UnsupportedWindowSizeRange();

        state.Window = new byte[windowSize];
        {
            for (var i = 0; i < windowSize; i++)
                state.Window[i] = 0xDC;
        }

        state.ActualSize = windowSize;
        state.WindowSize = windowSize;
        state.WindowPosition = 0;

        // Calculate required position slots.
        positionSlots = window switch {
            20 => 42,
            21 => 50,
            _ => window << 1,
        };

        state.R0 = state.R1 = state.R2 = 1;
        state.MainElements = (ushort)(LzxConstants.NUM_CHARS + (positionSlots << 3));
        state.HeaderRead = false;
        state.FramesRead = 0;
        state.BlocksRemaining = 0;
        state.BlockType = LzxConstants.BlockType.Invalid;
        state.IntelCurrentPosition = 0;
        state.IntelStarted = 0;

        state.PreTreeTable = new ushort[(1 << LzxConstants.PRETREE_TABLE_BITS) + (LzxConstants.PRETREE_MAX_SYMBOLS << 1)];
        state.PreTreeLength = new byte[LzxConstants.PRETREE_MAX_SYMBOLS + LzxConstants.LEN_TABLE_SAFETY];
        state.MainTreeTable = new ushort[(1 << LzxConstants.MAINTREE_TABLE_BITS) + (LzxConstants.MAINTREE_MAX_SYMBOLS << 1)];
        state.MainTreeLength = new byte[LzxConstants.MAINTREE_MAX_SYMBOLS + LzxConstants.LEN_TABLE_SAFETY];
        state.LengthTable = new ushort[(1 << LzxConstants.LENGTH_TABLE_BITS) + (LzxConstants.LENGTH_MAX_SYMBOLS << 1)];
        state.LengthLength = new byte[LzxConstants.LENGTH_MAX_SYMBOLS + LzxConstants.LEN_TABLE_SAFETY];
        state.AlignedTable = new ushort[(1 << LzxConstants.ALIGNED_TABLE_BITS) + (LzxConstants.ALIGNED_MAX_SYMBOLS << 1)];
        state.AlignedLength = new byte[LzxConstants.ALIGNED_MAX_SYMBOLS + LzxConstants.LEN_TABLE_SAFETY];

        // Initialize tables to 0 (because deltas will be applied to them).
        for (var i = 0; i < LzxConstants.MAINTREE_MAX_SYMBOLS; i++)
            state.MainTreeLength[i] = 0;

        for (var i = 0; i < LzxConstants.LENGTH_MAX_SYMBOLS; i++)
            state.LengthLength[i] = 0;
    }

    public int Decompress(Stream inData, int inLen, Stream outData, int outLen) {
        var bitbuf = new BitBuffer(inData);
        var startpos = inData.Position;
        var endpos = inData.Position + inLen;

        var window = state.Window;

        var window_posn = state.WindowPosition;
        var window_size = state.WindowSize;
        var R0 = state.R0;
        var R1 = state.R1;
        var R2 = state.R2;
        uint i, j;

        int togo = outLen, this_run, main_element, match_length, match_offset, length_footer, extra, verbatim_bits;
        int rundest, runsrc, copy_length, aligned_bits;

        bitbuf.InitBitStream();

        /* read header if necessary */
        if (!state.HeaderRead) {
            var intel = bitbuf.ReadBits(1);
            if (intel != 0) {
                // read the filesize
                i = bitbuf.ReadBits(16);
                j = bitbuf.ReadBits(16);
                state.IntelFileSize = (int)((i << 16) | j);
            }

            state.HeaderRead = true;
        }

        /* main decoding loop */
        while (togo > 0) {
            /* last block finished, new block expected */
            if (state.BlocksRemaining == 0) {
                // TODO may screw something up here
                if (state.BlockType == LzxConstants.BlockType.Uncompressed) {
                    if ((state.BlockLength & 1) == 1) inData.ReadByte(); /* realign bitstream to word */
                    bitbuf.InitBitStream();
                }

                state.BlockType = (LzxConstants.BlockType)bitbuf.ReadBits(3);
                ;
                i = bitbuf.ReadBits(16);
                j = bitbuf.ReadBits(8);
                state.BlocksRemaining = state.BlockLength = (uint)((i << 8) | j);

                switch (state.BlockType) {
                    case LzxConstants.BlockType.Aligned:
                        for (i = 0, j = 0; i < 8; i++) {
                            j = bitbuf.ReadBits(3);
                            state.AlignedLength[i] = (byte)j;
                        }

                        MakeDecodeTable(
                            LzxConstants.ALIGNED_MAX_SYMBOLS,
                            LzxConstants.ALIGNED_TABLE_BITS,
                            state.AlignedLength,
                            state.AlignedTable
                        );

                        /* rest of aligned header is same as verbatim */
                        goto case LzxConstants.BlockType.Verbatim;

                    case LzxConstants.BlockType.Verbatim:
                        ReadLengths(state.MainTreeLength, 0, 256, bitbuf);
                        ReadLengths(state.MainTreeLength, 256, state.MainElements, bitbuf);
                        MakeDecodeTable(
                            LzxConstants.MAINTREE_MAX_SYMBOLS,
                            LzxConstants.MAINTREE_TABLE_BITS,
                            state.MainTreeLength,
                            state.MainTreeTable
                        );

                        if (state.MainTreeLength[0xE8] != 0) state.IntelStarted = 1;

                        ReadLengths(state.LengthLength, 0, LzxConstants.NUM_SECONDARY_LENGTHS, bitbuf);
                        MakeDecodeTable(
                            LzxConstants.LENGTH_MAX_SYMBOLS,
                            LzxConstants.LENGTH_TABLE_BITS,
                            state.LengthLength,
                            state.LengthTable
                        );

                        break;

                    case LzxConstants.BlockType.Uncompressed:
                        state.IntelStarted = 1; /* because we can't assume otherwise */
                        bitbuf.EnsureBits(16); /* get up to 16 pad bits into the buffer */
                        if (bitbuf.GetBitsLeft() > 16) inData.Seek(-2, SeekOrigin.Current); /* and align the bitstream! */
                        byte hi, mh, ml, lo;
                        lo = (byte)inData.ReadByte();
                        ml = (byte)inData.ReadByte();
                        mh = (byte)inData.ReadByte();
                        hi = (byte)inData.ReadByte();
                        R0 = (uint)(lo | ml << 8 | mh << 16 | hi << 24);
                        lo = (byte)inData.ReadByte();
                        ml = (byte)inData.ReadByte();
                        mh = (byte)inData.ReadByte();
                        hi = (byte)inData.ReadByte();
                        R1 = (uint)(lo | ml << 8 | mh << 16 | hi << 24);
                        lo = (byte)inData.ReadByte();
                        ml = (byte)inData.ReadByte();
                        mh = (byte)inData.ReadByte();
                        hi = (byte)inData.ReadByte();
                        R2 = (uint)(lo | ml << 8 | mh << 16 | hi << 24);
                        break;

                    default:
                        return -1; // TODO throw proper exception
                }
            }

            /* buffer exhaustion check */
            if (inData.Position > startpos + inLen) {
                /* it's possible to have a file where the next run is less than
                 * 16 bits in size. In this case, the READ_HUFFSYM() macro used
                 * in building the tables will exhaust the buffer, so we should
                 * allow for this, but not allow those accidentally read bits to
                 * be used (so we check that there are at least 16 bits
                 * remaining - in this boundary case they aren't really part of
                 * the compressed data)
                 */
                //Debug.WriteLine("WTF");

                if (inData.Position > startpos + inLen + 2 || bitbuf.GetBitsLeft() < 16) return -1; //TODO throw proper exception
            }

            while ((this_run = (int)state.BlocksRemaining) > 0 && togo > 0) {
                if (this_run > togo) this_run = togo;
                togo -= this_run;
                state.BlocksRemaining -= (uint)this_run;

                /* apply 2^x-1 mask */
                window_posn &= window_size - 1;
                /* runs can't straddle the window wraparound */
                if (window_posn + this_run > window_size)
                    return -1; //TODO throw proper exception

                switch (state.BlockType) {
                    case LzxConstants.BlockType.Verbatim:
                        while (this_run > 0) {
                            main_element = (int)ReadHuffSym(
                                state.MainTreeTable,
                                state.MainTreeLength,
                                LzxConstants.MAINTREE_MAX_SYMBOLS,
                                LzxConstants.MAINTREE_TABLE_BITS,
                                bitbuf
                            );

                            if (main_element < LzxConstants.NUM_CHARS) {
                                /* literal: 0 to NUM_CHARS-1 */
                                window[window_posn++] = (byte)main_element;
                                this_run--;
                            }
                            else {
                                /* match: NUM_CHARS + ((slot<<3) | length_header (3 bits)) */
                                main_element -= LzxConstants.NUM_CHARS;

                                match_length = main_element & LzxConstants.NUM_PRIMARY_LENGTHS;
                                if (match_length == LzxConstants.NUM_PRIMARY_LENGTHS) {
                                    length_footer = (int)ReadHuffSym(
                                        state.LengthTable,
                                        state.LengthLength,
                                        LzxConstants.LENGTH_MAX_SYMBOLS,
                                        LzxConstants.LENGTH_TABLE_BITS,
                                        bitbuf
                                    );

                                    match_length += length_footer;
                                }

                                match_length += LzxConstants.MIN_MATCH;

                                match_offset = main_element >> 3;

                                if (match_offset > 2) {
                                    /* not repeated offset */
                                    if (match_offset != 3) {
                                        extra = extra_bits[match_offset];
                                        verbatim_bits = (int)bitbuf.ReadBits((byte)extra);
                                        match_offset = (int)position_base[match_offset] - 2 + verbatim_bits;
                                    }
                                    else {
                                        match_offset = 1;
                                    }

                                    /* update repeated offset LRU queue */
                                    R2 = R1;
                                    R1 = R0;
                                    R0 = (uint)match_offset;
                                }
                                else if (match_offset == 0) {
                                    match_offset = (int)R0;
                                }
                                else if (match_offset == 1) {
                                    match_offset = (int)R1;
                                    R1 = R0;
                                    R0 = (uint)match_offset;
                                }
                                else /* match_offset == 2 */ {
                                    match_offset = (int)R2;
                                    R2 = R0;
                                    R0 = (uint)match_offset;
                                }

                                rundest = (int)window_posn;
                                this_run -= match_length;

                                /* copy any wrapped around source data */
                                if (window_posn >= match_offset) {
                                    /* no wrap */
                                    runsrc = rundest - match_offset;
                                }
                                else {
                                    runsrc = rundest + ((int)window_size - match_offset);
                                    copy_length = match_offset - (int)window_posn;
                                    if (copy_length < match_length) {
                                        match_length -= copy_length;
                                        window_posn += (uint)copy_length;
                                        while (copy_length-- > 0) window[rundest++] = window[runsrc++];
                                        runsrc = 0;
                                    }
                                }

                                window_posn += (uint)match_length;

                                /* copy match data - no worries about destination wraps */
                                while (match_length-- > 0) window[rundest++] = window[runsrc++];
                            }
                        }

                        break;

                    case LzxConstants.BlockType.Aligned:
                        while (this_run > 0) {
                            main_element = (int)ReadHuffSym(
                                state.MainTreeTable,
                                state.MainTreeLength,
                                LzxConstants.MAINTREE_MAX_SYMBOLS,
                                LzxConstants.MAINTREE_TABLE_BITS,
                                bitbuf
                            );

                            if (main_element < LzxConstants.NUM_CHARS) {
                                /* literal 0 to NUM_CHARS-1 */
                                window[window_posn++] = (byte)main_element;
                                this_run--;
                            }
                            else {
                                /* match: NUM_CHARS + ((slot<<3) | length_header (3 bits)) */
                                main_element -= LzxConstants.NUM_CHARS;

                                match_length = main_element & LzxConstants.NUM_PRIMARY_LENGTHS;
                                if (match_length == LzxConstants.NUM_PRIMARY_LENGTHS) {
                                    length_footer = (int)ReadHuffSym(
                                        state.LengthTable,
                                        state.LengthLength,
                                        LzxConstants.LENGTH_MAX_SYMBOLS,
                                        LzxConstants.LENGTH_TABLE_BITS,
                                        bitbuf
                                    );

                                    match_length += length_footer;
                                }

                                match_length += LzxConstants.MIN_MATCH;

                                match_offset = main_element >> 3;

                                if (match_offset > 2) {
                                    /* not repeated offset */
                                    extra = extra_bits[match_offset];
                                    match_offset = (int)position_base[match_offset] - 2;
                                    if (extra > 3) {
                                        /* verbatim and aligned bits */
                                        extra -= 3;
                                        verbatim_bits = (int)bitbuf.ReadBits((byte)extra);
                                        match_offset += verbatim_bits << 3;
                                        aligned_bits = (int)ReadHuffSym(
                                            state.AlignedTable,
                                            state.AlignedLength,
                                            LzxConstants.ALIGNED_MAX_SYMBOLS,
                                            LzxConstants.ALIGNED_TABLE_BITS,
                                            bitbuf
                                        );

                                        match_offset += aligned_bits;
                                    }
                                    else if (extra == 3) {
                                        /* aligned bits only */
                                        aligned_bits = (int)ReadHuffSym(
                                            state.AlignedTable,
                                            state.AlignedLength,
                                            LzxConstants.ALIGNED_MAX_SYMBOLS,
                                            LzxConstants.ALIGNED_TABLE_BITS,
                                            bitbuf
                                        );

                                        match_offset += aligned_bits;
                                    }
                                    else if (extra > 0) /* extra==1, extra==2 */ {
                                        /* verbatim bits only */
                                        verbatim_bits = (int)bitbuf.ReadBits((byte)extra);
                                        match_offset += verbatim_bits;
                                    }
                                    else /* extra == 0 */ {
                                        /* ??? */
                                        match_offset = 1;
                                    }

                                    /* update repeated offset LRU queue */
                                    R2 = R1;
                                    R1 = R0;
                                    R0 = (uint)match_offset;
                                }
                                else if ( match_offset == 0) {
                                    match_offset = (int)R0;
                                }
                                else if (match_offset == 1) {
                                    match_offset = (int)R1;
                                    R1 = R0;
                                    R0 = (uint)match_offset;
                                }
                                else /* match_offset == 2 */ {
                                    match_offset = (int)R2;
                                    R2 = R0;
                                    R0 = (uint)match_offset;
                                }

                                rundest = (int)window_posn;
                                this_run -= match_length;

                                /* copy any wrapped around source data */
                                if (window_posn >= match_offset) {
                                    /* no wrap */
                                    runsrc = rundest - match_offset;
                                }
                                else {
                                    runsrc = rundest + ((int)window_size - match_offset);
                                    copy_length = match_offset - (int)window_posn;
                                    if (copy_length < match_length) {
                                        match_length -= copy_length;
                                        window_posn += (uint)copy_length;
                                        while (copy_length-- > 0) window[rundest++] = window[runsrc++];
                                        runsrc = 0;
                                    }
                                }

                                window_posn += (uint)match_length;

                                /* copy match data - no worries about destination wraps */
                                while (match_length-- > 0) window[rundest++] = window[runsrc++];
                            }
                        }

                        break;

                    case LzxConstants.BlockType.Uncompressed:
                        if (inData.Position + this_run > endpos) return -1; //TODO throw proper exception

                        var temp_buffer = new byte[this_run];
                        inData.Read(temp_buffer, 0, this_run);
                        temp_buffer.CopyTo(window, (int)window_posn);
                        window_posn += (uint)this_run;
                        break;

                    default:
                        return -1; //TODO throw proper exception
                }
            }
        }

        if (togo != 0) return -1; //TODO throw proper exception

        var start_window_pos = (int)window_posn;
        if (start_window_pos == 0) start_window_pos = (int)window_size;
        start_window_pos -= outLen;
        outData.Write(window, start_window_pos, outLen);

        state.WindowPosition = window_posn;
        state.R0 = R0;
        state.R1 = R1;
        state.R2 = R2;

        // TODO finish intel E8 decoding
        /* intel E8 decoding */
        if (state.FramesRead++ < 32768 && state.IntelFileSize != 0) {
            if (outLen <= 6 || state.IntelStarted == 0) {
                state.IntelCurrentPosition += outLen;
            }
            else {
                var dataend = outLen - 10;
                var curpos = (uint)state.IntelCurrentPosition;

                state.IntelCurrentPosition = (int)curpos + outLen;

                while (outData.Position < dataend) {
                    if (outData.ReadByte() != 0xE8) {
                        curpos++;
                        continue;
                    }
                }
            }

            return -1;
        }

        return 0;
    }

    // READ_LENGTHS(table, first, last)
    // if(lzx_read_lens(LENTABLE(table), first, last, bitsleft))
    //   return ERROR (ILLEGAL_DATA)
    // 

    // TODO make returns throw exceptions
    private int MakeDecodeTable(uint nsyms, uint nbits, byte[] length, ushort[] table) {
        ushort sym;
        uint leaf;
        byte bit_num = 1;
        uint fill;
        uint pos   = 0; /* the current position in the decode table */
        var table_mask  = (uint)(1 << (int)nbits);
        var bit_mask  = table_mask >> 1; /* don't do 0 length codes */
        var next_symbol = bit_mask; /* base of allocation for long codes */

        /* fill entries for codes short enough for a direct mapping */
        while (bit_num <= nbits ) {
            for (sym = 0; sym < nsyms; sym++) {
                if (length[sym] == bit_num) {
                    leaf = pos;

                    if ((pos += bit_mask) > table_mask) return 1; /* table overrun */

                    /* fill all possible lookups of this symbol with the symbol itself */
                    fill = bit_mask;
                    while (fill-- > 0) table[leaf++] = sym;
                }
            }

            bit_mask >>= 1;
            bit_num++;
        }

        /* if there are any codes longer than nbits */
        if (pos != table_mask) {
            /* clear the remainder of the table */
            for (sym = (ushort)pos; sym < table_mask; sym++) table[sym] = 0;

            /* give ourselves room for codes to grow by up to 16 more bits */
            pos <<= 16;
            table_mask <<= 16;
            bit_mask = 1 << 15;

            while (bit_num <= 16) {
                for (sym = 0; sym < nsyms; sym++) {
                    if (length[sym] == bit_num) {
                        leaf = pos >> 16;
                        for (fill = 0; fill < bit_num - nbits; fill++) {
                            /* if this path hasn't been taken yet, 'allocate' two entries */
                            if (table[leaf] == 0) {
                                table[next_symbol << 1] = 0;
                                table[(next_symbol << 1) + 1] = 0;
                                table[leaf] = (ushort)next_symbol++;
                            }

                            /* follow the path and select either left or right for next bit */
                            leaf = (uint)(table[leaf] << 1);
                            if (((pos >> (int)(15 - fill)) & 1) == 1) leaf++;
                        }

                        table[leaf] = sym;

                        if ((pos += bit_mask) > table_mask) return 1;
                    }
                }

                bit_mask >>= 1;
                bit_num++;
            }
        }

        /* full talbe? */
        if (pos == table_mask) return 0;

        /* either erroneous table, or all elements are 0 - let's find out. */
        for (sym = 0; sym < nsyms; sym++)
            if (length[sym] != 0)
                return 1;

        return 0;
    }

    // TODO throw exceptions instead of returns
    private void ReadLengths(byte[] lens, uint first, uint last, BitBuffer bitbuf) {
        uint x, y;
        int z;

        // hufftbl pointer here?

        for (x = 0; x < 20; x++) {
            y = bitbuf.ReadBits(4);
            state.PreTreeLength[x] = (byte)y;
        }

        MakeDecodeTable(
            LzxConstants.PRETREE_MAX_SYMBOLS,
            LzxConstants.PRETREE_TABLE_BITS,
            state.PreTreeLength,
            state.PreTreeTable
        );

        for (x = first; x < last;) {
            z = (int)ReadHuffSym(
                state.PreTreeTable,
                state.PreTreeLength,
                LzxConstants.PRETREE_MAX_SYMBOLS,
                LzxConstants.PRETREE_TABLE_BITS,
                bitbuf
            );

            if (z == 17) {
                y = bitbuf.ReadBits(4);
                y += 4;
                while (y-- != 0) lens[x++] = 0;
            }
            else if (z == 18) {
                y = bitbuf.ReadBits(5);
                y += 20;
                while (y-- != 0) lens[x++] = 0;
            }
            else if (z == 19) {
                y = bitbuf.ReadBits(1);
                y += 4;
                z = (int)ReadHuffSym(
                    state.PreTreeTable,
                    state.PreTreeLength,
                    LzxConstants.PRETREE_MAX_SYMBOLS,
                    LzxConstants.PRETREE_TABLE_BITS,
                    bitbuf
                );

                z = lens[x] - z;
                if (z < 0) z += 17;
                while (y-- != 0) lens[x++] = (byte)z;
            }
            else {
                z = lens[x] - z;
                if (z < 0) z += 17;
                lens[x++] = (byte)z;
            }
        }
    }

    private uint ReadHuffSym(ushort[] table, byte[] lengths, uint nsyms, uint nbits, BitBuffer bitbuf) {
        uint i, j;
        bitbuf.EnsureBits(16);
        if ((i = table[bitbuf.PeekBits((byte)nbits)]) >= nsyms) {
            j = (uint)(1 << (int)(sizeof(uint) * 8 - nbits));
            do {
                j >>= 1;
                i <<= 1;
                i |= (bitbuf.GetBuffer() & j) != 0 ? (uint)1 : 0;
                if (j == 0) return 0; // TODO throw proper exception
            } while ((i = table[i]) >= nsyms);
        }

        j = lengths[i];
        bitbuf.RemoveBits((byte)j);

        return i;
    }

    #region Our BitBuffer Class
    private class BitBuffer {
        uint buffer;
        byte bitsleft;
        Stream byteStream;

        public BitBuffer(Stream stream) {
            byteStream = stream;
            InitBitStream();
        }

        public void InitBitStream() {
            buffer = 0;
            bitsleft = 0;
        }

        public void EnsureBits(byte bits) {
            while (bitsleft < bits) {
                int lo = (byte)byteStream.ReadByte();
                int hi = (byte)byteStream.ReadByte();
                //int amount2shift = sizeof(uint)*8 - 16 - bitsleft;
                buffer |= (uint)(((hi << 8) | lo) << (sizeof(uint) * 8 - 16 - bitsleft));
                bitsleft += 16;
            }
        }

        public uint PeekBits(byte bits) {
            return buffer >> (sizeof(uint) * 8 - bits);
        }

        public void RemoveBits(byte bits) {
            buffer <<= bits;
            bitsleft -= bits;
        }

        public uint ReadBits(byte bits) {
            uint ret = 0;

            if (bits > 0) {
                EnsureBits(bits);
                ret = PeekBits(bits);
                RemoveBits(bits);
            }

            return ret;
        }

        public uint GetBuffer() {
            return buffer;
        }

        public byte GetBitsLeft() {
            return bitsleft;
        }
    }
    #endregion

    private struct LzxState {
        public uint R0{ get; set; }
        public uint R1{ get; set; }
        public uint R2{ get; set; }
        public ushort MainElements{ get; set; }
        public bool HeaderRead{ get; set; }
        public LzxConstants.BlockType BlockType{ get; set; }
        public uint BlockLength{ get; set; }
        public uint BlocksRemaining{ get; set; }
        public uint FramesRead{ get; set; }
        public int IntelFileSize{ get; set; }
        public int IntelCurrentPosition{ get; set; }
        public int IntelStarted{ get; set; }

        public ushort[] PreTreeTable { get; set; }

        public byte[] PreTreeLength { get; set; }

        public ushort[] MainTreeTable { get; set; }

        public byte[] MainTreeLength { get; set; }

        public ushort[] LengthTable { get; set; }

        public byte[] LengthLength { get; set; }

        public ushort[] AlignedTable { get; set; }

        public byte[] AlignedLength { get; set; }

        public uint ActualSize { get; set; }

        public byte[] Window { get; set; }

        public uint WindowSize { get; set; }

        public uint WindowPosition { get; set; }
    }
}
