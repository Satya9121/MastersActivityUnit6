import os

# Update doorPosition to -424f in both controller files
files = [
    r"D:\engsnap\MastersActivityUnit6\Assets\SeniorsActivityUnit8\Scripts\Screens\U8_AnuWalkingController_Masters_Activity.cs",
    r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\SeniorsActivityUnit8\Scripts\Screens\U8_AnuWalkingController_Masters_Activity.cs",
    r"D:\engsnap\MastersActivityUnit6\Assets\Own_Scripts\U8_AnuWalkToDoor.cs",
    r"c:\Users\ASUS\Downloads\MastersActivityUnit6-main\MastersActivityUnit6-main\Assets\Own_Scripts\U8_AnuWalkToDoor.cs"
]

for f in files:
    with open(f, 'r', encoding='utf-8') as fp:
        text = fp.read()
    # Replace any doorPosition vector with (-424f, -70f)
    import re
    text = re.sub(r'new Vector2\(-?\d+(\.\d+)?f,\s*-?\d+(\.\d+)?f\);\s*//?\s*door', 'new Vector2(-424f, -70f);', text)
    text = text.replace('new Vector2(-440f, -70f)', 'new Vector2(-424f, -70f)')
    text = text.replace('new Vector2(-382f, -70f)', 'new Vector2(-424f, -70f)')
    text = text.replace('new Vector2(-300f, 0f)', 'new Vector2(-424f, -70f)')
    with open(f, 'w', encoding='utf-8') as fp:
        fp.write(text)
    print("Updated doorPosition to -424f in:", f)
