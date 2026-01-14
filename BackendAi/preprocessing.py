import numpy as np
import cv2
from skimage.exposure import equalize_hist
from skimage.filters import threshold_li
from scipy import ndimage

def remove_artifacts(image):
    """
    image: float32, valori [0,1], shape (H,W)
    """
    try:
        thresh = threshold_li(image)
    except Exception:
        thresh = np.mean(image)

    binary_mask = image > thresh

    label_im, nb_labels = ndimage.label(binary_mask)

    if nb_labels == 0:
        return image, np.ones_like(image, dtype=bool)

    sizes = ndimage.sum(binary_mask, label_im, range(1, nb_labels + 1))
    biggest_label = np.argmax(sizes) + 1

    breast_mask = (label_im == biggest_label)
    breast_mask = ndimage.binary_fill_holes(breast_mask)

    cleaned_image = np.where(breast_mask, image, 0.0)

    return cleaned_image, breast_mask

def normalize_image_for_display(image_array):

    image = image_array.astype(np.float32)
    image = (image - np.min(image)) / (np.max(image) - np.min(image))
    return equalize_hist(image)

def normalize_image_for_model(image_array):

    image = image_array.astype(np.float32)
    image = (image - np.min(image)) / (np.max(image) - np.min(image))
    return equalize_hist(image)

def preprocess_and_orient_image(image_array, target_size=(512, 512)):
    if len(image_array.shape) == 3:
        if image_array.shape[2] == 3:
            image_array = cv2.cvtColor(image_array, cv2.COLOR_RGB2GRAY)
        elif image_array.shape[2] == 1:
            image_array = image_array[:, :, 0]

    image_resized = cv2.resize(image_array, target_size, interpolation=cv2.INTER_AREA)

    image_f32 = image_resized.astype(np.float32)
    min_val, max_val = np.min(image_f32), np.max(image_f32)

    if max_val - min_val > 0:
        normalized_image = (image_f32 - min_val) / (max_val - min_val)
    else:
        normalized_image = image_f32

    H, W = normalized_image.shape
    mid_w = W // 2
    mean_left = np.mean(normalized_image[:, :mid_w])
    mean_right = np.mean(normalized_image[:, mid_w:])

    was_flipped = False
    if mean_left < mean_right:
        oriented_image = np.fliplr(normalized_image)
        was_flipped = True
    else:
        oriented_image = normalized_image

    cleaned_image, breast_mask = remove_artifacts(oriented_image)

    img_uint8 = (cleaned_image * 255).astype(np.uint8)
    clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
    clahe_image_uint8 = clahe.apply(img_uint8)
    final_processed_image = clahe_image_uint8.astype(np.float32) / 255.0
    final_processed_image[~breast_mask] = 0.0

    return final_processed_image, was_flipped
