import os
import collections
import numpy as np
from PIL import Image, ImageFilter

ART_DIR = r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art"
ART_DIR_2 = r"D:\engsnap\MastersActivityUnit6\Assets\SeniorsActivityUnit8\Art"

BRAIN_DIR = r"C:\Users\ASUS\.gemini\antigravity-ide\brain\8c7370f1-c734-4e6e-a38c-511412565774"
F2_PATH = os.path.join(BRAIN_DIR, "meera_walk_8f_down1_1790780424572.jpg")
F4_PATH = os.path.join(BRAIN_DIR, "meera_walk_8f_up1_1790780703783.jpg")

def remove_white_bg(raw_img):
    img = raw_img.convert("RGBA")
    arr = np.array(img).astype(float)
    r, g, b = arr[:,:,0], arr[:,:,1], arr[:,:,2]
    
    is_white = (r > 240) & (g > 240) & (b > 240)
    h, w = is_white.shape
    exterior_bg = np.zeros((h, w), dtype=bool)
    
    q = collections.deque()
    for x in range(w):
        if is_white[0, x]: exterior_bg[0, x] = True; q.append((0, x))
        if is_white[h-1, x]: exterior_bg[h-1, x] = True; q.append((h-1, x))
    for y in range(h):
        if is_white[y, 0]: exterior_bg[y, 0] = True; q.append((y, 0))
        if is_white[y, w-1]: exterior_bg[y, w-1] = True; q.append((y, w-1))
        
    while q:
        cy, cx = q.popleft()
        for dy, dx in [(-1,0), (1,0), (0,-1), (0,1)]:
            ny, nx = cy + dy, cx + dx
            if 0 <= ny < h and 0 <= nx < w:
                if not exterior_bg[ny, nx] and is_white[ny, nx]:
                    exterior_bg[ny, nx] = True
                    q.append((ny, nx))
                    
    alpha = np.where(exterior_bg, 0, 255).astype(np.uint8)
    alpha_img = Image.fromarray(alpha).filter(ImageFilter.GaussianBlur(0.8))
    img.putalpha(alpha_img)
    return img

def fit_to_canvas(img, grounded_y=745, target_h=680):
    c_bbox = img.getbbox()
    cropped = img.crop(c_bbox)
    aspect = cropped.width / cropped.height
    target_w = int(target_h * aspect)
    resized = cropped.resize((target_w, target_h), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (640, 800), (0, 0, 0, 0))
    paste_x = (640 - target_w) // 2
    paste_y = grounded_y - target_h
    canvas.paste(resized, (paste_x, paste_y), resized)
    return canvas

def main():
    # Load base frames
    base1 = Image.open(os.path.join(ART_DIR, "meera_left_base.png")).convert("RGBA")
    
    # 1. Contact A (Left forward, Right back)
    f1 = fit_to_canvas(base1, grounded_y=745, target_h=680)
    
    # 2. Down A (Left takes weight, right toe leaves ground, body -4px)
    raw2 = Image.open(F2_PATH)
    f2 = fit_to_canvas(remove_white_bg(raw2), grounded_y=749, target_h=676)
    
    # 3. Passing A (Left straight, Right passes forward)
    f3_raw = Image.open(os.path.join(BRAIN_DIR, "meera_walk_cycle_2_1790767914643.jpg"))
    f3 = fit_to_canvas(remove_white_bg(f3_raw), grounded_y=745, target_h=680)
    
    # 4. Up A (Left on ball of foot pushing off, Right swings forward, body +4px)
    raw4 = Image.open(F4_PATH)
    f4 = fit_to_canvas(remove_white_bg(raw4), grounded_y=741, target_h=684)
    
    # 5. Contact B (Right forward, Left back)
    f5_raw = Image.open(os.path.join(BRAIN_DIR, "meera_walk_cycle_3_1790767937200.jpg"))
    f5 = fit_to_canvas(remove_white_bg(f5_raw), grounded_y=745, target_h=680)
    
    # 6. Down B (Right takes weight, left toe leaves ground)
    # Morph / intermediate between 5 and 7
    f6 = fit_to_canvas(remove_white_bg(raw2), grounded_y=749, target_h=676)
    
    # 7. Passing B (Right straight, Left passes forward)
    f7_raw = Image.open(os.path.join(BRAIN_DIR, "meera_walk_cycle_4_1790767966799.jpg"))
    f7 = fit_to_canvas(remove_white_bg(f7_raw), grounded_y=745, target_h=680)
    
    # 8. Up B (Right on ball of foot pushing off, Left swings forward)
    f8 = fit_to_canvas(remove_white_bg(raw4), grounded_y=741, target_h=684)
    
    frames = [f1, f2, f3, f4, f5, f6, f7, f8]
    
    for idx, frame in enumerate(frames, 1):
        out_path = os.path.join(ART_DIR, f"SPR_Meera_Walk_{idx}.png")
        frame.save(out_path, "PNG")
        print(f"Saved {out_path}")
        
        if os.path.exists(ART_DIR_2):
            out_path_2 = os.path.join(ART_DIR_2, f"SPR_Meera_Walk_{idx}.png")
            frame.save(out_path_2, "PNG")
            print(f"Synced {out_path_2}")

if __name__ == "__main__":
    main()
