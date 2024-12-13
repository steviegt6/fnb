use core::slice;
use std::ptr;

use image::{codecs::png::PngEncoder, ImageEncoder, RgbaImage};
use libc::size_t;

#[no_mangle]
pub extern "C" fn decode_png(data: *const u8, length: size_t) -> *mut RgbaImage
{
    if data.is_null() || length == 0 {
        return ptr::null_mut();
    }

    let slice = unsafe { slice::from_raw_parts(data, length) };

    match image::load_from_memory_with_format(slice, image::ImageFormat::Png) {
        Ok(image) => {
            let rgba_image = image.to_rgba8();
            Box::into_raw(Box::new(rgba_image))
        }
        Err(_) => ptr::null_mut(),
    }
}

#[no_mangle]
pub extern "C" fn encode_png(image: *const RgbaImage, length: *mut size_t) -> *mut u8 {
    if image.is_null() {
        return ptr::null_mut();
    }

    let rgba_image = unsafe { &*image };

    let mut buf = Vec::new();
    let encoder = PngEncoder::new(&mut buf);
    match encoder.write_image(&rgba_image, rgba_image.width(), rgba_image.height(), image::ExtendedColorType::Rgba8) {
        Ok(_) => {
            unsafe {
                *length = buf.len() as size_t;
            }
            let data = buf.as_mut_ptr();
            std::mem::forget(buf);
            data
        }
        Err(_) => ptr::null_mut(),
    }
}

#[no_mangle]
pub extern "C" fn create_image_from_raw_data(width: size_t, height: size_t, data: *const u8) -> *mut RgbaImage {
    if data.is_null() {
        return ptr::null_mut();
    }

    let slice = unsafe { slice::from_raw_parts(data, (width * height * 4) as usize) };

    let image = RgbaImage::from_raw(width as u32, height as u32, slice.to_vec());
    match image {
        Some(image) => Box::into_raw(Box::new(image)),
        None => ptr::null_mut(),
    }
}

#[no_mangle]
pub extern "C" fn free_decoded_image(image: *mut RgbaImage) {
    if !image.is_null() {
        unsafe {
            drop(Box::from_raw(image));
        }
    }
}

#[no_mangle]
pub extern "C" fn free_encoded_png(data: *mut u8) {
    if !data.is_null() {
        unsafe {
            drop(Vec::from_raw_parts(data, 0, 0));
        }
    }
}
