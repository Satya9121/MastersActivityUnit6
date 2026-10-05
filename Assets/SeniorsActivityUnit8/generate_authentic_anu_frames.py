import os
from PIL import Image

def generate_frames():
    src_dir = r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art"
    normal_path = os.path.join(src_dir, "crop_normal.png")
    knock_path = os.path.join(src_dir, "crop_knock.png")

    if not os.path.exists(normal_path) or not os.path.exists(knock_path):
        print("Source crops not found!")
        return

    normal = Image.open(normal_path).convert("RGBA")
    knock = Image.open(knock_path).convert("RGBA")

    # Target unified canvas
    cw, ch = 340, 780
    ground_y = 768

    # Base normal centered
    base_x = (cw - normal.width) // 2
    base_y = ground_y - normal.height

    # 1. SPR_Anu_Walk_Idle
    idle_frame = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    idle_frame.paste(normal, (base_x, base_y), normal)

    # Decompose normal character into layers for natural walking motion
    # Head & pigtails: 0..240
    # Torso, dress, arms: 240..525
    # Upper skirt hem: 470..535
    # Left leg & shoe: x: 40..132, y: 525..754
    # Right leg & shoe: x: 132..225, y: 525..754
    nw, nh = normal.size
    upper_body = normal.crop((0, 0, nw, 525))
    left_leg = normal.crop((35, 520, 134, nh))
    right_leg = normal.crop((134, 520, 230, nh))

    left_x_in_normal = 35
    right_x_in_normal = 134
    leg_y_in_normal = 520

    # Function to composite a walk frame
    def make_walk_frame(left_dx, left_dy, right_dx, right_dy, body_dy, body_rot=0):
        frame = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
        
        # Position legs
        lx = base_x + left_x_in_normal + left_dx
        ly = base_y + leg_y_in_normal + left_dy
        rx = base_x + right_x_in_normal + right_dx
        ry = base_y + leg_y_in_normal + right_dy

        # Back leg first, then body, then front leg
        # When moving left (facing left towards door):
        # Left leg is screen-left (front), right leg is screen-right (back)
        frame.paste(right_leg, (rx, ry), right_leg)
        frame.paste(left_leg, (lx, ly), left_leg)

        # Body with slight vertical bob
        by = base_y + body_dy
        frame.paste(upper_body, (base_x, by), upper_body)

        return frame

    # Frame 1: Contact A (Left leg forward, Right leg back)
    # Stride forward towards the door
    walk_1 = make_walk_frame(left_dx=-14, left_dy=-2, right_dx=+14, right_dy=-4, body_dy=+2)

    # Frame 2: Passing A (Weight on left leg, Right leg lifting & passing forward)
    walk_2 = make_walk_frame(left_dx=-2, left_dy=-4, right_dx=+4, right_dy=-16, body_dy=-3)

    # Frame 3: Contact B (Alternating step! Right leg forward, Left leg back)
    walk_3 = make_walk_frame(left_dx=+14, left_dy=-4, right_dx=-14, right_dy=-2, body_dy=+2)

    # Frame 4: Passing B (Weight on right leg, Left leg lifting & passing forward)
    walk_4 = make_walk_frame(left_dx=+4, left_dy=-16, right_dx=-2, right_dy=-4, body_dy=-3)

    # Knocking frames from authentic crop_knock.png
    # knock is 326 x 760
    knock_base_x = (cw - knock.width) // 2
    knock_base_y = ground_y - knock.height

    # Frame 1: Anticipation / Fist pulled back slightly
    knock_1 = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    knock_1.paste(knock, (knock_base_x + 6, knock_base_y - 2), knock)

    # Frame 2: Impact / Fist extended firmly on the door
    knock_2 = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    knock_2.paste(knock, (knock_base_x - 8, knock_base_y), knock)

    # Frame 3: Recoil / Fist rebounds slightly
    knock_3 = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    knock_3.paste(knock, (knock_base_x + 3, knock_base_y + 1), knock)

    frames = {
        "SPR_Anu_Walk_Idle.png": idle_frame,
        "SPR_Anu_Walk_1.png": walk_1,
        "SPR_Anu_Walk_2.png": walk_2,
        "SPR_Anu_Walk_3.png": walk_3,
        "SPR_Anu_Walk_4.png": walk_4,
        "SPR_Anu_Knock_1.png": knock_1,
        "SPR_Anu_Knock_2.png": knock_2,
        "SPR_Anu_Knock_3.png": knock_3
    }

    target_dirs = [
        r"D:\engsnap\MastersActivityUnit6\Assets\SeniorsActivityUnit8\Art",
        r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art"
    ]

    for d in target_dirs:
        for fname, img in frames.items():
            out_path = os.path.join(d, fname)
            img.save(out_path, "PNG")
            print(f"Saved {fname} to {d}")

if __name__ == "__main__":
    generate_frames()
