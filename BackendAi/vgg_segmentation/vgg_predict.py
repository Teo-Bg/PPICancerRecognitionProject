import numpy as np
import pydicom
import cv2
from preprocessing import preprocess_and_orient_image


TARGET_SIZE = (512, 512)

def predict_single_dicom_vgg(
    dicom_path,
    model,
    alpha=0.4,
    threshold=0.5
):
    ds = pydicom.dcmread(dicom_path)
    original_image = ds.pixel_array

def predict_single_dicom_vgg(
    dicom_path,
    model,
    alpha=0.4,
    threshold=0.5
):

    ds = pydicom.dcmread(dicom_path)
    original_image = ds.pixel_array

    processed_image, was_flipped = preprocess_and_orient_image(
        original_image,
        target_size=TARGET_SIZE
    )


    input_tensor = processed_image[np.newaxis, ..., np.newaxis]
    pred = model.predict(input_tensor, verbose=0)[0, ..., 0]
    mask_binary = (pred > threshold).astype(np.uint8)
    if was_flipped:
        mask_binary = np.fliplr(mask_binary)
        pred = np.fliplr(pred)
        processed_image = np.fliplr(processed_image)

    img_rgb = np.stack([processed_image] * 3, axis=-1)

    mask_color = np.zeros_like(img_rgb, dtype=np.float32)
    mask_color[..., 0] = mask_binary

    overlay = (1 - alpha) * img_rgb + alpha * mask_color
    overlay = np.clip(overlay, 0, 1)

    return pred, overlay, mask_binary
