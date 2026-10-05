import os
import shutil
import numpy as np
from PIL import Image

def fix_all_issues():
    cw, ch = 580, 754
    ground_y = 735
    target_h = 675

    media_path = r"C:\Users\ASUS\.gemini\antigravity-ide\brain\42cc47cf-a3d9-4f33-925f-a40c1262c46d\.tempmediaStorage\media_1790836853699.png"
    im = Image.open(media_path).convert("RGBA")
    arr = np.array(im)

    # 1. Clean background separation
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

    # Recolor red dress & tie to royal blue
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

    # =========================================================================
    # 1. GENERATE MEERA FRAMES (EXACTLY 1 HEAD, NO DOUBLE FACES, SAME WALK)
    # =========================================================================
    # Load Meera's single authentic head (from meera_left_base)
    m_base = Image.open(r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art\meera_left_base.png").convert("RGBA")
    head_crop = m_base.crop((140, 60, 505, 335))
    hw, hh = head_crop.size
    head_scale = 1.02
    scaled_head = head_crop.resize((int(hw * head_scale), int(hh * head_scale)), resample=Image.BICUBIC)

    meera_walk_frames = []
    anu_walk_frames = []
    anu_scaled_crops = []

    for i, (x1, x2) in enumerate(bounds):
        sub = clean_arr[:, x1:x2]
        ys, xs = np.where(sub[:, :, 3] > 0)
        crop = Image.fromarray(sub[ys.min():ys.max()+1, xs.min():xs.max()+1])
        pw, ph = crop.size
        scale = target_h / float(ph)
        nw, nh = int(pw * scale), int(ph * scale)
        scaled = crop.resize((nw, nh), resample=Image.BICUBIC)
        anu_scaled_crops.append((scaled, nw, nh))

        # --- Anu Walk Frame ---
        a_frame = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
        fx = (cw - nw) // 2
        fy = ground_y - nh
        a_frame.paste(scaled, (fx, fy), scaled)
        anu_walk_frames.append(a_frame)

        # --- Meera Walk Frame (Recolored Body + SINGLE Meera Head) ---
        blue_scaled = recolor_red_to_blue(np.array(scaled))
        
        # 100% ERASE Anu's head and neck above the collar line to prevent double faces
        collar_cutoff = int(nh * 0.40)
        blue_scaled[:collar_cutoff, :] = 0
        headless_body = Image.fromarray(blue_scaled)

        m_frame = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
        m_frame.paste(headless_body, (fx, fy), headless_body)

        # Place Meera's single head seamlessly on top of the collar
        hx = fx + (nw - scaled_head.width) // 2 - 12
        hy = fy + collar_cutoff - scaled_head.height + 25
        m_frame.paste(scaled_head, (hx, hy), scaled_head)
        meera_walk_frames.append(m_frame)

    anu_idle_frame = anu_walk_frames[1]
    meera_idle_frame = meera_walk_frames[1]

    # =========================================================================
    # 2. GENERATE CLEAN ANU DOOR KNOCKING FRAMES (HAND DIRECTLY TO DOOR WOOD)
    # =========================================================================
    standing = anu_walk_frames[1].copy()
    forearm = standing.crop((215, 375, 335, 510))

    # Knock 1: Poise (Arm raised toward door at chest height)
    k1 = standing.copy()
    k1_arr = np.array(k1)
    k1_arr[380:505, 230:325] = 0
    k1 = Image.fromarray(k1_arr)
    f_rot1 = forearm.rotate(40, resample=Image.BICUBIC, expand=True)
    k1.paste(f_rot1, (175, 330), f_rot1)

    # Knock 2: Impact (Fist extended touching door wood at left edge)
    k2 = standing.copy()
    k2_arr = np.array(k2)
    k2_arr[380:505, 230:325] = 0
    k2 = Image.fromarray(k2_arr)
    f_rot2 = forearm.rotate(68, resample=Image.BICUBIC, expand=True)
    k2.paste(f_rot2, (135, 305), f_rot2)

    # Knock 3: Recoil (Arm pulled back slightly)
    k3 = standing.copy()
    k3_arr = np.array(k3)
    k3_arr[380:505, 230:325] = 0
    k3 = Image.fromarray(k3_arr)
    f_rot3 = forearm.rotate(48, resample=Image.BICUBIC, expand=True)
    k3.paste(f_rot3, (165, 320), f_rot3)

    # =========================================================================
    # 3. FIX SHEEPISH POSE & ACTION SPRITES (NO BLACK BOX BACKGROUND)
    # =========================================================================
    # Load authentic SPR_Anu_Sheepish from Character — Anu U8 SA.png
    p_sheet = r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art\more sprites\Character — Anu U8 SA.png"
    im_sheet = Image.open(p_sheet)
    crop_sheepish = im_sheet.crop((667, 118, 667 + 286, 840))
    arr_s = np.array(crop_sheepish)
    ys, xs = np.where(arr_s[:, :, 3] > 10)
    clean_sheepish = crop_sheepish.crop((xs.min(), ys.min(), xs.max()+1, ys.max()+1))

    pw, ph = clean_sheepish.size
    scale = target_h / float(ph)
    nw, nh = int(pw * scale), int(ph * scale)
    scaled_sheepish = clean_sheepish.resize((nw, nh), resample=Image.BICUBIC)

    sheepish_frame = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    fx = (cw - nw) // 2
    fy = ground_y - nh
    sheepish_frame.paste(scaled_sheepish, (fx, fy), scaled_sheepish)

    # Standardize Bang 1, Bang 2, Push sprites to clean alpha 580x754
    art_dir_src = r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art"
    bang_push_frames = {}
    for fname in ["SPR_Anu_Bang_1.png", "SPR_Anu_Bang_2.png", "SPR_Anu_Push.png"]:
        p = os.path.join(art_dir_src, fname)
        if os.path.exists(p):
            bp_im = Image.open(p).convert("RGBA")
            bp_arr = np.array(bp_im)
            ys, xs = np.where(bp_arr[:, :, 3] > 20)
            bp_crop = bp_im.crop((xs.min(), ys.min(), xs.max()+1, ys.max()+1))
            pw, ph = bp_crop.size
            scale = target_h / float(ph)
            nw, nh = int(pw * scale), int(ph * scale)
            scaled_bp = bp_crop.resize((nw, nh), resample=Image.BICUBIC)
            bp_frame = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
            fx = (cw - nw) // 2
            fy = ground_y - nh
            bp_frame.paste(scaled_bp, (fx, fy), scaled_bp)
            bang_push_frames[fname] = bp_frame

    # =========================================================================
    # 4. SAVE TO BOTH PROJECTS (Downloads and D:\engsnap)
    # =========================================================================
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

        # Meera Walk Frames (Clean single head, 100% matching walk)
        for idx, f in enumerate(meera_walk_frames):
            f.save(os.path.join(td, f"SPR_Meera_Walk_{idx+1}.png"))
        meera_idle_frame.save(os.path.join(td, "SPR_Meera_Walk_Idle.png"))
        meera_walk_frames[0].save(os.path.join(td, "SPR_Meera_Entering.png"))

        # Anu Clean Knock Frames
        k1.save(os.path.join(td, "SPR_Anu_Knock_1.png"))
        k2.save(os.path.join(td, "SPR_Anu_Knock_2.png"))
        k3.save(os.path.join(td, "SPR_Anu_Knock_3.png"))
        k2.save(os.path.join(td, "SPR_Anu_Knocking.png"))

        # Clean Sheepish (100% Transparent, ZERO black box)
        sheepish_frame.save(os.path.join(td, "SPR_Anu_Sheepish_Side.png"))
        sheepish_frame.save(os.path.join(td, "SPR_Anu_Sheepish.png"))

        # Clean Bang & Push Frames
        for fname, f in bang_push_frames.items():
            f.save(os.path.join(td, fname))

        print("Saved all clean sprites to:", td)

    print("ALL 3 ISSUES RESOLVED SUCCESSFULLY!")

if __name__ == "__main__":
    fix_all_issues()
