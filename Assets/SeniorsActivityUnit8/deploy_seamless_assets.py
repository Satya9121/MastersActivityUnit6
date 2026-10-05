import os
import shutil
import numpy as np
from PIL import Image

def build_perfect_seamless_assets():
    cw, ch = 580, 754
    ground_y = 735
    target_h = 675

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

    # Load isolated ponytail crop
    pt = Image.open(r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art\test_isolated_ponytail.png").convert("RGBA")
    pw, ph = pt.size
    scaled_pt = pt.resize((int(pw * 1.15), int(ph * 1.15)), resample=Image.BICUBIC)

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

        # 1. Authentic Anu Walk Frame
        a_frame = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
        fx = (cw - nw) // 2
        fy = ground_y - nh
        a_frame.paste(scaled, (fx, fy), scaled)
        anu_walk_frames.append(a_frame)

        # 2. Authentic Meera Walk Frame (100% SEAMLESS Neck/Collar + Royal Blue + High Ponytail)
        blue_arr = recolor_red_to_blue(np.array(a_frame))

        # Erase only the low braided pigtail behind neck at x > 330, y in [240, 420]
        # Preserve white shirt collar & sleeve completely!
        for y in range(240, 420):
            for x in range(325, 460):
                if blue_arr[y, x, 3] > 0:
                    is_white_shirt = (blue_arr[y, x, 0] > 180) and (blue_arr[y, x, 1] > 180) and (blue_arr[y, x, 2] > 180)
                    if not is_white_shirt:
                        blue_arr[y, x] = [0, 0, 0, 0]

        meera_frame = Image.fromarray(blue_arr)
        # Paste high ponytail seamlessly
        meera_frame.paste(scaled_pt, (305, 45), scaled_pt)
        meera_walk_frames.append(meera_frame)

    anu_idle_frame = anu_walk_frames[1]
    meera_idle_frame = meera_walk_frames[1]

    # =========================================================================
    # 2. 5 ANU KNOCKING POSES (PERFECT ACTION ANIMATION)
    # =========================================================================
    standing = anu_walk_frames[1].copy()
    standing_arr = np.array(standing)
    body_clean = standing_arr.copy()
    body_clean[375:510, 230:330] = 0
    red_color = standing_arr[410, 215]
    for y in range(375, 480):
        for x in range(230, 305):
            if standing_arr[y, 210, 3] > 100:
                body_clean[y, x] = red_color
    body_filled = Image.fromarray(body_clean)

    arm_crop = standing.crop((225, 365, 330, 505))

    # Pose 0: Resting
    k_rest = standing.copy()

    # Pose 1: Raise (~30 deg)
    k_raise = body_filled.copy()
    rot_raise = arm_crop.rotate(30, resample=Image.BICUBIC, expand=True)
    k_raise.paste(rot_raise, (190, 345), rot_raise)

    # Pose 2: Reach (~55 deg)
    k_reach = body_filled.copy()
    rot_reach = arm_crop.rotate(55, resample=Image.BICUBIC, expand=True)
    k_reach.paste(rot_reach, (155, 315), rot_reach)

    # Pose 3: Contact (~80 deg, hand touching door wood)
    k_contact = body_filled.copy()
    rot_contact = arm_crop.rotate(80, resample=Image.BICUBIC, expand=True)
    k_contact.paste(rot_contact, (120, 290), rot_contact)

    # Pose 4: Retract (~62 deg, hand rebounded)
    k_retract = body_filled.copy()
    rot_retract = arm_crop.rotate(62, resample=Image.BICUBIC, expand=True)
    k_retract.paste(rot_retract, (148, 308), rot_retract)

    # =========================================================================
    # 3. SAVE ALL ARTWORK TO BOTH PROJECTS
    # =========================================================================
    target_dirs = [
        r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art",
        r"D:\engsnap\MastersActivityUnit6\Assets\SeniorsActivityUnit8\Art"
    ]

    for td in target_dirs:
        if not os.path.exists(td):
            continue

        # Anu Walk Frames (1..4) & Idle
        for idx, f in enumerate(anu_walk_frames):
            f.save(os.path.join(td, f"SPR_Anu_Walk_{idx+1}.png"))
        anu_idle_frame.save(os.path.join(td, "SPR_Anu_Walk_Idle.png"))

        # Meera Walk Frames (1..4) & Idle (100% SEAMLESS matching Image 2!)
        for idx, f in enumerate(meera_walk_frames):
            f.save(os.path.join(td, f"SPR_Meera_Walk_{idx+1}.png"))
        meera_idle_frame.save(os.path.join(td, "SPR_Meera_Walk_Idle.png"))
        meera_walk_frames[0].save(os.path.join(td, "SPR_Meera_Entering.png"))

        # 5 Clean Knock Poses
        k_rest.save(os.path.join(td, "SPR_Anu_Knock_0.png"))
        k_raise.save(os.path.join(td, "SPR_Anu_Knock_1.png"))
        k_reach.save(os.path.join(td, "SPR_Anu_Knock_2.png"))
        k_contact.save(os.path.join(td, "SPR_Anu_Knock_3.png"))
        k_retract.save(os.path.join(td, "SPR_Anu_Knock_4.png"))
        k_contact.save(os.path.join(td, "SPR_Anu_Knocking.png"))

        print("Deployed seamless assets to:", td)

    print("Master asset generation complete!")

if __name__ == "__main__":
    build_perfect_seamless_assets()
