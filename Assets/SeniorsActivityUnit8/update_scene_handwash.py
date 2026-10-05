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
        game_objects[file_id] = {'name': name, 'file_id': file_id}
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
    
    if class_id == '1': # GameObject
        go_id = file_id
        path = get_path(go_id)
        
        # Deactivate TimerBadge and TimerText in Scene Hierarchy
        if path in ('Unit8_Canvas/U8_Screen_03_Handwash/TimerBadge', 'Unit8_Canvas/U8_Screen_03_Handwash/TimerText'):
            body = re.sub(r'm_IsActive:\s*\d+', 'm_IsActive: 0', body)
            modified_count += 1
            print(f"Deactivated in Scene: {path}")

    elif class_id == '114': # MonoBehaviour
        go_id = monobehaviours.get(file_id, {}).get('go_id')
        path = get_path(go_id)
        
        # Update Screen 3 Header text
        if path == 'Unit8_Canvas/U8_Screen_03_Handwash/HeaderBar/TitleText':
            if 'm_text:' in body:
                body = re.sub(r'm_text:\s*.*', "m_text: '<color=#FFD700><b>UNIT 8: HANDWASHING</b></color> - <color=#FFFFFF><b>Song & Scrub</b></color>'", body)
                modified_count += 1
                print(f"Updated TitleText: {path}")

    new_docs.append(lines[0] + '\n' + body + '\n')

with open(scene_path, 'w', encoding='utf-8') as f:
    f.write('--- !u!'.join(new_docs))

print(f"\nSuccessfully updated {modified_count} elements in {scene_path}!")
