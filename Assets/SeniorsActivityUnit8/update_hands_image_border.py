import re

scene_path = r'Assets/SeniorsActivityUnit8/Scenes/SeniorsActivity_unit8.unity'
with open(scene_path, 'r', encoding='utf-8') as f:
    content = f.read()

docs = content.split('--- !u!')

new_docs = []
modified = False

for d in docs:
    if ' &2107319197' in d.split('\n')[0]:
        # HandsImage Image component
        d = re.sub(r'm_Color:\s*\{[^}]*\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}', d)
        d = re.sub(r'm_Sprite:\s*\{[^}]*\}', 'm_Sprite: {fileID: 21300000, guid: 8312f609543c2134e8b6f5da9d5c3908, type: 3}', d)
        d = re.sub(r'm_PreserveAspect:\s*\d+', 'm_PreserveAspect: 1', d)
        modified = True
        print("Updated HandsImage Image component (2107319197) to pure white and hand wash border.png")
    
    new_docs.append(d)

if modified:
    with open(scene_path, 'w', encoding='utf-8') as f:
        f.write('--- !u!'.join(new_docs))
    print("Scene saved successfully.")
