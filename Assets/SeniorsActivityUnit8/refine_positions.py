import re
import os

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
    elif class_id in ('4', '224'):
        m_go = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', body)
        m_parent = re.search(r'm_Father:\s*\{fileID:\s*(\d+)\}', body)
        transforms[file_id] = {
            'go_id': m_go.group(1) if m_go else None,
            'parent_id': m_parent.group(1) if m_parent else None,
            'file_id': file_id,
            'class_id': class_id
        }
    elif class_id == '114':
        m_go = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', body)
        monobehaviours[file_id] = {'go_id': m_go.group(1) if m_go else None, 'file_id': file_id}

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

new_docs = []
modified_count = 0

for d in docs:
    lines = d.strip().split('\n')
    if not lines or not lines[0]:
        new_docs.append(d)
        continue
    header = lines[0].strip()
    parts = header.split(' &')
    if len(parts) < 2:
        new_docs.append(d)
        continue
    class_id, file_id = parts[0], parts[1]
    body = '\n'.join(lines[1:])
    
    if class_id == '224': # RectTransform
        go_id = transforms.get(file_id, {}).get('go_id')
        path = get_path(go_id)
        
        # Screen 4: WashroomDoor & TrashBin safe area on 4:3
        if path == 'Unit8_Canvas/U8_Screen_04_Part3AfterYou/WashroomDoor':
            body = re.sub(r'm_AnchoredPosition:\s*\{[^}]*\}', 'm_AnchoredPosition: {x: -640, y: -39}', body)
            modified_count += 1
            print(f"Repositioned for safe margin: {path}")
            
        elif path == 'Unit8_Canvas/U8_Screen_04_Part3AfterYou/TrashBinImage':
            body = re.sub(r'm_AnchoredPosition:\s*\{[^}]*\}', 'm_AnchoredPosition: {x: 640, y: -224}', body)
            modified_count += 1
            print(f"Repositioned for safe margin: {path}")

    new_docs.append(lines[0] + '\n' + body + '\n')

with open(scene_path, 'w', encoding='utf-8') as f:
    f.write('--- !u!'.join(new_docs))

print(f"\nSuccessfully refined scene objects for responsive safe-area support!")
