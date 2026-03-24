using System;
using System.IO;

class Program {
    static void Main() {
        int sampleRate = 44100;
        int durationMs = 50;
        double frequency = 880.0;
        int nSamples = (sampleRate * durationMs) / 1000;
        
        using (var fs = new FileStream("AclsTracker/Resources/Raw/click.wav", FileMode.Create))
        using (var bw = new BinaryWriter(fs)) {
            // RIFF header
            bw.Write(new[] { 'R', 'I', 'F', 'F' });
            bw.Write(36 + nSamples * 2);
            bw.Write(new[] { 'W', 'A', 'V', 'E' });
            // fmt subchunk
            bw.Write(new[] { 'f', 'm', 't', ' ' });
            bw.Write(16);
            bw.Write((short)1); // PCM
            bw.Write((short)1); // channels
            bw.Write(sampleRate);
            bw.Write(sampleRate * 2);
            bw.Write((short)2);
            bw.Write((short)16);
            // data subchunk
            bw.Write(new[] { 'd', 'a', 't', 'a' });
            bw.Write(nSamples * 2);
            
            for (int i = 0; i < nSamples; i++) {
                double envelope = 1.0;
                if (i < 100) envelope = i / 100.0;
                else if (i > nSamples - 100) envelope = (nSamples - i) / 100.0;
                
                short val = (short)(32767.0 * envelope * Math.Sin(2.0 * Math.PI * frequency * i / sampleRate));
                bw.Write(val);
            }
        }
    }
}
