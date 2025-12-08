import torch
import numpy as np
import cv2
import pydicom

from torchvision import transforms


IMG_SIZE = 512

MEAN = [0.485, 0.456, 0.406]
STD  = [0.229, 0.224, 0.225]


val_transform = transforms.Compose([
    transforms.ToTensor(),
    transforms.Resize((IMG_SIZE, IMG_SIZE)),
    transforms.Normalize(mean=MEAN, std=STD),
])


def load_dicom_rgb(path):
    dicom = pydicom.dcmread(path)
    img = dicom.pixel_array.astype(np.float32)

    img -= img.min()
    img /= (img.max() + 1e-8)
    img = (img * 255).astype(np.uint8)

    img = cv2.resize(img, (IMG_SIZE, IMG_SIZE))
    img = cv2.cvtColor(img, cv2.COLOR_GRAY2RGB)

    return img


def predict_classification(model, dicom_path, abn_types, abn_classes, device):


    img = load_dicom_rgb(dicom_path)
    img_tensor = val_transform(img).unsqueeze(0).to(device)

    with torch.no_grad():
        type_logits, class_logits = model(img_tensor)

        type_probs = torch.sigmoid(type_logits).cpu().numpy()[0]

        class_probs = torch.softmax(class_logits, dim=1).cpu().numpy()[0]

    predicted_types = [abn_types[i] for i, p in enumerate(type_probs) if p > 0.5]


    predicted_class = abn_classes[int(class_probs.argmax())]

    return predicted_types, predicted_class, type_probs, class_probs
