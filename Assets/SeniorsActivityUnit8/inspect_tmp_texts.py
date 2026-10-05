import re

scene_path = r'Assets/SeniorsActivityUnit8/Scenes/SeniorsActivity_unit8.unity'
with open(scene_path, 'r', encoding='utf-8') as f:
    content = f.read()

docs = content.split('--- !u!')

game_objects = {}
transforms = {}
tmp_texts = {}

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
    elif class_id in ('4', '224'):
        m_go = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', body)
        m_parent = re.search(r'm_Father:\s*\{fileID:\s*(\d+)\}', body)
        transforms[file_id] = {
            'go_id': m_go.group(1) if m_go else None,
            'parent_id': m_parent.group(1) if m_parent else None,
        }
    elif class_id == '114':
        if 'm_text:' in body:
            m_go = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', body)
            m_text = re.search(r'm_text:\s*(.*)', body)
            m_size = re.search(r'm_fontSize:\s*([\d\.]+)', body)
            m_auto = re.search(r'm_enableAutoSizing:\s*(\d+)', body)
            m_wrap = re.search(r'm_enableWordWrapping:\s*(\d+)', body)
            m_min = re.search(r'm_fontSizeMin:\s*([\d\.]+)', body)
            m_max = re.search(r'm_fontSizeMax:\s*([\d\.]+)', body)
            tmp_texts[file_id] = {
                'go_id': m_go.group(1) if m_go else None,
                'text': m_text.group(1) if m_text else '',
                'fontSize': m_size.group(1) if m_size else 'N/A',
                'auto': m_auto.group(1) if m_auto else '0',
                'wrap': m_wrap.group(1) if m_wrap else '0',
                'min': m_min.group(1) if m_min else 'N/A',
                'max': m_max.group(1) if m_max else 'N/A',
                'body': body,
                'file_id': file_id
            }

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

print(f"Total TextMeshPro elements: {len(tmp_texts)}")
print(f"{'Text Path':<65} | {'Size':<6} | {'Auto':<5} | {'Wrap':<5} | Sample Content")
print("-" * 115)
for t_id, t in tmp_texts.items():
    go_id = t['go_id']
    path = get_path(go_id)
    text_sample = t['text'][:35]
    print(f"{path:<65} | {t['fontSize']:<6} | {t['auto']:<5} | {t['wrap']:<5} | {text_sample}")
