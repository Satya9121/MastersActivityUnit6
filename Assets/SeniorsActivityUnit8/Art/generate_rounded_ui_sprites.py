import os
from PIL import Image, ImageDraw, ImageFilter

ART_DIR = os.path.dirname(os.path.abspath(__file__))

def create_rounded_box_9slice():
    """
    Generates a crisp, premium, high-DPI 9-slice rounded rectangle sprite.
    Pure white base with subtle smooth edge anti-aliasing and soft ambient bevel
    so that Unity UI Image color tinting works flawlessly for all colors.
    """
    w, h = 256, 256
    scale = 4
    sw, sh = w * scale, h * scale
    radius = 44 * scale
    
    img = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    pad = 4 * scale
    
    # 1. Soft Outer Ambient Shadow (Very subtle dark glow for depth)
    shadow = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    sdraw = ImageDraw.Draw(shadow)
    sdraw.rounded_rectangle([pad, pad + 2 * scale, sw - pad, sh - pad + 2 * scale], radius=radius, fill=(0, 0, 0, 45))
    shadow = shadow.filter(ImageFilter.GaussianBlur(3 * scale))
    img.alpha_composite(shadow)
    
    # 2. Main White Rounded Body
    body_img = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    bdraw = ImageDraw.Draw(body_img)
    bdraw.rounded_rectangle([pad, pad, sw - pad, sh - pad], radius=radius, fill=(255, 255, 255, 255))
    
    # 3. Very subtle top-to-bottom soft gradient (255 at top down to 240 at bottom for rich tactile depth)
    grad = Image.new("L", (sw, sh), 255)
    gdraw = ImageDraw.Draw(grad)
    for y in range(pad, sh - pad):
        t = (y - pad) / max(1, (sh - 2 * pad))
        val = int(255 - t * 14) # Very slight 5% gradient
        gdraw.line([(pad, y), (sw - pad, y)], fill=val)
        
    mask = Image.new("L", (sw, sh), 0)
    mdraw = ImageDraw.Draw(mask)
    mdraw.rounded_rectangle([pad, pad, sw - pad, sh - pad], radius=radius, fill=255)
    
    white_layer = Image.new("RGBA", (sw, sh), (255, 255, 255, 255))
    body_img.paste(white_layer, (0, 0), mask)
    
    # 4. Subtle Inner Highlight / Top Lip
    inner_highlight = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    ihdraw = ImageDraw.Draw(inner_highlight)
    ihdraw.rounded_rectangle([pad + 2 * scale, pad + 2 * scale, sw - pad - 2 * scale, pad + 16 * scale], radius=int(radius * 0.8), fill=(255, 255, 255, 60))
    
    # 5. Crisp Outer Border
    border_img = Image.new("RGBA", (sw, sh), (0, 0, 0, 0))
    brdraw = ImageDraw.Draw(border_img)
    brdraw.rounded_rectangle([pad, pad, sw - pad, sh - pad], radius=radius, outline=(255, 255, 255, 240), width=int(1.5 * scale))
    
    img.alpha_composite(body_img)
    img.alpha_composite(inner_highlight)
    img.alpha_composite(border_img)
    
    final_img = img.resize((w, h), Image.Resampling.LANCZOS)
    out_path = os.path.join(ART_DIR, "UI_RoundedBox_9Slice.png")
    final_img.save(out_path, "PNG")
    print(f"Generated UI_RoundedBox_9Slice.png ({w}x{h}) -> {out_path}")
    
    # Update / ensure .meta has matching spriteBorder
    meta_path = out_path + ".meta"
    # We set spriteBorder to 48px on each side (out of 256px) so 9-slicing preserves the 44px radius corners
    update_meta_border(meta_path, border_x=48, border_y=48, border_z=48, border_w=48)

def update_meta_border(meta_path, border_x=48, border_y=48, border_z=48, border_w=48):
    if not os.path.exists(meta_path):
        return
    with open(meta_path, "r", encoding="utf-8") as f:
        meta_text = f.read()
        
    import re
    meta_text = re.sub(r'spriteBorder:\s*\{[^}]*\}', f'spriteBorder: {{x: {border_x}, y: {border_y}, z: {border_z}, w: {border_w}}}', meta_text)
    
    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(meta_text)
    print(f"Updated spriteBorder in {meta_path} to x:{border_x} y:{border_y} z:{border_z} w:{border_w}")

if __name__ == "__main__":
    create_rounded_box_9slice()
