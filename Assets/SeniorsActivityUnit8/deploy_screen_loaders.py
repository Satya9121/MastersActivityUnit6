import os

# Update U8_Part1_DoorController_Masters_Activity.cs
p1_targets = [
    r"D:\engsnap\MastersActivityUnit6\Assets\SeniorsActivityUnit8\Scripts\Screens\U8_Part1_DoorController_Masters_Activity.cs",
    r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Scripts\Screens\U8_Part1_DoorController_Masters_Activity.cs"
]

for p in p1_targets:
    if os.path.exists(p):
        with open(p, "r", encoding="utf-8") as f:
            content = f.read()

        # Update meeraWalkSprites array size to 8
        content = content.replace("[SerializeField] private Sprite[] meeraWalkSprites = new Sprite[4];",
                                  "[SerializeField] private Sprite[] meeraWalkSprites = new Sprite[8];")

        # Update EnsureMeeraSpritesLoaded to load 8 frames
        old_loader = """        public void EnsureMeeraSpritesLoaded()
        {
#if UNITY_EDITOR
            if (meeraWalkSprites == null || meeraWalkSprites.Length < 4 || meeraWalkSprites[0] == null)
            {
                meeraWalkSprites = new Sprite[4];
                meeraWalkSprites[0] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Meera_Walk_1.png");
                meeraWalkSprites[1] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Meera_Walk_2.png");
                meeraWalkSprites[2] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Meera_Walk_3.png");
                meeraWalkSprites[3] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Meera_Walk_4.png");
            }"""

        new_loader = """        public void EnsureMeeraSpritesLoaded()
        {
#if UNITY_EDITOR
            if (meeraWalkSprites == null || meeraWalkSprites.Length < 8 || meeraWalkSprites[0] == null)
            {
                meeraWalkSprites = new Sprite[8];
                for (int i = 0; i < 8; i++)
                {
                    meeraWalkSprites[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/SeniorsActivityUnit8/Art/SPR_Meera_Walk_{i + 1}.png");
                }
            }"""

        if old_loader in content:
            content = content.replace(old_loader, new_loader)
        else:
            # General replacement for EnsureMeeraSpritesLoaded block
            import re
            content = re.sub(
                r'public void EnsureMeeraSpritesLoaded\(\)[\s\S]*?meeraWalkSprites\[3\] = .*?;\s*\}',
                new_loader.strip(),
                content
            )

        with open(p, "w", encoding="utf-8") as f:
            f.write(content)
        print("Updated Part 1 in:", p)


# Update U8_Part2_WashroomInsideController_Masters_Activity.cs
p2_targets = [
    r"D:\engsnap\MastersActivityUnit6\Assets\SeniorsActivityUnit8\Scripts\Screens\U8_Part2_WashroomInsideController_Masters_Activity.cs",
    r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Scripts\Screens\U8_Part2_WashroomInsideController_Masters_Activity.cs"
]

for p in p2_targets:
    if os.path.exists(p):
        with open(p, "r", encoding="utf-8") as f:
            content = f.read()

        content = content.replace("[SerializeField] private Sprite[] anuWalkSprites = new Sprite[4];",
                                  "[SerializeField] private Sprite[] anuWalkSprites = new Sprite[8];")

        old_anu_loader = """        public void EnsureAnuWalkSpritesLoaded()
        {
#if UNITY_EDITOR
            if (anuWalkSprites == null || anuWalkSprites.Length < 4 || anuWalkSprites[0] == null)
            {
                anuWalkSprites = new Sprite[4];
                anuWalkSprites[0] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Walk_1.png");
                anuWalkSprites[1] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Meera_Walk_2.png"); // fallback safety
                anuWalkSprites[1] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Walk_2.png");
                anuWalkSprites[2] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Walk_3.png");
                anuWalkSprites[3] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SeniorsActivityUnit8/Art/SPR_Anu_Walk_4.png");
            }
#endif
        }"""

        new_anu_loader = """        public void EnsureAnuWalkSpritesLoaded()
        {
#if UNITY_EDITOR
            if (anuWalkSprites == null || anuWalkSprites.Length < 8 || anuWalkSprites[0] == null)
            {
                anuWalkSprites = new Sprite[8];
                for (int i = 0; i < 8; i++)
                {
                    anuWalkSprites[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/SeniorsActivityUnit8/Art/SPR_Anu_Walk_{i + 1}.png");
                }
            }
#endif
        }"""

        if old_anu_loader in content:
            content = content.replace(old_anu_loader, new_anu_loader)
        else:
            import re
            content = re.sub(
                r'public void EnsureAnuWalkSpritesLoaded\(\)[\s\S]*?#endif\s*\}',
                new_anu_loader.strip(),
                content
            )

        with open(p, "w", encoding="utf-8") as f:
            f.write(content)
        print("Updated Part 2 in:", p)
