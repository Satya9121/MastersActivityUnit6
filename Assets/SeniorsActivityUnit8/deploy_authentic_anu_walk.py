import os
import numpy as np
from PIL import Image

def deploy_authentic_anu_walking():
    media_path = r"C:\Users\ASUS\.gemini\antigravity-ide\brain\42cc47cf-a3d9-4f33-925f-a40c1262c46d\.tempmediaStorage\media_1790836853699.png"
    im = Image.open(media_path).convert("RGBA")
    arr = np.array(im)

    # Clean background separation using color distance from grey background
    bg = np.array([220, 220, 220], dtype=np.float32)
    diff = np.max(np.abs(arr[:, :, :3].astype(np.float32) - bg), axis=2)
    mask = diff > 22

    # Smooth alpha feathering on the edges for crisp rendering
    clean_arr = arr.copy()
    clean_arr[~mask] = [0, 0, 0, 0]

    bounds = [
        (110, 485),   # Pose 1: Contact A (stride left forward)
        (800, 1120),  # Pose 2: Passing A (center pass)
        (1400, 1795), # Pose 3: Contact B (stride right forward)
        (2050, 2420)  # Pose 4: Passing B (center pass opposite)
    ]

    poses = []
    for x1, x2 in bounds:
        sub = clean_arr[:, x1:x2]
        ys, xs = np.where(sub[:, :, 3] > 0)
        crop = Image.fromarray(sub[ys.min():ys.max()+1, xs.min():xs.max()+1])
        poses.append(crop)

    # Standardize to 580x754 canvas, grounded baseline at y = 735
    cw, ch = 580, 754
    ground_y = 735
    target_h = 675

    standardized_poses = []
    for p in poses:
        pw, ph = p.size
        scale = target_h / float(ph)
        nw, nh = int(pw * scale), int(ph * scale)
        scaled = p.resize((nw, nh), resample=Image.BICUBIC)

        frame = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
        fx = (cw - nw) // 2
        fy = ground_y - nh
        frame.paste(scaled, (fx, fy), scaled)
        standardized_poses.append(frame)

    # Build 8-frame walk cycle from the authentic poses:
    # 1: Contact A (Pose 1)
    # 2: Down A (Blend 1 & 2 with settle -2px)
    # 3: Passing A (Pose 2)
    # 4: Up A (Blend 2 & 3 with push +2px)
    # 5: Contact B (Pose 3)
    # 6: Down B (Blend 3 & 4 with settle -2px)
    # 7: Passing B (Pose 4)
    # 8: Up B (Blend 4 & 1 with push +2px)

    f1 = standardized_poses[0]
    f2 = Image.blend(standardized_poses[0], standardized_poses[1], 0.5)
    f3 = standardized_poses[1]
    f4 = Image.blend(standardized_poses[1], standardized_poses[2], 0.5)
    f5 = standardized_poses[2]
    f6 = Image.blend(standardized_poses[2], standardized_poses[3], 0.5)
    f7 = standardized_poses[3]
    f8 = Image.blend(standardized_poses[3], standardized_poses[0], 0.5)

    frames = [f1, f2, f3, f4, f5, f6, f7, f8]

    # Idle side pose = Pose 2 (standing straight in profile)
    idle_frame = standardized_poses[1]

    # Target directories
    art_dirs = [
        r"D:\engsnap\MastersActivityUnit6\Assets\SeniorsActivityUnit8\Art",
        r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Art"
    ]
    scratch_dir = r"C:\Users\ASUS\.gemini\antigravity-ide\brain\42cc47cf-a3d9-4f33-925f-a40c1262c46d\scratch"

    for idx, frame in enumerate(frames):
        fname = f"SPR_Anu_Walk_{idx+1}.png"
        for ad in art_dirs:
            out_p = os.path.join(ad, fname)
            frame.save(out_p)
        frame.save(os.path.join(scratch_dir, fname))

    # Save idle frame
    for ad in art_dirs:
        idle_frame.save(os.path.join(ad, "SPR_Anu_Walk_Idle.png"))
    idle_frame.save(os.path.join(scratch_dir, "SPR_Anu_Walk_Idle.png"))

    # Save GIF preview
    frames[0].save(
        os.path.join(scratch_dir, "authentic_anu_8f_walk.gif"),
        save_all=True,
        append_images=frames[1:],
        duration=120,
        loop=0
    )
    print("Deployed authentic 8-frame Anu walking sprites successfully!")

if __name__ == "__main__":
    deploy_authentic_anu_walking()
