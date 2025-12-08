import numpy as np
import pydicom
import cv2
import torch
from model_segmentation import DSUNet

SIZE = 256
MEAN = np.array([0.485, 0.456, 0.406], dtype=np.float32)
STD  = np.array([0.229, 0.224, 0.225], dtype=np.float32)

def load_dicom_as_rgb(path):
    dicom = pydicom.dcmread(path)
    img = dicom.pixel_array.astype(np.float32)

    img -= img.min()
    img /= (img.max() + 1e-8)
    img = (img * 255).astype(np.uint8)

    img = cv2.resize(img, (SIZE, SIZE))
    img_rgb = cv2.cvtColor(img, cv2.COLOR_GRAY2RGB)

    return img_rgb

def preprocess_image_no_aug(img_rgb):
    img = img_rgb.astype(np.float32) / 255.0
    img = (img - MEAN) / STD
    img = torch.tensor(img).permute(2, 0, 1)
    return img.unsqueeze(0)

def predict_single_dicom(path, model, device, alpha=0.4, threshold=0.3):
    model.eval()

    img_rgb = load_dicom_as_rgb(path)
    img_tensor = preprocess_image_no_aug(img_rgb).to(device)

    with torch.no_grad():
        pred = torch.sigmoid(model(img_tensor)).cpu().numpy()[0, 0]

    mask_binary = (pred > threshold).astype(np.uint8)

    mask_color = np.zeros_like(img_rgb, dtype=np.float32)
    mask_color[..., 0] = mask_binary * 1.0

    img_float = img_rgb.astype(np.float32) / 255.0

    overlay = img_float.copy()
    overlay = (1 - alpha) * overlay + alpha * mask_color
    overlay = np.clip(overlay, 0, 1)

    return pred, overlay, mask_binary
