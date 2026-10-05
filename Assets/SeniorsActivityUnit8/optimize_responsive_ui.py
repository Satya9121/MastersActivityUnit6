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

# Modify YAML docs
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
        
        # 1. Standardize HeaderBar RectTransforms
        if path.endswith('/HeaderBar') and 'Unit8_Canvas' in path:
            body = re.sub(r'm_AnchorMin:\s*\{[^}]*\}', 'm_AnchorMin: {x: 0.5, y: 1}', body)
            body = re.sub(r'm_AnchorMax:\s*\{[^}]*\}', 'm_AnchorMax: {x: 0.5, y: 1}', body)
            body = re.sub(r'm_AnchoredPosition:\s*\{[^}]*\}', 'm_AnchoredPosition: {x: 0, y: -15}', body)
            body = re.sub(r'm_SizeDelta:\s*\{[^}]*\}', 'm_SizeDelta: {x: 1200, y: 75}', body)
            body = re.sub(r'm_Pivot:\s*\{[^}]*\}', 'm_Pivot: {x: 0.5, y: 1}', body)
            modified_count += 1
            print(f"Standardized HeaderBar RectTransform: {path}")

        # 2. Standardize Background RectTransforms
        elif path.endswith('/Background') and 'Unit8_Canvas' in path:
            body = re.sub(r'm_AnchorMin:\s*\{[^}]*\}', 'm_AnchorMin: {x: 0, y: 0}', body)
            body = re.sub(r'm_AnchorMax:\s*\{[^}]*\}', 'm_AnchorMax: {x: 1, y: 1}', body)
            body = re.sub(r'm_AnchoredPosition:\s*\{[^}]*\}', 'm_AnchoredPosition: {x: 0, y: 0}', body)
            body = re.sub(r'm_SizeDelta:\s*\{[^}]*\}', 'm_SizeDelta: {x: 0, y: 0}', body)
            body = re.sub(r'm_Pivot:\s*\{[^}]*\}', 'm_Pivot: {x: 0.5, y: 0.5}', body)
            modified_count += 1
            print(f"Standardized Background Full-Stretch: {path}")

    elif class_id == '114': # MonoBehaviour (e.g. TextMeshProUGUI, CanvasScaler)
        go_id = monobehaviours.get(file_id, {}).get('go_id')
        path = get_path(go_id)
        
        # 3. CanvasScaler
        if 'm_UiScaleMode:' in body and 'm_ReferenceResolution:' in body:
            body = re.sub(r'm_UiScaleMode:\s*\d+', 'm_UiScaleMode: 1', body)
            body = re.sub(r'm_ReferenceResolution:\s*\{[^}]*\}', 'm_ReferenceResolution: {x: 1920, y: 1080}', body)
            body = re.sub(r'm_ScreenMatchMode:\s*\d+', 'm_ScreenMatchMode: 0', body)
            body = re.sub(r'm_MatchWidthOrHeight:\s*[\d\.]+', 'm_MatchWidthOrHeight: 0.5', body)
            modified_count += 1
            print(f"Ensured CanvasScaler: {path} (1920x1080, match 0.5)")
            
        # 4. TextMeshPro Auto-Sizing
        elif 'm_text:' in body and 'm_fontSize:' in body:
            # Enable auto-sizing so text scales seamlessly from mobile to smartboards
            m_sz = re.search(r'm_fontSize:\s*([\d\.]+)', body)
            base_size = float(m_sz.group(1)) if m_sz else 30.0
            
            min_size = max(14.0, base_size * 0.55)
            max_size = base_size * 1.1
            
            body = re.sub(r'm_enableAutoSizing:\s*\d+', 'm_enableAutoSizing: 1', body)
            body = re.sub(r'm_fontSizeMin:\s*[\d\.]+', f'm_fontSizeMin: {min_size:.1f}', body)
            body = re.sub(r'm_fontSizeMax:\s*[\d\.]+', f'm_fontSizeMax: {max_size:.1f}', body)
            body = re.sub(r'm_enableWordWrapping:\s*\d+', 'm_enableWordWrapping: 1', body)
            modified_count += 1
            print(f"Enabled TMP Auto-Sizing on {path} (Base: {base_size}, Min: {min_size:.1f}, Max: {max_size:.1f})")

    new_docs.append(lines[0] + '\n' + body + '\n')

with open(scene_path, 'w', encoding='utf-8') as f:
    f.write('--- !u!'.join(new_docs))

print(f"\nSuccessfully optimized {modified_count} components for responsive multi-screen scaling in {scene_path}!")
