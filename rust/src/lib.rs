use std::ptr;

use libc::size_t;

#[no_mangle]
pub extern "C" fn encode_png(width: u32, height: u32, data: *const u8, length: *mut size_t) -> *mut u8 {
    // encode a raw image of width x height into a png using the png crate

    let data = unsafe { std::slice::from_raw_parts(data, (width * height * 4) as usize) };
    let mut buf = Vec::new();
    let mut encoder = png::Encoder::new(&mut buf, width, height);
    encoder.set_color(png::ColorType::Rgba);
    encoder.set_depth(png::BitDepth::Eight);
    let mut writer = encoder.write_header().unwrap();
    writer.write_image_data(data).unwrap();
    writer.finish().unwrap();

    unsafe { *length = buf.len() as size_t }

    let ptr = unsafe { libc::malloc(*length) as *mut u8 };
    if ptr.is_null() {
        return ptr::null_mut();
    }

    unsafe {
        ptr::copy_nonoverlapping(buf.as_ptr(), ptr, *length);
    }

    ptr
}

#[no_mangle]
pub extern "C" fn free_encoded_png(data: *mut u8) {
    if !data.is_null() {
        unsafe {
            drop(Vec::from_raw_parts(data, 0, 0));
        }
    }
}
