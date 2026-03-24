import wave
import struct
import math

def generate_click_wav(filename):
    sample_rate = 44100
    duration = 0.05 # 50ms
    frequency = 880.0
    
    n_samples = int(sample_rate * duration)
    
    with wave.open(filename, 'w') as wav_file:
        wav_file.setnchannels(1)
        wav_file.setsampwidth(2)
        wav_file.setframerate(sample_rate)
        
        for i in range(n_samples):
            # Apply an envelope to avoid clicks at the start/end
            envelope = 1.0
            if i < 100:
                envelope = i / 100.0
            elif i > n_samples - 100:
                envelope = (n_samples - i) / 100.0
                
            value = int(32767.0 * envelope * math.sin(2.0 * math.pi * frequency * i / sample_rate))
            data = struct.pack('<h', value)
            wav_file.writeframesraw(data)

if __name__ == '__main__':
    generate_click_wav('AclsTracker/Resources/Raw/click.wav')
