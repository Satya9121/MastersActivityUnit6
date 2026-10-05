import os
import numpy as np
from PIL import Image

def generate_clean_sprites_and_controller():
    scratch = r"C:\Users\ASUS\.gemini\antigravity-ide\brain\42cc47cf-a3d9-4f33-925f-a40c1262c46d\scratch"
    cw, ch = 580, 754
    ground_y = 735
    target_h = 675

    # =========================================================================
    # 1. CRISP 4-KEYFRAME WALKING SPRITES (NO GHOSTING / NO BLENDING)
    # =========================================================================
    walk_frames = []
    for i in range(1, 5):
        p = os.path.join(scratch, f"authentic_anu_pose_{i}.png")
        img = Image.open(p).convert("RGBA")
        pw, ph = img.size
        scale = target_h / float(ph)
        nw, nh = int(pw * scale), int(ph * scale)
        scaled = img.resize((nw, nh), resample=Image.BICUBIC)

        frame = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
        fx = (cw - nw) // 2
        fy = ground_y - nh
        frame.paste(scaled, (fx, fy), scaled)
        walk_frames.append(frame)

    art_dirs = [
        r"D:\engsnap\MastersActivityUnit6\Assets\SeniorsActivityUnit8\Art",
        r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art"
    ]

    # Authentic standing still idle pose (feet together, natural resting posture)
    im_norm = Image.open(os.path.join(art_dirs[1], "crop_orig_normal.png"))
    arr_norm = np.array(im_norm)
    ys_n, xs_n = np.where(arr_norm[:, :, 3] > 10)
    crop_n = Image.fromarray(arr_norm[ys_n.min():ys_n.max()+1, xs_n.min():xs_n.max()+1])
    pw_n, ph_n = crop_n.size
    scale_n = target_h / float(ph_n)
    nw_n, nh_n = int(pw_n * scale_n), int(ph_n * scale_n)
    scaled_idle = crop_n.resize((nw_n, nh_n), resample=Image.BICUBIC)
    idle_frame = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    idle_frame.paste(scaled_idle, ((cw - nw_n) // 2, ground_y - nh_n), scaled_idle)

    # =========================================================================
    # 2. CRISP KNOCKING SPRITES (INTACT AUTHENTIC ARTWORK)
    # =========================================================================
    im_knock = Image.open(os.path.join(art_dirs[1], "crop_orig_knocking.png"))
    arr_knock = np.array(im_knock)
    ys_k, xs_k = np.where(arr_knock[:, :, 3] > 10)
    crop_k = Image.fromarray(arr_knock[ys_k.min():ys_k.max()+1, xs_k.min():xs_k.max()+1])

    pw_k, ph_k = crop_k.size
    scale_k = target_h / float(ph_k)
    nw_k, nh_k = int(pw_k * scale_k), int(ph_k * scale_k)
    scaled_knock = crop_k.resize((nw_k, nh_k), resample=Image.BICUBIC)

    base_k_x = (cw - nw_k) // 2
    base_k_y = ground_y - nh_k

    # Knock 1: Raise fist (poise)
    k1 = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    k1.paste(scaled_knock, (base_k_x, base_k_y), scaled_knock)

    # Knock 2: Impact on door (shifted forward -8px towards door)
    k2 = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    k2.paste(scaled_knock, (base_k_x - 8, base_k_y), scaled_knock)

    # Knock 3: Retract between knocks (shifted +4px back)
    k3 = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    k3.paste(scaled_knock, (base_k_x + 4, base_k_y), scaled_knock)

    # Knock 4: Return hand down (idle)
    k4 = idle_frame.copy()

    # =========================================================================
    # 3. SAVE ALL SPRITES TO PROJECT DIRECTORIES
    # =========================================================================
    art_dirs = [
        r"D:\engsnap\MastersActivityUnit6\Assets\SeniorsActivityUnit8\Art",
        r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art"
    ]

    for ad in art_dirs:
        os.makedirs(ad, exist_ok=True)

        # 4 walking frames (and 5-8 mirrored/repeated for 8-frame loop compatibility)
        walk_frames[0].save(os.path.join(ad, "SPR_Anu_Walk_1.png"))
        walk_frames[1].save(os.path.join(ad, "SPR_Anu_Walk_2.png"))
        walk_frames[2].save(os.path.join(ad, "SPR_Anu_Walk_3.png"))
        walk_frames[3].save(os.path.join(ad, "SPR_Anu_Walk_4.png"))
        walk_frames[0].save(os.path.join(ad, "SPR_Anu_Walk_5.png"))
        walk_frames[1].save(os.path.join(ad, "SPR_Anu_Walk_6.png"))
        walk_frames[2].save(os.path.join(ad, "SPR_Anu_Walk_7.png"))
        walk_frames[3].save(os.path.join(ad, "SPR_Anu_Walk_8.png"))

        # Idle
        idle_frame.save(os.path.join(ad, "SPR_Anu_Walk_Idle.png"))

        # Knocking
        idle_frame.save(os.path.join(ad, "SPR_Anu_Knock_0.png"))
        k1.save(os.path.join(ad, "SPR_Anu_Knock_1.png"))
        k2.save(os.path.join(ad, "SPR_Anu_Knock_2.png"))
        k3.save(os.path.join(ad, "SPR_Anu_Knock_3.png"))
        k4.save(os.path.join(ad, "SPR_Anu_Knock_4.png"))
        k1.save(os.path.join(ad, "SPR_Anu_Knocking.png"))

    # Save to scratch
    walk_frames[0].save(os.path.join(scratch, "SPR_Anu_Walk_1.png"))
    walk_frames[1].save(os.path.join(scratch, "SPR_Anu_Walk_2.png"))
    walk_frames[2].save(os.path.join(scratch, "SPR_Anu_Walk_3.png"))
    walk_frames[3].save(os.path.join(scratch, "SPR_Anu_Walk_4.png"))
    idle_frame.save(os.path.join(scratch, "SPR_Anu_Walk_Idle.png"))
    idle_frame.save(os.path.join(scratch, "SPR_Anu_Knock_0.png"))
    k1.save(os.path.join(scratch, "SPR_Anu_Knock_1.png"))
    k2.save(os.path.join(scratch, "SPR_Anu_Knock_2.png"))
    k3.save(os.path.join(scratch, "SPR_Anu_Knock_3.png"))
    k4.save(os.path.join(scratch, "SPR_Anu_Knock_4.png"))
    k1.save(os.path.join(scratch, "SPR_Anu_Knocking.png"))

    # Animated preview GIF of the entire sequence: WALK -> STOP -> STAND -> KNOCK 1, 2, 3 -> STAND
    full_seq = [
        walk_frames[0], walk_frames[1], walk_frames[2], walk_frames[3],
        walk_frames[0], walk_frames[1], walk_frames[2], walk_frames[3],
        idle_frame, idle_frame,
        k1, k2, k3, k2, k3, k2, k3,
        idle_frame, idle_frame
    ]
    full_seq[0].save(
        os.path.join(scratch, "full_walk_and_knock_sequence.gif"),
        save_all=True,
        append_images=full_seq[1:],
        duration=130,
        loop=0
    )
    print("Generated all clean walking and knocking sprites successfully!")

if __name__ == "__main__":
    generate_clean_sprites_and_controller()
