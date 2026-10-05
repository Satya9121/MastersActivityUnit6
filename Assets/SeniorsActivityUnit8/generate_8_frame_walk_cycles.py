import os
import math
from PIL import Image, ImageOps, ImageFilter

def build_8_frame_cycles():
    art_dirs = [
        r"D:\engsnap\MastersActivityUnit6\Assets\SeniorsActivityUnit8\Art",
        r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art"
    ]
    scratch_dir = r"C:\Users\ASUS\.gemini\antigravity-ide\brain\42cc47cf-a3d9-4f33-925f-a40c1262c46d\scratch"
    os.makedirs(scratch_dir, exist_ok=True)

    base_art = art_dirs[0]

    # =========================================================================
    # 1. ANU 8-FRAME WALK CYCLE
    # =========================================================================
    ref_anu_path = os.path.join(base_art, "anu_side_walk_ref.png")
    ref_anu = Image.open(ref_anu_path).convert("RGBA")
    
    cw, ch = 600, 780
    ground_y = 760
    aw, ah = ref_anu.size
    base_x = (cw - aw) // 2
    base_y = ground_y - ah

    # Extract Anu parts
    # Upper body (head, torso, arms)
    anu_head_torso = ref_anu.crop((0, 0, aw, 525))
    anu_leg_front = ref_anu.crop((50, 520, 220, ah))  # Front / Near leg
    anu_leg_back = ref_anu.crop((260, 520, 460, ah))  # Back / Far leg

    # Front arm & back arm for opposition swing
    # Back arm (darker / inside) & front arm (closer)
    arm_f = ref_anu.crop((120, 240, 270, 510))
    arm_b = ref_anu.crop((270, 240, 390, 500))
    torso_only = ref_anu.crop((0, 0, aw, 525))

    anu_frames = []
    
    # 8-Frame Motion Offsets for natural walking curve:
    # 1: Left Contact (Front leg forward, Back leg back)
    # 2: Down / Recoil (Weight shift down ~4px)
    # 3: Passing A (Back leg passing front leg, body centered)
    # 4: Up / High point (Front leg extends, back leg swings forward, body +3px)
    # 5: Right Contact (Back leg forward, Front leg back)
    # 6: Down / Recoil (Weight shift down ~4px)
    # 7: Passing B (Front leg passing back leg, body centered)
    # 8: Up / High point (Back leg extends, front leg swings forward, body +3px)

    configs_anu = [
        # Frame 1: Left Contact
        {"bf": (260, 520), "ff": (50, 520), "by": 0, "arm_swing": 0},
        # Frame 2: Down
        {"bf": (230, 515), "ff": (80, 518), "by": 4, "arm_swing": -4},
        # Frame 3: Passing A
        {"bf": (190, 500), "ff": (130, 515), "by": -1, "arm_swing": -10},
        # Frame 4: Up / Push
        {"bf": (140, 510), "ff": (180, 515), "by": -4, "arm_swing": -5},
        # Frame 5: Right Contact
        {"bf": (70, 520), "ff": (240, 520), "by": 0, "arm_swing": 0},
        # Frame 6: Down
        {"bf": (100, 518), "ff": (210, 515), "by": 4, "arm_swing": 4},
        # Frame 7: Passing B
        {"bf": (140, 515), "ff": (180, 500), "by": -1, "arm_swing": 10},
        # Frame 8: Up / Push
        {"bf": (200, 515), "ff": (120, 510), "by": -4, "arm_swing": 5},
    ]

    for i, cfg in enumerate(configs_anu):
        f = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
        by = base_y + cfg["by"]
        
        # Paste Back Leg
        f.paste(anu_leg_back, (base_x + cfg["bf"][0], by + cfg["bf"][1]), anu_leg_back)
        # Paste Upper Body
        f.paste(anu_head_torso, (base_x, by), anu_head_torso)
        # Paste Front Leg
        f.paste(anu_leg_front, (base_x + cfg["ff"][0], by + cfg["ff"][1]), anu_leg_front)

        anu_frames.append(f)

    # Save Anu frames to all art directories
    for idx, f in enumerate(anu_frames):
        fname = f"SPR_Anu_Walk_{idx+1}.png"
        for ad in art_dirs:
            if os.path.exists(ad):
                out_p = os.path.join(ad, fname)
                f.save(out_p)
        # Save copy to scratch
        f.save(os.path.join(scratch_dir, fname))

    # Save Anu GIF preview
    anu_frames[0].save(
        os.path.join(scratch_dir, "anu_8frame_walk.gif"),
        save_all=True,
        append_images=anu_frames[1:],
        duration=120,
        loop=0
    )
    print("Generated Anu 8-frame walk cycle successfully!")

    # =========================================================================
    # 2. MEERA 8-FRAME WALK CYCLE (Matching exactly Meera's authentic blue uniform)
    # =========================================================================
    p_head = Image.open(os.path.join(base_art, "meera_part_head.png")).convert("RGBA")
    p_torso = Image.open(os.path.join(base_art, "meera_part_torso.png")).convert("RGBA")
    p_skirt = Image.open(os.path.join(base_art, "meera_part_skirt.png")).convert("RGBA")
    p_arm_f = Image.open(os.path.join(base_art, "meera_part_arm_front.png")).convert("RGBA")
    p_arm_b = Image.open(os.path.join(base_art, "meera_part_arm_back.png")).convert("RGBA")
    p_leg_f = Image.open(os.path.join(base_art, "meera_part_leg_front.png")).convert("RGBA")
    p_leg_b = Image.open(os.path.join(base_art, "meera_part_leg_back.png")).convert("RGBA")

    mw, mh = 640, 800
    m_ground_y = 780

    configs_meera = [
        # Frame 1: Left Contact (Front leg forward, Back leg back)
        {"lf": (195, 630), "lb": (345, 630), "af": (230, 395), "ab": (325, 385), "by": 0, "rot_af": 12, "rot_ab": -14},
        # Frame 2: Down (Knees flex, body lowers 4px)
        {"lf": (220, 633), "lb": (315, 626), "af": (240, 398), "ab": (310, 389), "by": 4, "rot_af": 6, "rot_ab": -7},
        # Frame 3: Passing A (Back leg passing front leg)
        {"lf": (245, 630), "lb": (275, 615), "af": (250, 395), "ab": (290, 395), "by": -1, "rot_af": 0, "rot_ab": 0},
        # Frame 4: Up / Push-off (High point)
        {"lf": (270, 626), "lb": (235, 620), "af": (265, 390), "ab": (270, 400), "by": -4, "rot_af": -6, "rot_ab": 7},
        # Frame 5: Right Contact (Back leg forward, Front leg back)
        {"lf": (330, 630), "lb": (210, 630), "af": (285, 395), "ab": (250, 385), "by": 0, "rot_af": -12, "rot_ab": 14},
        # Frame 6: Down (Knees flex, body lowers 4px)
        {"lf": (305, 626), "lb": (235, 633), "af": (275, 398), "ab": (265, 389), "by": 4, "rot_af": -6, "rot_ab": 7},
        # Frame 7: Passing B (Front leg passing back leg)
        {"lf": (275, 615), "lb": (255, 630), "af": (250, 395), "ab": (290, 395), "by": -1, "rot_af": 0, "rot_ab": 0},
        # Frame 8: Up / Push-off (High point)
        {"lf": (240, 620), "lb": (280, 626), "af": (240, 390), "ab": (310, 400), "by": -4, "rot_af": 6, "rot_ab": -7},
    ]

    meera_frames = []

    for i, cfg in enumerate(configs_meera):
        f = Image.new("RGBA", (mw, mh), (0, 0, 0, 0))
        by = cfg["by"]

        # 1. Back Arm (Rotated)
        arm_b_rot = p_arm_b.rotate(cfg["rot_ab"], resample=Image.BICUBIC, expand=True)
        f.paste(arm_b_rot, (cfg["ab"][0], cfg["ab"][1] + by), arm_b_rot)

        # 2. Back Leg
        f.paste(p_leg_b, (cfg["lb"][0], cfg["lb"][1] + by), p_leg_b)

        # 3. Head & Torso & Skirt
        f.paste(p_head, (0, by), p_head)
        f.paste(p_torso, (200, 340 + by), p_torso)
        f.paste(p_skirt, (190, 460 + by), p_skirt)

        # 4. Front Leg
        f.paste(p_leg_f, (cfg["lf"][0], cfg["lf"][1] + by), p_leg_f)

        # 5. Front Arm (Rotated)
        arm_f_rot = p_arm_f.rotate(cfg["rot_af"], resample=Image.BICUBIC, expand=True)
        f.paste(arm_f_rot, (cfg["af"][0], cfg["af"][1] + by), arm_f_rot)

        meera_frames.append(f)

    # Save Meera frames to all art directories
    for idx, f in enumerate(meera_frames):
        fname = f"SPR_Meera_Walk_{idx+1}.png"
        for ad in art_dirs:
            if os.path.exists(ad):
                out_p = os.path.join(ad, fname)
                f.save(out_p)
        # Save copy to scratch
        f.save(os.path.join(scratch_dir, fname))

    # Save Meera GIF preview
    meera_frames[0].save(
        os.path.join(scratch_dir, "meera_8frame_walk.gif"),
        save_all=True,
        append_images=meera_frames[1:],
        duration=120,
        loop=0
    )
    print("Generated Meera 8-frame walk cycle successfully!")

if __name__ == "__main__":
    build_8_frame_cycles()
