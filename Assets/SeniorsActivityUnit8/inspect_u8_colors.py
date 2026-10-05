import re

scene_path = r'Assets/SeniorsActivityUnit8/Scenes/SeniorsActivity_unit8.unity'
with open(scene_path, 'r', encoding='utf-8') as f:
    content = f.read()

docs = content.split('--- !u!')

game_objects = {}
transforms = {}
images = {}
texts = {}

for d in docs:
    lines = d.strip().split('\n')
    if not lines or not lines[0]:
        continue
    header = lines[0].strip()
    parts = header.split(' &')
    if len(parts) < 2:
        continue
    class_id, file_id = parts[0], parts[1]
    body = '\n'.join(lines[1:])
    
    if class_id == '1': # GameObject
        m_name = re.search(r'm_Name:\s*(.*)', body)
        name = m_name.group(1).strip() if m_name else ''
        game_objects[file_id] = name
    elif class_id in ('4', '224'): # RectTransform
        m_go = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', body)
        m_parent = re.search(r'm_Father:\s*\{fileID:\s*(\d+)\}', body)
        m_size = re.search(r'm_SizeDelta:\s*\{x:\s*([\d\.-]+),\s*y:\s*([\d\.-]+)\}', body)
        transforms[file_id] = {
            'go_id': m_go.group(1) if m_go else None,
            'parent_id': m_parent.group(1) if m_parent else None,
            'size': m_size.groups() if m_size else (0, 0)
        }
    elif class_id == '114':
        if 'm_Sprite:' in body:
            m_go = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', body)
            m_col = re.search(r'm_Color:\s*\{r:\s*([\d\.]+),\s*g:\s*([\d\.]+),\s*b:\s*([\d\.]+),\s*a:\s*([\d\.]+)\}', body)
            m_type = re.search(r'm_Type:\s*(\d+)', body)
            images[file_id] = {
                'go_id': m_go.group(1) if m_go else None,
                'color': m_col.groups() if m_col else None,
                'type': m_type.group(1) if m_type else '0',
                'body': body
            }

def get_path(go_id):
    if not go_id or go_id not in game_objects:
        return ""
    name = game_objects[go_id]
    for t_id, t in transforms.items():
        if t['go_id'] == go_id:
            if t['parent_id'] and t['parent_id'] != '0':
                parent_go_id = transforms.get(t['parent_id'], {}).get('go_id')
                parent_path = get_path(parent_go_id)
                return f"{parent_path}/{name}" if parent_path else name
            return name
    return name

print(f"{'UI Element Path':<70} | {'Type':<6} | {'Color (RGBA)':<30} | Size")
print("-" * 125)
for img_id, img in images.items():
    go_id = img['go_id']
    path = get_path(go_id)
    if any(k in path.lower() for k in ['btn', 'button', 'card', 'title', 'panel', 'box', 'bg', 'bar', 'dialog', 'header', 'footer', 'choice', 'banner', 'prompt', 'container', 'pill', 'badge', 'modal']):
        col_str = f"({float(img['color'][0]):.2f}, {float(img['color'][1]):.2f}, {float(img['color'][2]):.2f}, {float(img['color'][3]):.2f})" if img['color'] else "N/A"
        # find size
        size_str = "N/A"
        for t_id, t in transforms.items():
            if t['go_id'] == go_id:
                size_str = f"{t['size'][0]}x{t['size'][1]}"
                break
        print(f"{path:<70} | {img['type']:<6} | {col_str:<30} | {size_str}")
