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
            'anchor_min': [float(x) for x in m_anchor_min.groups()] if m_anchor_min else [0.5, 0.5],
            'anchor_max': [float(x) for x in m_anchor_max.groups()] if m_anchor_max else [0.5, 0.5],
            'pos': [float(x) for x in m_anchored_pos.groups()] if m_anchored_pos else [0.0, 0.0],
            'size': [float(x) for x in m_size_delta.groups()] if m_size_delta else [0.0, 0.0],
            'pivot': [float(x) for x in m_pivot.groups()] if m_pivot else [0.5, 0.5],
            'body': body,
            'file_id': file_id
        }

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

def calc_rect(t, parent_w, parent_h):
    amin = t['anchor_min']
    amax = t['anchor_max']
    pos = t['pos']
    sz = t['size']
    piv = t['pivot']
    
    # if stretch
    w = (amax[0] - amin[0]) * parent_w + sz[0]
    h = (amax[1] - amin[1]) * parent_h + sz[1]
    
    # center pos
    cx = amin[0] * parent_w + pos[0] + (0.5 - piv[0]) * w
    cy = amin[1] * parent_h + pos[1] + (0.5 - piv[1]) * h
    
    left = cx - w / 2
    right = cx + w / 2
    bottom = cy - h / 2
    top = cy + h / 2
    return left, right, bottom, top, w, h

# Test aspect ratios at height=1080
test_resolutions = [
    ("4:3 Tablet/Smartboard", 1440, 1080),
    ("16:10 Laptop/Smartboard", 1728, 1080),
    ("16:9 Standard Screen", 1920, 1080),
    ("19.5:9 Modern Mobile", 2340, 1080),
    ("20:9 Ultra-Wide Mobile", 2400, 1080)
]

print("=== CHECKING RESPONSIVE LAYOUT ACROSS SCREEN SIZES ===")
for screen_name in ["U8_Screen_01_Part1Door", "U8_Screen_02_Part2Inside", "U8_Screen_03_Handwash", "U8_Screen_04_Part3AfterYou", "U8_Screen_05_Part4EmptySoap", "U8_Screen_06_Ending"]:
    print(f"\n--- {screen_name} ---")
    for t_id, t in transforms.items():
        go_id = t['go_id']
        path = get_path(go_id)
        if path.startswith(f"Unit8_Canvas/{screen_name}/"):
            sub_path = path.replace(f"Unit8_Canvas/{screen_name}/", "")
            # Only top level children of screen
            if "/" not in sub_path:
                print(f"Element: {sub_path:<35} | Anchors: min={t['anchor_min']} max={t['anchor_max']} | Pos={t['pos']} | Size={t['size']}")
                for res_name, rw, rh in test_resolutions:
                    l, r, b, top, w, h = calc_rect(t, rw, rh)
                    is_out_x = l < -50 or r > rw + 50
                    is_out_y = b < -50 or top > rh + 50
                    if is_out_x or is_out_y or w < 0 or h < 0:
                        print(f"   [!] ISSUE on {res_name} ({rw}x{rh}): Rect=[L:{l:.0f}, R:{r:.0f}, B:{b:.0f}, T:{top:.0f}], W:{w:.0f}, H:{h:.0f}")
