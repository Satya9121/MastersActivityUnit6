import os
import shutil
import numpy as np
from PIL import Image

def deploy_perfect_solution():
    cw, ch = 580, 754
    ground_y = 735
    target_h = 675

    # =========================================================================
    # 1. AUTHENTIC ANU KNOCKING SPRITE (EXACTLY MATCHING IMAGE 2)
    # =========================================================================
    p_anu_sheet = r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art\more sprites\Character — Anu U8 SA.png"
    im_anu_sheet = Image.open(p_anu_sheet)

    # Sliced SPR_Anu_Knocking: rect x=317, y=113, w=326, h=760 in 941h
    crop_knock = im_anu_sheet.crop((317, 941 - 113 - 760, 317 + 326, 941 - 113))
    arr_k = np.array(crop_knock)
    ys, xs = np.where(arr_k[:, :, 3] > 10)
    clean_knock = crop_knock.crop((xs.min(), ys.min(), xs.max()+1, ys.max()+1))

    pw, ph = clean_knock.size
    scale = target_h / float(ph)
    nw, nh = int(pw * scale), int(ph * scale)
    scaled_knock = clean_knock.resize((nw, nh), resample=Image.BICUBIC)

    frame_knock = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    fx = (cw - nw) // 2
    fy = ground_y - nh
    frame_knock.paste(scaled_knock, (fx, fy), scaled_knock)

    # Also extract authentic SPR_Anu_Sheepish
    crop_sheep = im_anu_sheet.crop((667, 118, 667 + 286, 840))
    arr_s = np.array(crop_sheep)
    ys, xs = np.where(arr_s[:, :, 3] > 10)
    clean_sheep = crop_sheep.crop((xs.min(), ys.min(), xs.max()+1, ys.max()+1))
    pw_s, ph_s = clean_sheep.size
    scale_s = target_h / float(ph_s)
    nw_s, nh_s = int(pw_s * scale_s), int(ph_s * scale_s)
    scaled_sheep = clean_sheep.resize((nw_s, nh_s), resample=Image.BICUBIC)
    frame_sheep = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    frame_sheep.paste(scaled_sheep, ((cw - nw_s) // 2, ground_y - nh_s), scaled_sheep)

    # =========================================================================
    # 2. SEAMLESS MEERA WALKING FRAMES (EXACTLY MATCHING IMAGE 1)
    # =========================================================================
    media_path = r"C:\Users\ASUS\.gemini\antigravity-ide\brain\42cc47cf-a3d9-4f33-925f-a40c1262c46d\.tempmediaStorage\media_1790836853699.png"
    im = Image.open(media_path).convert("RGBA")
    arr = np.array(im)

    bg = np.array([220, 220, 220], dtype=np.float32)
    diff = np.max(np.abs(arr[:, :, :3].astype(np.float32) - bg), axis=2)
    clean_arr = arr.copy()
    clean_arr[diff <= 22] = [0, 0, 0, 0]

    bounds = [
        (110, 485),   # Pose 1: Contact A
        (800, 1120),  # Pose 2: Passing A
        (1400, 1795), # Pose 3: Contact B
        (2050, 2420)  # Pose 4: Passing B
    ]

    def recolor_red_to_blue(frame_arr):
        rgb = frame_arr[:, :, :3].astype(np.float32) / 255.0
        r, g, b = rgb[:, :, 0], rgb[:, :, 1], rgb[:, :, 2]
        max_c = np.maximum(np.maximum(r, g), b)
        min_c = np.minimum(np.minimum(r, g), b)
        delta = max_c - min_c

        h = np.zeros_like(r)
        mask_r = (max_c == r) & (delta > 0)
        mask_g = (max_c == g) & (delta > 0)
        mask_b = (max_c == b) & (delta > 0)

        h[mask_r] = ((g[mask_r] - b[mask_r]) / delta[mask_r]) % 6.0
        h[mask_g] = ((b[mask_g] - r[mask_g]) / delta[mask_g]) + 2.0
        h[mask_b] = ((r[mask_b] - g[mask_b]) / delta[mask_b]) + 4.0
        h = h * 60.0

        s = np.zeros_like(r)
        s[max_c > 0] = delta[max_c > 0] / max_c[max_c > 0]
        v = max_c

        red_mask = ((h < 16) | (h > 342)) & (s > 0.45) & (v > 0.20) & (frame_arr[:, :, 3] > 80)
        h_new = 218.0
        s_new = np.clip(s * 1.15, 0.0, 1.0)
        v_new = np.clip(v * 0.95, 0.0, 1.0)
        c = v_new * s_new
        x_c = c * (1 - np.abs((h_new / 60.0) % 2 - 1))
        m = v_new - c

        r_blue = 0.0 + m
        g_blue = x_c + m
        b_blue = c + m

        blue_arr = frame_arr.copy()
        blue_arr[red_mask, 0] = np.clip(r_blue[red_mask] * 255, 0, 255).astype(np.uint8)
        blue_arr[red_mask, 1] = np.clip(g_blue[red_mask] * 255, 0, 255).astype(np.uint8)
        blue_arr[red_mask, 2] = np.clip(b_blue[red_mask] * 255, 0, 255).astype(np.uint8)
        return blue_arr

    # Isolated ponytail from Image 1
    pt = Image.open(r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art\test_isolated_ponytail.png").convert("RGBA")
    pw_pt, ph_pt = pt.size
    scaled_pt = pt.resize((int(pw_pt * 1.15), int(ph_pt * 1.15)), resample=Image.BICUBIC)

    anu_walk_frames = []
    meera_walk_frames = []

    for i, (x1, x2) in enumerate(bounds):
        sub = clean_arr[:, x1:x2]
        ys, xs = np.where(sub[:, :, 3] > 0)
        crop = Image.fromarray(sub[ys.min():ys.max()+1, xs.min():xs.max()+1])
        pw, ph = crop.size
        scale = target_h / float(ph)
        nw, nh = int(pw * scale), int(ph * scale)
        scaled = crop.resize((nw, nh), resample=Image.BICUBIC)

        # Anu Walk Frame
        a_frame = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
        fx = (cw - nw) // 2
        fy = ground_y - nh
        a_frame.paste(scaled, (fx, fy), scaled)
        anu_walk_frames.append(a_frame)

        # Meera Walk Frame (100% SEAMLESS, matching Image 1)
        blue_arr = recolor_red_to_blue(np.array(a_frame))
        for y in range(240, 420):
            for x in range(325, 460):
                if blue_arr[y, x, 3] > 0:
                    is_white_shirt = (blue_arr[y, x, 0] > 180) and (blue_arr[y, x, 1] > 180) and (blue_arr[y, x, 2] > 180)
                    if not is_white_shirt:
                        blue_arr[y, x] = [0, 0, 0, 0]

        meera_frame = Image.fromarray(blue_arr)
        meera_frame.paste(scaled_pt, (305, 45), scaled_pt)
        meera_walk_frames.append(meera_frame)

    anu_idle_frame = anu_walk_frames[1]
    meera_idle_frame = meera_walk_frames[1]

    # Save to both project directories
    target_dirs = [
        r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art",
        r"D:\engsnap\MastersActivityUnit6\Assets\SeniorsActivityUnit8\Art"
    ]

    for td in target_dirs:
        if not os.path.exists(td):
            continue

        # Anu Walk Frames
        for idx, f in enumerate(anu_walk_frames):
            f.save(os.path.join(td, f"SPR_Anu_Walk_{idx+1}.png"))
        anu_idle_frame.save(os.path.join(td, "SPR_Anu_Walk_Idle.png"))

        # Meera Walk Frames (Matching Image 1)
        for idx, f in enumerate(meera_walk_frames):
            f.save(os.path.join(td, f"SPR_Meera_Walk_{idx+1}.png"))
        meera_idle_frame.save(os.path.join(td, "SPR_Meera_Walk_Idle.png"))
        meera_walk_frames[0].save(os.path.join(td, "SPR_Meera_Entering.png"))

        # Authentic Anu Knocking Sprite (Matching Image 2)
        frame_knock.save(os.path.join(td, "SPR_Anu_Knocking.png"))
        frame_knock.save(os.path.join(td, "SPR_Anu_Knock_2.png"))
        frame_knock.save(os.path.join(td, "SPR_Anu_Knock_3.png"))
        anu_idle_frame.save(os.path.join(td, "SPR_Anu_Knock_0.png"))
        anu_idle_frame.save(os.path.join(td, "SPR_Anu_Knock_1.png"))
        anu_idle_frame.save(os.path.join(td, "SPR_Anu_Knock_4.png"))

        # Clean Sheepish
        frame_sheep.save(os.path.join(td, "SPR_Anu_Sheepish_Side.png"))
        frame_sheep.save(os.path.join(td, "SPR_Anu_Sheepish.png"))

        print("Saved assets to:", td)

    print("Master asset deployment complete!")

if __name__ == "__main__":
    deploy_perfect_solution()
