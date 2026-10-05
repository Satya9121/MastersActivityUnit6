import re

scene_path = r'Assets/SeniorsActivityUnit8/Scenes/SeniorsActivity_unit8.unity'
with open(scene_path, 'r', encoding='utf-8') as f:
    content = f.read()

docs = content.split('--- !u!')

game_objects = {}
transforms = {}
images = {}
buttons = {}

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
        transforms[file_id] = {
            'go_id': m_go.group(1) if m_go else None,
            'parent_id': m_parent.group(1) if m_parent else None,
        }
    elif class_id == '114':
        m_go = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', body)
        if 'm_Sprite:' in body:
            m_sprite = re.search(r'm_Sprite:\s*\{fileID:\s*(-?\d+)(?:,\s*guid:\s*([a-f0-9]+))?', body)
            m_type = re.search(r'm_Type:\s*(\d+)', body)
            images[file_id] = {
                'go_id': m_go.group(1) if m_go else None,
                'sprite_guid': m_sprite.group(2) if (m_sprite and m_sprite.group(2)) else None,
                'sprite_file_id': m_sprite.group(1) if m_sprite else None,
                'type': m_type.group(1) if m_type else '0',
                'file_id': file_id,
                'body': body
            }
        if 'm_OnClick:' in body:
            buttons[file_id] = {'go_id': m_go.group(1) if m_go else None}

def get_path(go_id):
    if not go_id or go_id not in game_objects:
        return ''
    name = game_objects[go_id]
    for t_id, t in transforms.items():
        if t['go_id'] == go_id:
            if t['parent_id'] and t['parent_id'] != '0':
                parent_go_id = transforms.get(t['parent_id'], {}).get('go_id')
                parent_path = get_path(parent_go_id)
                return f'{parent_path}/{name}' if parent_path else name
            return name
    return name

print('--- CHECKING BUTTON IMAGES ---')
for b_id, b in buttons.items():
    go_id = b['go_id']
    path = get_path(go_id)
    img_found = False
    for i_id, img in images.items():
        if img['go_id'] == go_id:
            img_found = True
            t_str = "Sliced" if img['type'] == '1' else f"Type {img['type']}"
            guid_str = img['sprite_guid'] if img['sprite_guid'] else "None"
            print(f"Button: {path:<70} | {t_str:<8} | GUID: {guid_str}")
    if not img_found:
        print(f"Button: {path:<70} | NO IMAGE COMPONENT FOUND!")

print('\n--- CHECKING CARDS / TITLES / HEADERS / PROMPTS / BADGES ---')
for i_id, img in images.items():
    go_id = img['go_id']
    path = get_path(go_id)
    if any(k in path.lower() for k in ['card', 'title', 'header', 'prompt', 'feedback', 'badge', 'panel', 'container', 'choice']):
        t_str = "Sliced" if img['type'] == '1' else f"Type {img['type']}"
        guid_str = img['sprite_guid'] if img['sprite_guid'] else "None"
        print(f"UI Card/Title: {path:<70} | {t_str:<8} | GUID: {guid_str}")
