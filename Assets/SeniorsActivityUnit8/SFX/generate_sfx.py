import os
import wave
import math
import struct
import random

SFX_DIR = os.path.dirname(os.path.abspath(__file__))

def create_wav(filename, duration, sample_func, sample_rate=44100, volume=0.7):
    filepath = os.path.join(SFX_DIR, filename)
    num_samples = int(duration * sample_rate)
    frames = bytearray()
    for i in range(num_samples):
        t = float(i) / sample_rate
        sample_val = sample_func(t, duration)
        sample_val = max(-1.0, min(1.0, sample_val)) * volume
        packed_val = struct.pack('<h', int(sample_val * 32767.0))
        frames.extend(packed_val)
    with wave.open(filepath, 'wb') as wav_file:
        wav_file.setnchannels(1)  # Mono
        wav_file.setsampwidth(2)  # 16-bit
        wav_file.setframerate(sample_rate)
        wav_file.writeframes(frames)
    file_size = os.path.getsize(filepath)
    print(f"  -> Generated SFX: {filename} ({duration:.2f}s, {file_size} bytes)")

def main():
    print(f"Generating Unit 8 Audio SFX in: {SFX_DIR}")

    # 1. SFX_Knock (Two polite wooden knocks)
    def knock_func(t, dur):
        decay = math.exp(-35 * (t % 0.3))
        freq = 180 + 40 * math.sin(t * 50)
        return decay * math.sin(2 * math.pi * freq * t)
    create_wav("SFX_Knock.wav", 0.6, knock_func)

    # 2. SFX_BangDoor (Three heavy impatient bangs)
    def bang_func(t, dur):
        period = t % 0.25
        decay = math.exp(-22 * period)
        noise = (random.random() * 2.0 - 1.0) * 0.4
        freq = 90 + 30 * math.sin(period * 40)
        return decay * (math.sin(2 * math.pi * freq * period) + noise)
    create_wav("SFX_BangDoor.wav", 0.8, bang_func)

    # 3. SFX_BoltRattle (Metal rattle refusing to open)
    def rattle_func(t, dur):
        decay = math.exp(-8 * t)
        noise = (random.random() * 2.0 - 1.0)
        metallic = math.sin(2 * math.pi * 1200 * t) * math.sin(2 * math.pi * 850 * t)
        return decay * (0.6 * noise + 0.4 * metallic)
    create_wav("SFX_BoltRattle.wav", 0.6, rattle_func)

    # 4. SFX_BoltClick (Clean metallic lock click)
    def bolt_func(t, dur):
        decay = math.exp(-40 * t)
        click = math.sin(2 * math.pi * 1800 * t) + math.sin(2 * math.pi * 2400 * t)
        return decay * click * 0.5
    create_wav("SFX_BoltClick.wav", 0.25, bolt_func)

    # 5. SFX_Flush (Toilet water flush whoosh)
    def flush_func(t, dur):
        env = math.sin(math.pi * (t / dur))
        noise = (random.random() * 2.0 - 1.0)
        swirl = math.sin(2 * math.pi * (140 + 60 * math.sin(t * 8)) * t)
        return env * (0.7 * noise + 0.3 * swirl)
    create_wav("SFX_Flush.wav", 2.2, flush_func)

    # 6. SFX_TapOn & SFX_TapOff
    def tap_on_func(t, dur):
        env = min(1.0, t * 5)
        noise = (random.random() * 2.0 - 1.0) * 0.5
        bubble = math.sin(2 * math.pi * 600 * t) * 0.3
        return env * (noise + bubble)
    create_wav("SFX_TapOn.wav", 1.2, tap_on_func)

    def tap_off_func(t, dur):
        decay = math.exp(-15 * t)
        squeak = math.sin(2 * math.pi * (1200 - 600 * t) * t) * 0.4
        return decay * squeak
    create_wav("SFX_TapOff.wav", 0.3, tap_off_func)

    # 7. SFX_SoapPump & SFX_SoapEmpty
    def soap_pump_func(t, dur):
        decay = math.exp(-20 * t)
        squish = math.sin(2 * math.pi * 320 * t) + (random.random() * 0.3)
        return decay * squish
    create_wav("SFX_SoapPump.wav", 0.35, soap_pump_func)

    def soap_empty_func(t, dur):
        decay = math.exp(-35 * t)
        hollow = math.sin(2 * math.pi * 480 * t) * 0.8
        return decay * hollow
    create_wav("SFX_SoapEmpty.wav", 0.25, soap_empty_func)

    # 8. SFX_Scrub & SFX_Bubble & SFX_GermOff
    def scrub_func(t, dur):
        env = math.sin(math.pi * (t / dur))
        noise = (random.random() * 2.0 - 1.0) * 0.6
        return env * noise
    create_wav("SFX_Scrub.wav", 0.25, scrub_func)

    def bubble_func(t, dur):
        decay = math.exp(-30 * t)
        freq = 700 + 1200 * t
        return decay * math.sin(2 * math.pi * freq * t)
    create_wav("SFX_Bubble.wav", 0.2, bubble_func)

    def germ_off_func(t, dur):
        decay = math.exp(-12 * t)
        freq = 900 + 400 * math.sin(t * 40)
        return decay * math.sin(2 * math.pi * freq * t)
    create_wav("SFX_GermOff.wav", 0.35, germ_off_func)

    # 9. SFX_TowelPull & SFX_BinDrop & SFX_Wipe
    def towel_pull_func(t, dur):
        env = math.sin(math.pi * (t / dur))
        paper = (random.random() * 2.0 - 1.0) * 0.5
        return env * paper
    create_wav("SFX_TowelPull.wav", 0.4, towel_pull_func)

    def bin_drop_func(t, dur):
        decay = math.exp(-20 * t)
        thud = math.sin(2 * math.pi * 140 * t) * 0.7 + (random.random() * 0.2)
        return decay * thud
    create_wav("SFX_BinDrop.wav", 0.3, bin_drop_func)

    def wipe_func(t, dur):
        env = math.sin(math.pi * (t / dur))
        squeak = math.sin(2 * math.pi * (1600 + 400 * math.sin(t * 30)) * t) * 0.25
        noise = (random.random() * 2.0 - 1.0) * 0.35
        return env * (squeak + noise)
    create_wav("SFX_Wipe.wav", 0.35, wipe_func)

    # 10. SFX_Slip (Shoe skid / scrabble catch)
    def slip_func(t, dur):
        decay = math.exp(-8 * t)
        skid = (random.random() * 2.0 - 1.0) * 0.7 * math.sin(2 * math.pi * 350 * t)
        thud = math.exp(-25 * (t - 0.25)) * math.sin(2 * math.pi * 120 * t) if t > 0.25 else 0
        return decay * skid + thud
    create_wav("SFX_Slip.wav", 0.6, slip_func)

    # 11. AMB_Washroom (Gentle tiled room tone with echo drip)
    def amb_washroom_func(t, dur):
        room_tone = (random.random() * 2.0 - 1.0) * 0.05
        drip_t = t % 2.0
        drip = math.exp(-40 * drip_t) * math.sin(2 * math.pi * 1400 * drip_t) * 0.2 if drip_t < 0.15 else 0
        return room_tone + drip
    create_wav("AMB_Washroom.wav", 4.0, amb_washroom_func, volume=0.3)

    # 12. MUS_HandwashSong (20s joyful melody for handwashing)
    def song_func(t, dur):
        notes = [261.63, 293.66, 329.63, 349.23, 392.00, 440.00, 493.88, 523.25]
        note_idx = int(t * 2) % len(notes)
        freq = notes[note_idx]
        lead = math.sin(2 * math.pi * freq * t) * 0.3
        bass = math.sin(2 * math.pi * (freq * 0.5) * t) * 0.2
        return lead + bass
    create_wav("MUS_HandwashSong.wav", 20.0, song_func, volume=0.45)

    # 13. SFX_Star (Bright chiming glockenspiel sparkle)
    def star_func(t, dur):
        decay1 = math.exp(-6 * t)
        decay2 = math.exp(-8 * max(0, t - 0.08)) if t >= 0.08 else 0
        decay3 = math.exp(-10 * max(0, t - 0.16)) if t >= 0.16 else 0
        s1 = math.sin(2 * math.pi * 1318.5 * t) * decay1  # E6
        s2 = math.sin(2 * math.pi * 1567.9 * t) * decay2  # G6
        s3 = math.sin(2 * math.pi * 2093.0 * t) * decay3  # C7
        return (s1 + s2 + s3) * 0.4
    create_wav("SFX_Star.wav", 1.2, star_func, volume=0.7)

    # 14. SFX_Clap (Applause / clapping bursts)
    def clap_func(t, dur):
        env = math.exp(-12 * (t % 0.12))
        noise = (random.random() * 2.0 - 1.0) * 0.6
        return env * noise
    create_wav("SFX_Clap.wav", 1.5, clap_func, volume=0.6)

    # 15. SFX_Confetti (Party popper pop and sparkle)
    def confetti_func(t, dur):
        pop = math.exp(-35 * t) * math.sin(2 * math.pi * 220 * t)
        sparkle = math.exp(-4 * t) * (random.random() * 2.0 - 1.0) * 0.3
        return pop + sparkle
    create_wav("SFX_Confetti.wav", 0.8, confetti_func, volume=0.7)

    # 16. SFX_Sparkle (High frequency magical chime)
    def sparkle_func(t, dur):
        decay = math.exp(-5 * t)
        freq = 2400 + 800 * math.sin(t * 30)
        return decay * math.sin(2 * math.pi * freq * t) * 0.35
    create_wav("SFX_Sparkle.wav", 0.9, sparkle_func, volume=0.6)

    # 17. MUS_Win (4s celebratory fanfare victory tune)
    def win_func(t, dur):
        melody_notes = [523.25, 659.25, 783.99, 1046.50]  # C5, E5, G5, C6
        idx = min(len(melody_notes) - 1, int(t * 2))
        freq = melody_notes[idx]
        env = math.exp(-2.5 * (t % 0.5)) if t < 2.0 else math.exp(-1.2 * (t - 2.0))
        lead = math.sin(2 * math.pi * freq * t) * env
        bass = math.sin(2 * math.pi * (freq * 0.5) * t) * 0.3
        return (lead + bass) * 0.45
    create_wav("MUS_Win.wav", 4.0, win_func, volume=0.6)

    # 18. SFX_MeeraGasp (Meera startle gasp)
    def mee_gasp_func(t, dur):
        env = math.sin(math.pi * (t / dur))
        gasp = math.sin(2 * math.pi * (450 + 200 * t) * t) * 0.4 + ((random.random() * 2.0 - 1.0) * 0.2)
        return env * gasp
    create_wav("VO_U8_MEE_2.wav", 0.4, mee_gasp_func, volume=0.6)

    print("All Unit 8 SFX WAV files generated successfully!")

if __name__ == "__main__":
    main()
