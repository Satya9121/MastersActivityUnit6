import os
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
        game_objects[file_id] = {
            'name': name,
            'components': m_comp,
            'file_id': file_id
        }
    elif class_id in ('4', '224'): # Transform / RectTransform
        m_go = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', body)
        m_parent = re.search(r'm_Father:\s*\{fileID:\s*(\d+)\}', body)
        m_children = re.findall(r'-\s*\{fileID:\s*(\d+)\}', body)
        transforms[file_id] = {
            'go_id': m_go.group(1) if m_go else None,
            'parent_id': m_parent.group(1) if m_parent else None,
            'children': m_children,
            'is_rect': class_id == '224'
        }
    elif class_id == '114': # MonoBehaviour
        m_go = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', body)
        m_script = re.search(r'm_Script:\s*\{fileID:\s*(\d+),\s*guid:\s*([a-f0-9]+)', body)
        m_sprite = re.search(r'm_Sprite:\s*\{fileID:\s*(-?\d+)(?:,\s*guid:\s*([a-f0-9]+))?', body)
        m_color = re.search(r'm_Color:\s*\{r:\s*([\d\.]+),\s*g:\s*([\d\.]+),\s*b:\s*([\d\.]+),\s*a:\s*([\d\.]+)\}', body)
        m_text = re.search(r'm_text:\s*(.*)', body)
        
        monobehaviours[file_id] = {
            'go_id': m_go.group(1) if m_go else None,
            'script_guid': m_script.group(2) if m_script else None,
            'sprite_guid': m_sprite.group(2) if (m_sprite and m_sprite.group(2)) else None,
            'sprite_file_id': m_sprite.group(1) if m_sprite else None,
            'color': m_color.groups() if m_color else None,
            'text': m_text.group(1) if m_text else None,
            'body': body
        }

print(f"Total GameObjects: {len(game_objects)}")
print(f"Total Transforms: {len(transforms)}")
print(f"Total MonoBehaviours: {len(monobehaviours)}")

# Build hierarchy
def get_path(go_id):
    if not go_id or go_id not in game_objects:
        return ""
    name = game_objects[go_id]['name']
    # Find transform for this go
    for t_id, t in transforms.items():
        if t['go_id'] == go_id:
            if t['parent_id'] and t['parent_id'] != '0':
                parent_go_id = transforms.get(t['parent_id'], {}).get('go_id')
                parent_path = get_path(parent_go_id)
                return f"{parent_path}/{name}" if parent_path else name
            return name
    return name

print("\n--- ALL UI ELEMENTS (Buttons, Cards, Titles, Panels, Prompts, Choices) ---")
for mb_id, mb in monobehaviours.items():
    go_id = mb['go_id']
    if not go_id or go_id not in game_objects:
        continue
    go = game_objects[go_id]
    name = go['name']
    path = get_path(go_id)
    
    is_image = 'm_Sprite:' in mb['body']
    is_button = 'm_OnClick:' in mb['body']
    is_tmp = 'm_text:' in mb['body'] or 'm_fontAsset:' in mb['body']
    
    if is_button:
        print(f"[BUTTON] Path: {path} (Sprite: {mb.get('sprite_guid')})")
    elif is_image:
        if any(k in path.lower() for k in ['btn', 'button', 'card', 'title', 'panel', 'box', 'bg', 'bar', 'dialog', 'header', 'footer', 'choice', 'banner', 'prompt', 'container', 'pill', 'badge', 'modal', 'instruction', 'bubble', 'timer', 'star']):
            print(f"[IMAGE/PANEL/CARD/TITLE] Path: {path} (Sprite: {mb.get('sprite_guid')}, FileID: {mb.get('sprite_file_id')})")
