import os
from PIL import Image

def generate_left_view():
    src_dir = r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art"
    sheep_path = os.path.join(src_dir, "crop_sheepish.png")
    knock_path = os.path.join(src_dir, "crop_knocking.png")

    if not os.path.exists(sheep_path) or not os.path.exists(knock_path):
        print("Required crops not found!")
        return

    sheep = Image.open(sheep_path).convert("RGBA")
    knock = Image.open(knock_path).convert("RGBA")

    # Unified canvas
    cw, ch = 340, 780
    ground_y = 768

    # Scale sheepish to height 754 (matching Anu's standard height)
    target_h = 754
    target_w = int(sheep.width * (target_h / sheep.height))
    sheep_scaled = sheep.resize((target_w, target_h), Image.Resampling.LANCZOS)

    # Extract smiling 3/4 left face from knock (326, 760)
    knock_face = knock.crop((110, 80, 205, 185))
    scale_factor = target_h / knock.height
    face_w = int(knock_face.width * scale_factor)
    face_h = int(knock_face.height * scale_factor)
    knock_face_scaled = knock_face.resize((face_w, face_h), Image.Resampling.LANCZOS)

    # Build base 3/4 left-facing Anu
    base_left = sheep_scaled.copy()
    base_left.paste(knock_face_scaled, (116, 80), knock_face_scaled)

    # Base position centered in canvas
    base_x = (cw - base_left.width) // 2
    base_y = ground_y - base_left.height

    # 1. Idle Frame (Standing facing left towards door)
    idle_frame = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    idle_frame.paste(base_left, (base_x, base_y), base_left)

    # Separate upper body and legs of left-facing Anu
    # Upper body: y 0..530
    # Left / front leg (screen-left): x 55..155, y 525..754
    # Right / back leg (screen-right): x 150..245, y 525..754
    bw, bh = base_left.size
    upper_body = base_left.crop((0, 0, bw, 530))
    leg_front = base_left.crop((55, 525, 155, bh))
    leg_back = base_left.crop((150, 525, 245, bh))

    front_x_base = 55
    back_x_base = 150
    leg_y_base = 525

    def make_left_walk_frame(front_dx, front_dy, back_dx, back_dy, body_dy):
        frame = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))

        fx = base_x + front_x_base + front_dx
        fy = base_y + leg_y_base + front_dy
        bx = base_x + back_x_base + back_dx
        by = base_y + leg_y_base + back_dy

        # Composite: back leg first, then body, then front leg
        frame.paste(leg_back, (bx, by), leg_back)
        frame.paste(upper_body, (base_x, base_y + body_dy), upper_body)
        frame.paste(leg_front, (fx, fy), leg_front)

        return frame

    # Walking animation from the left view with alternating footsteps:
    # Frame 1: Contact A (Front/left leg steps forward-left, Back/right leg pushes back-right)
    walk_1 = make_left_walk_frame(front_dx=-16, front_dy=-2, back_dx=+16, back_dy=-4, body_dy=+2)

    # Frame 2: Passing A (Weight on front leg, Back leg lifting & swinging forward under dress)
    walk_2 = make_left_walk_frame(front_dx=-2, front_dy=-4, back_dx=+2, back_dy=-16, body_dy=-3)

    # Frame 3: Contact B (Alternating step! Back/right leg steps forward-left, Front/left leg pushes back-right)
    walk_3 = make_left_walk_frame(front_dx=+16, front_dy=-4, back_dx=-16, back_dy=-2, body_dy=+2)

    # Frame 4: Passing B (Weight on back leg, Front leg lifting & swinging forward under dress)
    walk_4 = make_left_walk_frame(front_dx=+2, front_dy=-16, back_dx=-2, back_dy=-4, body_dy=-3)

    # Knocking frames from authentic crop_knocking.png (326, 760)
    # Scaled to canvas ground_y = 768
    knock_x = (cw - knock.width) // 2
    knock_y = ground_y - knock.height

    # Frame 1: Anticipation / Fist drawn back slightly before knock
    knock_1 = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    knock_1.paste(knock, (knock_x + 8, knock_y - 2), knock)

    # Frame 2: Impact / Fist extended forward contacting the door surface of the SHUT door
    knock_2 = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    knock_2.paste(knock, (knock_x - 10, knock_y), knock)

    # Frame 3: Recoil / Fist rebounds slightly
    knock_3 = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    knock_3.paste(knock, (knock_x + 4, knock_y + 1), knock)

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
            print(f"Saved left-view {fname} to {d}")

if __name__ == "__main__":
    generate_left_view()
