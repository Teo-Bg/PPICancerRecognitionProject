import torch
import torch.nn as nn
from torchvision import models


class MultiTaskResNet(nn.Module):
    def __init__(self, num_types, num_classes):
        super().__init__()

        backbone = models.resnet18(weights=models.ResNet18_Weights.IMAGENET1K_V1)


        self.features = nn.Sequential(*list(backbone.children())[:-1])
        in_features = backbone.fc.in_features

        self.dropout = nn.Dropout(0.3)

        self.type_head = nn.Linear(in_features, num_types)

        self.class_head = nn.Linear(in_features, num_classes)

    def forward(self, x):
        x = self.features(x)
        x = torch.flatten(x, 1)
        x = self.dropout(x)

        type_logits = self.type_head(x)      
        class_logits = self.class_head(x)    

        return type_logits, class_logits


def load_classification_model(model_path, num_types, num_classes, device):
    model = MultiTaskResNet(num_types, num_classes)
    model.load_state_dict(torch.load(model_path, map_location=device))
    model.to(device)
    model.eval()
    return model
