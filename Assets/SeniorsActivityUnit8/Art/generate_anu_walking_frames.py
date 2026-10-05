import os
import math
from PIL import Image, ImageDraw

ART_DIR = os.path.dirname(os.path.abspath(__file__))

def create_base_canvas():
    # 4x supersampling for ultra smooth vector-like rendering: 1440 x 2400 -> downscaled to 360 x 600
    return Image.new("RGBA", (1440, 2400), (0, 0, 0, 0))

def draw_anu_frame(scale=4, bob=0, l_leg_pos=(-60, 0), r_leg_pos=(70, -12), 
                   l_arm_pos=(60, 0), r_arm_pos=(-60, -10), 
                   facing="left", pose="walk", knock_stage=0):
    """
    scale: 4 (supersampling)
    bob: vertical offset of pelvis/torso/head (in standard pixels)
    l_leg_pos: (foot_x_offset, foot_y_lift)
    r_leg_pos: (foot_x_offset, foot_y_lift)
    """
    S = scale
    img = create_base_canvas()
    draw = ImageDraw.Draw(img)

    # Base coordinates in 360x600 space
    cx = 180
    base_ground_y = 575
    
    # Head & Pelvis center with vertical bob
    head_cy = 140 + bob
    pelvis_y = 480 + bob
    neck_y = 220 + bob

    # Colors
    c_hair = (41, 37, 36, 255)         # #292524 dark espresso
    c_skin = (254, 215, 170, 255)      # #FED7AA warm peach
    c_skin_shadow = (245, 190, 140, 255) # depth shadow for far limbs
    c_dress_top = (244, 63, 94, 255)   # #F43F5E vibrant rose red
    c_dress_bot = (190, 18, 60, 255)   # #BE123C deep ruby
    c_dress_stroke = (136, 19, 55, 255)# #881337 dark ruby outline
    c_shoe = (28, 25, 23, 255)         # #1C1917 dark espresso / black
    c_sock = (255, 255, 255, 255)      # white socks
    c_ribbon = (225, 29, 72, 255)      # #E11D48 ribbon
    c_cheek = (251, 113, 133, 160)     # #FB7185 soft blush
    c_collar = (255, 255, 255, 255)    # Peter Pan collar

    # 1. FAR ARM (Right arm when facing left, or behind body)
    if pose == "knock":
        # In knock pose, left arm rests comfortably, right arm knocks
        # Far arm is idle
        pass
    else:
        # Swing far arm
        r_shoulder = (cx + 20, neck_y + 30)
        r_hand = (cx + r_arm_pos[0], neck_y + 130 + r_arm_pos[1])
        draw.line([r_shoulder[0]*S, r_shoulder[1]*S, r_hand[0]*S, r_hand[1]*S], 
                  fill=c_skin_shadow, width=16*S)
        draw.ellipse([(r_hand[0]-11)*S, (r_hand[1]-11)*S, (r_hand[0]+11)*S, (r_hand[1]+11)*S], 
                     fill=c_skin_shadow)

    # 2. FAR LEG (Right leg)
    r_hip = (cx + 18, pelvis_y)
    r_foot_x = cx + r_leg_pos[0]
    r_foot_y = base_ground_y + r_leg_pos[1]
    r_knee_x = (r_hip[0] + r_foot_x) / 2 + (8 if r_leg_pos[1] < -10 else 0)
    r_knee_y = (r_hip[1] + r_foot_y) / 2
    # Draw leg upper & lower
    draw.line([r_hip[0]*S, r_hip[1]*S, r_knee_x*S, r_knee_y*S, r_foot_x*S, (r_foot_y-10)*S], 
              fill=c_skin_shadow, width=16*S, joint="curve")
    # Far Sock & Shoe
    draw.rounded_rectangle([(r_foot_x-10)*S, (r_foot_y-16)*S, (r_foot_x+10)*S, (r_foot_y-8)*S], 
                           radius=3*S, fill=c_sock)
    draw.ellipse([(r_foot_x-22)*S, (r_foot_y-12)*S, (r_foot_x+16)*S, (r_foot_y+6)*S], 
                 fill=c_shoe)

    # 3. FAR PIGTAIL (Right bun)
    draw.ellipse([(cx+45)*S, (head_cy-25)*S, (cx+85)*S, (head_cy+25)*S], fill=c_hair)
    draw.ellipse([(cx+45)*S, (head_cy-5)*S, (cx+60)*S, (head_cy+15)*S], fill=c_ribbon)

    # 4. HEAD BASE (Hair & Skull)
    draw.ellipse([(cx-70)*S, (head_cy-80)*S, (cx+70)*S, (head_cy+80)*S], fill=c_hair)

    # 5. FACE (Oriented left)
    # Profile / 3-quarter facing left
    face_cx = cx - 18
    face_cy = head_cy + 8
    draw.ellipse([(face_cx-55)*S, (face_cy-58)*S, (face_cx+55)*S, (face_cy+58)*S], fill=c_skin)
    
    # Hair bangs & front hair
    draw.polygon([(cx-70)*S, (head_cy-40)*S, 
                  (cx-30)*S, (head_cy-20)*S, 
                  (cx+10)*S, (head_cy-45)*S, 
                  (cx+50)*S, (head_cy-25)*S, 
                  (cx+70)*S, (head_cy-50)*S,
                  (cx+70)*S, (head_cy-80)*S,
                  (cx-70)*S, (head_cy-80)*S], fill=c_hair)
    # Sideburn / cheek frame
    draw.ellipse([(cx+30)*S, (head_cy-10)*S, (cx+60)*S, (head_cy+40)*S], fill=c_hair)

    # 6. FACIAL FEATURES (Left facing)
    # Eye (Profile: Left eye prominent, looking left)
    eye_x = face_cx - 20
    eye_y = face_cy - 4
    draw.ellipse([(eye_x-8)*S, (eye_y-14)*S, (eye_x+8)*S, (eye_y+14)*S], fill=(28, 25, 23, 255))
    # Eye shine
    draw.ellipse([(eye_x-5)*S, (eye_y-10)*S, (eye_x+1)*S, (eye_y-4)*S], fill=(255, 255, 255, 255))
    draw.ellipse([(eye_x+1)*S, (eye_y+2)*S, (eye_x+5)*S, (eye_y+6)*S], fill=(255, 255, 255, 200))
    # Eyebrow
    draw.arc([(eye_x-12)*S, (eye_y-26)*S, (eye_x+12)*S, (eye_y-14)*S], start=190, end=350, fill=c_hair, width=3*S)

    # Soft rosy cheek
    draw.ellipse([(eye_x-8)*S, (eye_y+18)*S, (eye_x+22)*S, (eye_y+36)*S], fill=c_cheek)

    # Cute nose
    nose_x = face_cx - 45
    nose_y = face_cy + 10
    draw.arc([(nose_x)*S, (nose_y-5)*S, (nose_x+10)*S, (nose_y+7)*S], start=90, end=270, fill=(210, 140, 100, 255), width=2*S)

    # Cheerful Smile
    smile_x = face_cx - 30
    smile_y = face_cy + 26
    draw.arc([(smile_x-8)*S, (smile_y-6)*S, (smile_x+18)*S, (smile_y+14)*S], start=10, end=170, fill=(180, 50, 50, 255), width=3*S)

    # 7. NEAR PIGTAIL (Left bun, facing viewer)
    draw.ellipse([(cx-88)*S, (head_cy-22)*S, (cx-44)*S, (head_cy+30)*S], fill=c_hair)
    draw.ellipse([(cx-58)*S, (head_cy-5)*S, (cx-42)*S, (head_cy+15)*S], fill=c_ribbon)

    # 8. TORSO & DRESS (A-Line Red/Rose Dress)
    # Dress gradient simulation with layers
    dress_pts = [
        ((cx-35)*S, (neck_y+25)*S),
        ((cx+35)*S, (neck_y+25)*S),
        ((cx+85)*S, (pelvis_y+5)*S),
        ((cx-85)*S, (pelvis_y+5)*S)
    ]
    draw.polygon(dress_pts, fill=c_dress_bot)
    # Upper dress highlight
    draw.polygon([((cx-35)*S, (neck_y+25)*S),
                  ((cx+35)*S, (neck_y+25)*S),
                  ((cx+65)*S, (pelvis_y-80)*S),
                  ((cx-65)*S, (pelvis_y-80)*S)], fill=c_dress_top)
    # Dress outline
    draw.line(dress_pts + [dress_pts[0]], fill=c_dress_stroke, width=4*S)

    # Peter Pan white collar
    draw.ellipse([(cx-36)*S, (neck_y+16)*S, (cx+2)*S, (neck_y+38)*S], fill=c_collar)
    draw.ellipse([(cx-2)*S, (neck_y+16)*S, (cx+36)*S, (neck_y+38)*S], fill=c_collar)
    draw.ellipse([(cx-6)*S, (neck_y+24)*S, (cx+4)*S, (neck_y+34)*S], fill=c_dress_stroke)

    # 9. NEAR LEG (Left leg - in front)
    l_hip = (cx - 18, pelvis_y)
    l_foot_x = cx + l_leg_pos[0]
    l_foot_y = base_ground_y + l_leg_pos[1]
    l_knee_x = (l_hip[0] + l_foot_x) / 2 + (-8 if l_leg_pos[1] < -10 else 0)
    l_knee_y = (l_hip[1] + l_foot_y) / 2
    # Draw near leg
    draw.line([l_hip[0]*S, l_hip[1]*S, l_knee_x*S, l_knee_y*S, l_foot_x*S, (l_foot_y-10)*S], 
              fill=c_skin, width=17*S, joint="curve")
    # Near Sock & Shoe
    draw.rounded_rectangle([(l_foot_x-11)*S, (l_foot_y-17)*S, (l_foot_x+11)*S, (l_foot_y-8)*S], 
                           radius=3*S, fill=c_sock)
    draw.ellipse([(l_foot_x-24)*S, (l_foot_y-13)*S, (l_foot_x+18)*S, (l_foot_y+7)*S], 
                 fill=c_shoe)
    # Shoe buckle / shine
    draw.ellipse([(l_foot_x-14)*S, (l_foot_y-10)*S, (l_foot_x-6)*S, (l_foot_y-4)*S], fill=(100, 100, 100, 255))

    # 10. NEAR ARM / KNOCKING ARM
    if pose == "knock":
        # Knocking gesture facing left towards the door!
        shoulder = (cx - 20, neck_y + 35)
        if knock_stage == 1:
            # Raised elbow, fist ready near door
            elbow = (cx - 70, neck_y + 65)
            fist = (cx - 110, neck_y + 35)
            draw.line([shoulder[0]*S, shoulder[1]*S, elbow[0]*S, elbow[1]*S, fist[0]*S, fist[1]*S], 
                      fill=c_skin, width=17*S, joint="curve")
            draw.ellipse([(fist[0]-14)*S, (fist[1]-14)*S, (fist[0]+14)*S, (fist[1]+14)*S], fill=c_skin)
        elif knock_stage == 2:
            # Arm extended forward, fist rapping on door with knock impact stars!
            elbow = (cx - 75, neck_y + 60)
            fist = (cx - 135, neck_y + 45)
            draw.line([shoulder[0]*S, shoulder[1]*S, elbow[0]*S, elbow[1]*S, fist[0]*S, fist[1]*S], 
                      fill=c_skin, width=17*S, joint="curve")
            draw.ellipse([(fist[0]-14)*S, (fist[1]-14)*S, (fist[0]+14)*S, (fist[1]+14)*S], fill=c_skin)
            # Knock impact arcs / stars
            impact_x = fist[0] - 12
            impact_y = fist[1]
            draw.arc([(impact_x-18)*S, (impact_y-25)*S, (impact_x+12)*S, (impact_y+25)*S], 
                     start=110, end=250, fill=(250, 204, 21, 255), width=4*S)
            draw.arc([(impact_x-28)*S, (impact_y-38)*S, (impact_x+18)*S, (impact_y+38)*S], 
                     start=115, end=245, fill=(250, 204, 21, 200), width=3*S)
        elif knock_stage == 3:
            # Recoil / rebound
            elbow = (cx - 65, neck_y + 68)
            fist = (cx - 100, neck_y + 48)
            draw.line([shoulder[0]*S, shoulder[1]*S, elbow[0]*S, elbow[1]*S, fist[0]*S, fist[1]*S], 
                      fill=c_skin, width=17*S, joint="curve")
            draw.ellipse([(fist[0]-14)*S, (fist[1]-14)*S, (fist[0]+14)*S, (fist[1]+14)*S], fill=c_skin)
    else:
        # Normal walking arm swing (Near arm)
        l_shoulder = (cx - 20, neck_y + 35)
        l_hand = (cx + l_arm_pos[0], neck_y + 130 + l_arm_pos[1])
        draw.line([l_shoulder[0]*S, l_shoulder[1]*S, l_hand[0]*S, l_hand[1]*S], 
                  fill=c_skin, width=17*S)
        draw.ellipse([(l_hand[0]-12)*S, (l_hand[1]-12)*S, (l_hand[0]+12)*S, (l_hand[1]+12)*S], 
                     fill=c_skin)

    # Downscale with high quality Lanczos anti-aliasing
    final_img = img.resize((360, 600), Image.Resampling.LANCZOS)
    return final_img

def main():
    print(f"Generating Anu walking and knocking animation sprites in: {ART_DIR}")

    # 1. WALK CYCLE FRAMES (4 key steps creating smooth alternating strides)
    # Frame 1: Stride A - Left leg forward, Right leg back
    f1 = draw_anu_frame(bob=2, l_leg_pos=(-65, 0), r_leg_pos=(65, -8), 
                        l_arm_pos=(55, -15), r_arm_pos=(-55, 10))
    f1.save(os.path.join(ART_DIR, "SPR_Anu_Walk_1.png"), "PNG")
    print("  -> Generated SPR_Anu_Walk_1.png (Stride A: Left Foot Forward)")

    # Frame 2: Passing A - Left leg straight (support), Right leg swinging forward past left
    f2 = draw_anu_frame(bob=-6, l_leg_pos=(-5, 0), r_leg_pos=(20, -32), 
                        l_arm_pos=(10, 0), r_arm_pos=(-10, 0))
    f2.save(os.path.join(ART_DIR, "SPR_Anu_Walk_2.png"), "PNG")
    print("  -> Generated SPR_Anu_Walk_2.png (Passing A: Weight on Left, Right Crossing)")

    # Frame 3: Stride B - Right leg forward, Left leg back (Opposite stride!)
    f3 = draw_anu_frame(bob=2, l_leg_pos=(65, -8), r_leg_pos=(-65, 0), 
                        l_arm_pos=(-55, 10), r_arm_pos=(55, -15))
    f3.save(os.path.join(ART_DIR, "SPR_Anu_Walk_3.png"), "PNG")
    print("  -> Generated SPR_Anu_Walk_3.png (Stride B: Right Foot Forward - Alternating!)")

    # Frame 4: Passing B - Right leg straight (support), Left leg swinging forward past right
    f4 = draw_anu_frame(bob=-6, l_leg_pos=(20, -32), r_leg_pos=(-5, 0), 
                        l_arm_pos=(-10, 0), r_arm_pos=(10, 0))
    f4.save(os.path.join(ART_DIR, "SPR_Anu_Walk_4.png"), "PNG")
    print("  -> Generated SPR_Anu_Walk_4.png (Passing B: Weight on Right, Left Crossing)")

    # 2. IDLE FRAME (Natural standing pose facing left)
    f_idle = draw_anu_frame(bob=0, l_leg_pos=(-18, 0), r_leg_pos=(22, 0), 
                            l_arm_pos=(0, 0), r_arm_pos=(0, 0), pose="idle")
    f_idle.save(os.path.join(ART_DIR, "SPR_Anu_Walk_Idle.png"), "PNG")
    print("  -> Generated SPR_Anu_Walk_Idle.png (Standing Idle facing left)")

    # 3. KNOCKING FRAMES (Reaching door and rapping on door)
    # Knock 1: Poised / Raising fist
    k1 = draw_anu_frame(bob=0, l_leg_pos=(-18, 0), r_leg_pos=(22, 0), pose="knock", knock_stage=1)
    k1.save(os.path.join(ART_DIR, "SPR_Anu_Knock_1.png"), "PNG")
    print("  -> Generated SPR_Anu_Knock_1.png (Raising fist)")

    # Knock 2: Knock contact / Impact
    k2 = draw_anu_frame(bob=0, l_leg_pos=(-18, 0), r_leg_pos=(22, 0), pose="knock", knock_stage=2)
    k2.save(os.path.join(ART_DIR, "SPR_Anu_Knock_2.png"), "PNG")
    print("  -> Generated SPR_Anu_Knock_2.png (Knock contact & impact)")

    # Knock 3: Rebound between knocks
    k3 = draw_anu_frame(bob=0, l_leg_pos=(-18, 0), r_leg_pos=(22, 0), pose="knock", knock_stage=3)
    k3.save(os.path.join(ART_DIR, "SPR_Anu_Knock_3.png"), "PNG")
    print("  -> Generated SPR_Anu_Knock_3.png (Knock rebound)")

if __name__ == "__main__":
    main()
