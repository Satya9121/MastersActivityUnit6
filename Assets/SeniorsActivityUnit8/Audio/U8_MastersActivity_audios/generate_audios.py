import asyncio
import edge_tts
import os

# Voice mappings supported by Edge-TTS
VOICE_TEACHER = "en-IN-NeerjaNeural"
VOICE_ANU = "en-IN-NeerjaNeural"
VOICE_MEERA = "en-IN-NeerjaNeural"
VOICE_STUDENT_INSIDE = "en-IN-NeerjaNeural"

audio_items = [
    # 1. Teacher / Narrator
    {"file": "Anu needs the washroom.mp3", "text": "Anu needs the washroom.", "voice": VOICE_TEACHER},
    {"file": "The door is closed What should Anu do.mp3", "text": "The door is closed. What should Anu do?", "voice": VOICE_TEACHER},
    {"file": "Knock and wait.mp3", "text": "Knock, and wait.", "voice": VOICE_TEACHER},
    {"file": "Now what next.mp3", "text": "Now, what next?", "voice": VOICE_TEACHER},
    {"file": "Do not forget to flush.mp3", "text": "Do not forget to flush.", "voice": VOICE_TEACHER},
    {"file": "Now wash your hands Keep tapping.mp3", "text": "Press and hold Wash Hands to keep washing your hands.", "voice": VOICE_TEACHER},
    {"file": "Keep going.mp3", "text": "Keep going!", "voice": VOICE_TEACHER},
    {"file": "All clean One star.mp3", "text": "All clean! One star.", "voice": VOICE_TEACHER},
    {"file": "Turn the tap off.mp3", "text": "Turn the tap off.", "voice": VOICE_TEACHER},
    {"file": "Towel in the bin.mp3", "text": "Towel in the bin.", "voice": VOICE_TEACHER},
    {"file": "Wipe the sink.mp3", "text": "Wipe the sink.", "voice": VOICE_TEACHER},
    {"file": "Here comes Meera.mp3", "text": "Here comes Meera.", "voice": VOICE_TEACHER},
    {"file": "Oh dear Shall we try again.mp3", "text": "Oh dear. Shall we try again?", "voice": VOICE_TEACHER},
    {"file": "The soap is finished What should Meera do.mp3", "text": "The soap is finished. What should Meera do?", "voice": VOICE_TEACHER},
    {"file": "Three stars Ready for the next person.mp3", "text": "Three stars! Ready for the next person!", "voice": VOICE_TEACHER},
    {"file": "Would the next person be happy.mp3", "text": "Would the next person be happy?", "voice": VOICE_TEACHER},

    # 2. Characters
    {"file": "Sorry.mp3", "text": "Sorry!", "voice": VOICE_ANU},
    {"file": "Maam the soap is finished.mp3", "text": "Ma'am, the soap is finished.", "voice": VOICE_MEERA},
    {"file": "Just a minute.mp3", "text": "Just a minute!", "voice": VOICE_STUDENT_INSIDE},
]

async def generate():
    total = len(audio_items)
    for index, item in enumerate(audio_items, 1):
        filename = item["file"]
        text = item["text"]
        voice = item["voice"]

        if os.path.exists(filename):
            file_size = os.path.getsize(filename)
            if file_size > 2000:
                print(f"[{index}/{total}] SKIP (Already exists, size={file_size} bytes): {filename}")
                continue
            else:
                try:
                    os.remove(filename)
                except Exception:
                    pass

        print(f"[{index}/{total}] Generating: {filename} ({voice})...")
        try:
            communicate = edge_tts.Communicate(text, voice)
            await communicate.save(filename)
            new_size = os.path.getsize(filename)
            print(f"  -> SUCCESS: {filename} ({new_size} bytes)")
        except Exception as e:
            print(f"  -> ERROR generating {filename}: {e}")

if __name__ == "__main__":
    asyncio.run(generate())
