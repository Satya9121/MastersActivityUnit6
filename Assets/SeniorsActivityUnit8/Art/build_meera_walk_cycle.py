import os
import math
import numpy as np
from PIL import Image, ImageFilter

ART_DIR = os.path.dirname(os.path.abspath(__file__))
BASE_PATH = os.path.join(ART_DIR, "meera_left_base.png")

def extract_layers():
    base = Image.open(BASE_PATH).convert("RGBA")
    w, h = base.size
    arr = np.array(base)
    
    # Masks for different body regions in 640x800 coordinate space
    # 1. Head & Hair: y < 355
    head_mask = np.zeros((h, w), dtype=bool)
    head_mask[:355, :] = True
    head_arr = arr.copy()
    head_arr[~head_mask] = 0
    head_img = Image.fromarray(head_arr)
    
    # 2. Torso, Collar, Tie, Blouse & Skirt: y from 340 to 710, x from 200 to 470
    torso_mask = np.zeros((h, w), dtype=bool)
    # Exclude far arm (x < 245, y > 430) and near arm (x > 385, y > 430)
    for y in range(340, 715):
        for x in range(180, 480):
            # Exclude arm areas
            if y > 430 and x < 240:
                continue
            if y > 440 and x > 385:
                continue
            torso_mask[y, x] = True
            
    torso_arr = arr.copy()
    torso_arr[~torso_mask] = 0
    torso_img = Image.fromarray(torso_arr)
    
    # 3. Near Leg (Left Leg - Front Leg in base): y from 670 to 800, x from 120 to 320
    near_leg_mask = np.zeros((h, w), dtype=bool)
    near_leg_mask[670:800, 120:320] = True
    near_leg_arr = arr.copy()
    near_leg_arr[~near_leg_mask] = 0
    near_leg_img = Image.fromarray(near_leg_arr)
    
    # 4. Far Leg (Right Leg - Back Leg in base): y from 670 to 800, x from 320 to 520
    far_leg_mask = np.zeros((h, w), dtype=bool)
    far_leg_mask[670:800, 320:520] = True
    far_leg_arr = arr.copy()
    far_leg_arr[~far_leg_mask] = 0
    far_leg_img = Image.fromarray(far_leg_arr)
    
    # 5. Far Arm (Right Arm - Front/Forward Arm in base): y from 400 to 650, x from 130 to 260
    far_arm_mask = np.zeros((h, w), dtype=bool)
    far_arm_mask[400:650, 130:260] = True
    far_arm_arr = arr.copy()
    far_arm_arr[~far_arm_mask] = 0
    far_arm_img = Image.fromarray(far_arm_arr)
    
    # 6. Near Arm (Left Arm - Back Arm in base): y from 430 to 650, x from 370 to 490
    near_arm_mask = np.zeros((h, w), dtype=bool)
    near_arm_mask[430:650, 370:490] = True
    near_arm_arr = arr.copy()
    near_arm_arr[~near_arm_mask] = 0
    near_arm_img = Image.fromarray(near_arm_arr)
    
    return head_img, torso_img, near_leg_img, far_leg_img, far_arm_img, near_arm_img

def rotate_layer(img, angle_deg, pivot_x, pivot_y):
    if abs(angle_deg) < 0.01:
        return img
    return img.rotate(angle_deg, resample=Image.BICUBIC, center=(pivot_x, pivot_y))

def translate_layer(img, dx, dy):
    if dx == 0 and dy == 0:
        return img
    canvas = Image.new("RGBA", img.size, (0, 0, 0, 0))
    canvas.paste(img, (int(dx), int(dy)), img)
    return canvas

def compose_frame(head, torso, near_leg, far_leg, far_arm, near_arm,
                  far_arm_angle=0, near_arm_angle=0,
                  near_leg_angle=0, far_leg_angle=0,
                  bob_y=0, leg_shift_x=0):
    w, h = 640, 800
    canvas = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    
    # Joint Pivots in 640x800 coordinate space
    far_arm_pivot = (245, 435)
    near_arm_pivot = (395, 445)
    near_leg_pivot = (250, 680)
    far_leg_pivot = (370, 680)
    
    # 1. Rotate Limbs
    r_far_arm = rotate_layer(far_arm, far_arm_angle, far_arm_pivot[0], far_arm_pivot[1])
    r_near_arm = rotate_layer(near_arm, near_arm_angle, near_arm_pivot[0], near_arm_pivot[1])
    r_near_leg = rotate_layer(near_leg, near_leg_angle, near_leg_pivot[0], near_leg_pivot[1])
    r_far_leg = rotate_layer(far_leg, far_leg_angle, far_leg_pivot[0], far_leg_pivot[1])
    
    # Depth Ordering (from background to foreground):
    # 1. Far Arm (swings behind torso / on far side)
    canvas.alpha_composite(r_far_arm)
    
    # 2. Far Leg (right leg - with far-side shading)
    canvas.alpha_composite(r_far_leg)
    
    # 3. Torso & Skirt (with vertical bob)
    t_torso = translate_layer(torso, 0, bob_y)
    canvas.alpha_composite(t_torso)
    
    # 4. Near Leg (left leg - foreground leg)
    canvas.alpha_composite(r_near_leg)
    
    # 5. Near Arm (left arm - foreground arm)
    canvas.alpha_composite(r_near_arm)
    
    # 6. Head & Hair (with vertical bob)
    t_head = translate_layer(head, 0, bob_y)
    canvas.alpha_composite(t_head)
    
    return canvas

def generate_all_frames():
    head, torso, near_leg, far_leg, far_arm, near_arm = extract_layers()
    
    # Frame 1: Stride A (Left leg forward, Right leg back, Far arm forward, Near arm back)
    f1 = Image.open(BASE_PATH)
    f1.save(os.path.join(ART_DIR, "SPR_Meera_Walk_1.png"), "PNG")
    print("Frame 1 (Stride A) generated.")
    
    # Frame 2: Passing A (Left leg straight supporting weight, Right leg bent passing forward, Arms passing through neutral, Bob up -8px)
    f2 = compose_frame(head, torso, near_leg, far_leg, far_arm, near_arm,
                       far_arm_angle=-14, near_arm_angle=14,
                       near_leg_angle=-12, far_leg_angle=18,
                       bob_y=-8)
    f2.save(os.path.join(ART_DIR, "SPR_Meera_Walk_2.png"), "PNG")
    print("Frame 2 (Passing A) generated.")
    
    # Frame 3: Stride B (Right leg forward, Left leg back, Near arm forward, Far arm back, Bob 0px)
    # In Stride B:
    # Far leg rotates forward (+24 deg), Near leg rotates back (-24 deg)
    # Near arm rotates forward (+28 deg), Far arm rotates back (-28 deg)
    f3 = compose_frame(head, torso, near_leg, far_leg, far_arm, near_arm,
                       far_arm_angle=-28, near_arm_angle=28,
                       near_leg_angle=-24, far_leg_angle=24,
                       bob_y=0)
    f3.save(os.path.join(ART_DIR, "SPR_Meera_Walk_3.png"), "PNG")
    print("Frame 3 (Stride B) generated.")
    
    # Frame 4: Passing B (Right leg straight supporting weight, Left leg bent passing forward, Arms passing through neutral, Bob up -8px)
    f4 = compose_frame(head, torso, near_leg, far_leg, far_arm, near_arm,
                       far_arm_angle=14, near_arm_angle=-14,
                       near_leg_angle=18, far_leg_angle=-12,
                       bob_y=-8)
    f4.save(os.path.join(ART_DIR, "SPR_Meera_Walk_4.png"), "PNG")
    print("Frame 4 (Passing B) generated.")

if __name__ == "__main__":
    generate_all_frames()
