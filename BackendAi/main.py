from fastapi import FastAPI, UploadFile, File, Response
import torch
import numpy as np
import shutil
import os
import uuid
import cv2

from model_segmentation import DSUNet
from segmentation_predict import predict_single_dicom

from model_classification import MultiTaskResNet
from classification_predict import predict_classification

from vgg_segmentation.vgg_model import load_vgg_model
from vgg_segmentation.vgg_predict import predict_single_dicom_vgg

app = FastAPI()
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")


model_seg = DSUNet(in_channels=3, n_classes=1, base_filters=64).to(device)
model_seg.load_state_dict(torch.load("dsu_net_best.pth", map_location=device))
model_seg.eval()

vgg_model = load_vgg_model(
    "vgg16_unet_mass_segmentation_bce_dice.keras"
)

NUM_TYPES = 7
NUM_CLASSES = 4

ABN_TYPES = ['CALC', 'CIRC', 'SPIC', 'MISC', 'ARCH', 'ASYM', 'NORM']
ABN_CLASSES = ['B', 'M', 'N', 'NoCancer']

model_clf = MultiTaskResNet(NUM_TYPES, NUM_CLASSES).to(device)
model_clf.load_state_dict(
    torch.load("best_dmid_multitask_resnet18.pth", map_location=device)
)
model_clf.eval()

TEMP_DIR = "temp"
os.makedirs(TEMP_DIR, exist_ok=True)

@app.post("/predict/")
async def predict(file: UploadFile = File(...)):
    if not file.filename.lower().endswith(".dcm"):
        return {"error": "Only DICOM (.dcm) files are accepted."}

    temp_path = os.path.join(TEMP_DIR, f"{uuid.uuid4()}.dcm")
    with open(temp_path, "wb") as buffer:
        shutil.copyfileobj(file.file, buffer)

    try:
        _, overlay, _ = predict_single_dicom(temp_path, model_seg, device)

        overlay_uint8 = (overlay * 255).astype(np.uint8)
        overlay_bgr = cv2.cvtColor(overlay_uint8, cv2.COLOR_RGB2BGR)

        success, encoded_image = cv2.imencode(".png", overlay_bgr)
        if not success:
            return {"error": "Could not encode output image"}

        return Response(
            content=encoded_image.tobytes(),
            media_type="image/png"
        )
    finally:
        os.remove(temp_path)

@app.post("/predict-full/")
async def predict_full(file: UploadFile = File(...)):
    if not file.filename.lower().endswith(".dcm"):
        return {"error": "Only DICOM (.dcm) files are accepted."}

    temp_path = os.path.join(TEMP_DIR, f"{uuid.uuid4()}.dcm")
    with open(temp_path, "wb") as buffer:
        shutil.copyfileobj(file.file, buffer)

    try:
        _, _, mask_binary = predict_single_dicom(
            temp_path, model_seg, device
        )

        predicted_types, predicted_class, type_probs, class_probs = (
            predict_classification(
                model_clf,
                temp_path,
                ABN_TYPES,
                ABN_CLASSES,
                device
            )
        )

        return {
            "mask_binary": mask_binary.tolist(),
            "classification": {
                "predicted_types": predicted_types,
                "predicted_class": predicted_class,
                "type_probs": type_probs.tolist(),
                "class_probs": class_probs.tolist()
            },
            "message": "Success"
        }
    finally:
        os.remove(temp_path)

@app.post("/predict-full-vgg/")
async def predict_full_vgg(file: UploadFile = File(...)):
    if not file.filename.lower().endswith(".dcm"):
        return {"error": "Only DICOM (.dcm) files are accepted."}

    temp_path = os.path.join(TEMP_DIR, f"{uuid.uuid4()}.dcm")
    with open(temp_path, "wb") as buffer:
        shutil.copyfileobj(file.file, buffer)

    try:
        _, _, mask_binary = predict_single_dicom_vgg(
            temp_path,
            vgg_model
        )

        predicted_types, predicted_class, type_probs, class_probs = (
            predict_classification(
                model_clf,
                temp_path,
                ABN_TYPES,
                ABN_CLASSES,
                device
            )
        )

        return {
            "mask_binary": mask_binary.tolist(),
            "classification": {
                "predicted_types": predicted_types,
                "predicted_class": predicted_class,
                "type_probs": type_probs.tolist(),
                "class_probs": class_probs.tolist()
            },
            "message": "Success"
        }
    finally:
        os.remove(temp_path)
