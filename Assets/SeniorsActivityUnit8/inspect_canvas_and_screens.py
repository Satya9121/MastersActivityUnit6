import re

scene_path = r'Assets/SeniorsActivityUnit8/Scenes/SeniorsActivity_unit8.unity'
with open(scene_path, 'r', encoding='utf-8') as f:
    content = f.read()

docs = content.split('--- !u!')

game_objects = {}
transforms = {}
monobehaviours = {}

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
        m_comp = re.findall(r'component:\s*\{fileID:\s*(\d+)\}', body)
        game_objects[file_id] = {'name': name, 'components': m_comp, 'file_id': file_id}
    elif class_id in ('4', '224'): # RectTransform / Transform
        m_go = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', body)
        m_parent = re.search(r'm_Father:\s*\{fileID:\s*(\d+)\}', body)
        m_anchor_min = re.search(r'm_AnchorMin:\s*\{x:\s*([\d\.-]+),\s*y:\s*([\d\.-]+)\}', body)
        m_anchor_max = re.search(r'm_AnchorMax:\s*\{x:\s*([\d\.-]+),\s*y:\s*([\d\.-]+)\}', body)
        m_anchored_pos = re.search(r'm_AnchoredPosition:\s*\{x:\s*([\d\.-]+),\s*y:\s*([\d\.-]+)\}', body)
        m_size_delta = re.search(r'm_SizeDelta:\s*\{x:\s*([\d\.-]+),\s*y:\s*([\d\.-]+)\}', body)
        m_pivot = re.search(r'm_Pivot:\s*\{x:\s*([\d\.-]+),\s*y:\s*([\d\.-]+)\}', body)
        
        transforms[file_id] = {
            'go_id': m_go.group(1) if m_go else None,
            'parent_id': m_parent.group(1) if m_parent else None,
            'anchor_min': m_anchor_min.groups() if m_anchor_min else None,
            'anchor_max': m_anchor_max.groups() if m_anchor_max else None,
            'pos': m_anchored_pos.groups() if m_anchored_pos else None,
            'size': m_size_delta.groups() if m_size_delta else None,
            'pivot': m_pivot.groups() if m_pivot else None,
            'body': body,
            'file_id': file_id
        }
    elif class_id == '114':
        m_go = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', body)
        monobehaviours[file_id] = {'go_id': m_go.group(1) if m_go else None, 'body': body, 'file_id': file_id}

# Find Canvas
print("=== CANVAS INSPECTION ===")
for go_id, go in game_objects.items():
    if "canvas" in go['name'].lower():
        print(f"Canvas GO: {go['name']} (ID: {go_id})")
        for comp_id in go['components']:
            if comp_id in monobehaviours:
                mb = monobehaviours[comp_id]
                print(f"  MonoBehaviour ID: {comp_id}")
                for line in mb['body'].split('\n')[:20]:
                    if any(k in line for k in ['m_UiScaleMode', 'm_ReferenceResolution', 'm_ScreenMatchMode', 'm_MatchWidthOrHeight', 'm_RenderMode', 'm_Script']):
                        print(f"    {line.strip()}")

def get_path(go_id):
    if not go_id or go_id not in game_objects:
        return ''
    name = game_objects[go_id]['name']
    for t_id, t in transforms.items():
        if t['go_id'] == go_id:
            if t['parent_id'] and t['parent_id'] != '0':
                parent_go_id = transforms.get(t['parent_id'], {}).get('go_id')
                parent_path = get_path(parent_go_id)
                return f'{parent_path}/{name}' if parent_path else name
            return name
    return name

print("\n=== SCREEN ROOT & MAJOR CONTAINER RECTTRANSFORMS ===")
for t_id, t in transforms.items():
    go_id = t['go_id']
    path = get_path(go_id)
    if any(k in path.lower() for k in ['u8_screen_', 'background', 'headerbar', 'promptcard', 'feedbackcard', 'choicecontainer', 'btnscardcontainer', 'starscontainer', 'titlecard']):
        print(f"{path:<70} | Anchors: min={t['anchor_min']} max={t['anchor_max']} | Pos={t['pos']} | Size={t['size']} | Pivot={t['pivot']}")
