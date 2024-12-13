// int width, int height, byte* data, out nint size -> byte*
#[no_mangle]
pub extern "C" fn encode_png(width: u32, height: u32, data: *const u8, length: *mut usize) -> *mut u8 {
    // Encode a tML "raw img" (literally just width, height, data) into a PNG
    // using the `png` crate as opposed to the `image` crate.

    let data = unsafe { std::slice::from_raw_parts(data, (width * height * 4) as usize) };
    let mut buf = Vec::new();

    // Pretty sure this is all we need to do to set the encoder up as expected.
    // Don't think any other options need configuring.
    let mut encoder = png::Encoder::new(&mut buf, width, height);
    encoder.set_color(png::ColorType::Rgba);
    encoder.set_depth(png::BitDepth::Eight);
    
    let mut writer = encoder.write_header().unwrap();
    writer.write_image_data(data).unwrap();
    writer.finish().unwrap();

    // I LOVE C INTEROP AGHHH
    unsafe { *length = buf.len() }

    let ptr = buf.as_mut_ptr();
    std::mem::forget(buf);

    ptr
}

// byte* data -> void
#[no_mangle]
pub extern "C" fn free_encoded_png(data: *mut u8) {
    if !data.is_null() {
        unsafe {
            drop(Vec::from_raw_parts(data, 0, 0));
        }
    }
}