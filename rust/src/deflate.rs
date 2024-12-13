#[no_mangle]
pub extern "C" fn decompress_deflate(in_data: *const u8, in_length: usize, out_data: *mut u8, out_length: usize) -> usize {
    // Decompress a DEFLATE-compressed buffer into a buffer of raw data.
    // This is a very simple wrapper around the `fdelfate` crate used by `png`.

    let in_data = unsafe { std::slice::from_raw_parts(in_data, in_length) };
    let out_data = unsafe { std::slice::from_raw_parts_mut(out_data, out_length) };

    let decompressor = fdeflate::Decompressor::new();

    // I hate this; sorry.
    let mut decompressor = unsafe { std::mem::transmute::<fdeflate::Decompressor, MyDecompressor>(decompressor) };
    decompressor.state = State::BlockHeader;
    let mut decompressor = unsafe { std::mem::transmute::<MyDecompressor, fdeflate::Decompressor>(decompressor) };

    decompressor.read(in_data, out_data, 0, false).unwrap().1
}

struct CompressedBlock {
    _litlen_table: Box<[u32; 4096]>,
    _secondary_table: Vec<u16>,

    _dist_table: Box<[u32; 512]>,
    _dist_secondary_table: Vec<u16>,

    _eof_code: u16,
    _eof_mask: u16,
    _eof_bits: u8,
}

struct BlockHeader {
    _hlit: usize,
    _hdist: usize,
    _hclen: usize,
    _num_lengths_read: usize,

    _table: [u32; 128],
    _code_lengths: [u8; 320],
}

enum State {
    _ZlibHeader,
    BlockHeader,
    _CodeLengthCodes,
    _CodeLengths,
    _CompressedData,
    _UncompressedData,
    _Checksum,
    _Done,
}

pub struct Adler32 {
    _a: u16,
    _b: u16,
    _update: Adler32Imp,
}

type Adler32Imp = fn(u16, u16, &[u8]) -> (u16, u16);

struct MyDecompressor {
    _compression: CompressedBlock,
    _header: BlockHeader,
    _uncompressed_bytes_left: u16,

    _buffer: u64,
    _nbits: u8,

    _queued_rle: Option<(u8, usize)>,
    _queued_backref: Option<(usize, usize)>,
    _last_block: bool,
    _fixed_table: bool,

    state: State,
    _checksum: Adler32,
    _ignore_adler32: bool,
}