import torch
import torch.nn as nn

class DenseBlock(nn.Module):
    def __init__(self, in_channels, growth_rate=32, num_layers=4):
        super(DenseBlock, self).__init__()
        self.layers=nn.ModuleList()
        for i in range(num_layers):
            layer = self._make_dense_layer(in_channels + i * growth_rate, growth_rate)
            self.layers.append(layer)
        
    def _make_dense_layer(self, in_channels, growth_rate):
        return nn.Sequential(
            nn.BatchNorm2d(in_channels), nn.ReLU(inplace=True),
            nn.Conv2d(in_channels, growth_rate * 4, kernel_size=1, bias=False),
            nn.BatchNorm2d(growth_rate * 4), nn.ReLU(inplace=True),
            nn.Conv2d(growth_rate * 4, growth_rate, kernel_size=3, padding=1, bias=False)
        )
    def forward(self, x):
        features = [x]
        for layer in self.layers:
            combined_input = torch.cat(features, dim=1)
            new_features = layer(combined_input)
            features.append(new_features)
        return torch.cat(features, dim=1)

class AttentionBlock(nn.Module):
    def __init__(self, F_g, F_l, F_int):
        super(AttentionBlock, self).__init__()
        self.W_g = nn.Sequential(
            nn.Conv2d(F_g, F_int, kernel_size=1),
            nn.BatchNorm2d(F_int))
        self.W_x = nn.Sequential(
            nn.Conv2d(F_l, F_int, kernel_size=1),
            nn.BatchNorm2d(F_int))
        self.psi = nn.Sequential(
            nn.Conv2d(F_int, 1, kernel_size=1),
            nn.BatchNorm2d(1),
            nn.Sigmoid())
        self.relu = nn.ReLU(inplace=True)
    
    def forward(self, g, x):
        g1 = self.W_g(g)
        x1 = self.W_x(x)
        psi = self.relu(g1 + x1)
        psi = self.psi(psi)
        return x * psi

class DSUNet(nn.Module):

    def __init__(self, in_channels=3, n_classes=1, base_filters=64):
        super(DSUNet, self).__init__()

        def _make_encoder_block(in_c, out_c):
            return nn.Sequential(
                nn.Conv2d(in_c, out_c, 3, padding=1), nn.BatchNorm2d(out_c), nn.ReLU(inplace=True),
                nn.Conv2d(out_c, out_c, 3, padding=1), nn.BatchNorm2d(out_c), nn.ReLU(inplace=True)
            )
        def _make_decoder_block(in_c, out_c):
            return nn.Sequential(
                nn.Conv2d(in_c, out_c, 3, padding=1), nn.BatchNorm2d(out_c), nn.ReLU(inplace=True),
                nn.Conv2d(out_c, out_c, 3, padding=1), nn.BatchNorm2d(out_c), nn.ReLU(inplace=True)
            )
        
        self.enc1 = _make_encoder_block(in_channels, base_filters)
        self.pool1 = nn.MaxPool2d(2)
        self.enc2 = _make_encoder_block(base_filters, base_filters * 2)
        self.pool2 = nn.MaxPool2d(2)
        self.enc3 = _make_encoder_block(base_filters * 2, base_filters * 4)
        self.pool3 = nn.MaxPool2d(2)
        self.enc4 = _make_encoder_block(base_filters * 4, base_filters * 8)
        self.pool4 = nn.MaxPool2d(2)

        self.bottleneck = DenseBlock(base_filters * 8, growth_rate=32, num_layers=6)
        bottleneck_out_channels = base_filters * 8 + 32 * 6

        self.up4 = nn.ConvTranspose2d(bottleneck_out_channels, base_filters * 8, 2, stride=2)
        self.att4 = AttentionBlock(base_filters * 8, base_filters * 8, base_filters * 4)
        self.dec4 = _make_decoder_block(base_filters * 16 , base_filters * 8)

        self.up3 = nn.ConvTranspose2d(base_filters * 8, base_filters * 4, 2, stride=2)
        self.att3 = AttentionBlock(base_filters * 4, base_filters * 4, base_filters * 2)
        self.dec3 = _make_decoder_block(base_filters * 8, base_filters * 4)

        self.up2 = nn.ConvTranspose2d(base_filters * 4, base_filters * 2, 2, stride=2)
        self.att2 = AttentionBlock(base_filters * 2, base_filters * 2, base_filters)
        self.dec2 = _make_decoder_block(base_filters * 4, base_filters * 2)

        self.up1 = nn.ConvTranspose2d(base_filters * 2, base_filters, 2, stride=2)
        self.att1 = AttentionBlock(base_filters, base_filters, base_filters // 2)
        self.dec1 = _make_decoder_block(base_filters * 2, base_filters)

        self.final = nn.Conv2d(base_filters, n_classes, kernel_size=1)
        
    def forward(self, x):
        e1 = self.enc1(x)
        e2 = self.enc2(self.pool1(e1))
        e3 = self.enc3(self.pool2(e2))
        e4 = self.enc4(self.pool3(e3))
        b = self.bottleneck(self.pool4(e4))

        d4 = self.up4(b)
        e4_att = self.att4(d4, e4)
        d4 = self.dec4(torch.cat([d4, e4_att], dim=1))

        d3 = self.up3(d4)
        e3_att = self.att3(d3, e3)
        d3 = self.dec3(torch.cat([d3, e3_att], dim=1))

        d2 = self.up2(d3)
        e2_att = self.att2(d2, e2)
        d2 = self.dec2(torch.cat([d2, e2_att], dim=1))

        d1 = self.up1(d2)
        e1_att = self.att1(d1, e1)
        d1 = self.dec1(torch.cat([d1, e1_att], dim=1))

        out = self.final(d1)
        return out
