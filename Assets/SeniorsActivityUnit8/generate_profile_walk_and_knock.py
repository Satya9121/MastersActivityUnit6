import os
from PIL import Image

def generate_profile_assets():
    art_dir = r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art"
    ref_path = os.path.join(art_dir, "anu_side_walk_ref.png")
    knock_src_path = os.path.join(art_dir, "crop_knocking.png")

    if not os.path.exists(ref_path) or not os.path.exists(knock_src_path):
        print("Required reference files not found!")
        return

    ref = Image.open(ref_path).convert("RGBA")
    knock_src = Image.open(knock_src_path).convert("RGBA")

    cw, ch = 600, 780
    ground_y = 765
    w, h = ref.size
    base_x = (cw - w) // 2
    base_y = ground_y - h

    # Upper body and legs from the authentic reference image
    upper = ref.crop((0, 0, w, 525))
    leg_f = ref.crop((50, 520, 220, h)) # near / front leg
    leg_b = ref.crop((260, 520, 460, h)) # far / back leg

    # 1. SPR_Anu_Walk_1 (Contact A - reference stride)
    f1 = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    f1.paste(leg_b, (base_x + 260, base_y + 520), leg_b)
    f1.paste(upper, (base_x, base_y), upper)
    f1.paste(leg_f, (base_x + 50, base_y + 520), leg_f)

    # 2. SPR_Anu_Walk_2 (Passing A - front leg plants, back leg swings forward)
    f2 = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    f2.paste(leg_b, (base_x + 190, base_y + 500), leg_b)
    f2.paste(upper, (base_x, base_y - 5), upper)
    f2.paste(leg_f, (base_x + 130, base_y + 515), leg_f)

    # 3. SPR_Anu_Walk_3 (Contact B - alternating stride! Back leg steps forward-left, front leg back-right)
    f3 = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    f3.paste(leg_b, (base_x + 70, base_y + 520), leg_b)
    f3.paste(upper, (base_x, base_y + 2), upper)
    f3.paste(leg_f, (base_x + 240, base_y + 520), leg_f)

    # 4. SPR_Anu_Walk_4 (Passing B - far leg plants, front leg swings forward)
    f4 = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    f4.paste(leg_f, (base_x + 190, base_y + 500), leg_f)
    f4.paste(upper, (base_x, base_y - 5), upper)
    f4.paste(leg_b, (base_x + 150, base_y + 515), leg_b)

    # 5. SPR_Anu_Walk_Idle (Standing facing left in profile towards the door)
    idle = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    idle.paste(leg_b, (base_x + 160, base_y + 520), leg_b)
    idle.paste(upper, (base_x, base_y), upper)
    idle.paste(leg_f, (base_x + 130, base_y + 520), leg_f)

    # Knocking frames:
    # Use upper body without lowered arm + knocking fist from crop_knocking
    # In knock_src (326, 760), crop knocking arm and scale to match height:
    scale_k = h / knock_src.height
    arm_crop = knock_src.crop((5, 230, 130, 460))
    arm_w = int(arm_crop.width * scale_k)
    arm_h = int(arm_crop.height * scale_k)
    arm_scaled = arm_crop.resize((arm_w, arm_h), Image.Resampling.LANCZOS)

    # Knock 1: Anticipation / Raised fist
    k1 = idle.copy()
    k1.paste(arm_scaled, (base_x + 20, base_y + 260), arm_scaled)

    # Knock 2: Impact / Extended fist striking the "SHUT" door
    k2 = idle.copy()
    k2.paste(arm_scaled, (base_x - 30, base_y + 260), arm_scaled)

    # Knock 3: Recoil
    k3 = idle.copy()
    k3.paste(arm_scaled, (base_x + 5, base_y + 262), arm_scaled)

    frames = {
        "SPR_Anu_Walk_1.png": f1,
        "SPR_Anu_Walk_2.png": f2,
        "SPR_Anu_Walk_3.png": f3,
        "SPR_Anu_Walk_4.png": f4,
        "SPR_Anu_Walk_Idle.png": idle,
        "SPR_Anu_Knock_1.png": k1,
        "SPR_Anu_Knock_2.png": k2,
        "SPR_Anu_Knock_3.png": k3
    }

    target_dirs = [
        r"D:\engsnap\MastersActivityUnit6\Assets\SeniorsActivityUnit8\Art",
        r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art"
    ]

    for d in target_dirs:
        for fname, img in frames.items():
            p = os.path.join(d, fname)
            img.save(p, "PNG")
            print(f"Saved {fname} to {d}")

if __name__ == "__main__":
    generate_profile_assets()
